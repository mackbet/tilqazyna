using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

#if UNITY_IOS
using Apple.GameKit;
#endif

public class AppleLoginManager : MonoBehaviour
{
    public static AppleLoginManager Instance { get; private set; }

    #region Настройки
    [Header("Настройки сцены")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private bool autoLoadSceneOnLogin = true;
    [SerializeField] private GameObject curtain;
    #endregion

    #region События
    public event Action<string> OnLoginSuccess;
    public event Action<string> OnLoginFailed;
    public event Action OnLogoutSuccess;
    #endregion

    #region Состояние
    private bool isAuthenticated = false;
    private bool isGameCenterAuthenticated = false;

#if UNITY_IOS
    private string m_Signature;
    private string m_TeamPlayerId;
    private string m_PublicKeyUrl;
    private string m_Salt;
    private ulong m_Timestamp;
#endif
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async void Start()
    {
        await InitializeUnityServices();
        TryAutoLoginGameCenter();
    }
    #endregion

    #region Инициализация
    private async Task InitializeUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            Debug.Log("[Auth-Apple] Инициализация Unity Services...");
            await UnityServices.InitializeAsync();
            Debug.Log("[Auth-Apple] Unity Services инициализированы");
        }
    }
    #endregion

    #region Автоматический вход через Game Center
    private async void TryAutoLoginGameCenter()
    {
#if UNITY_IOS
        Debug.Log("[Auth-Apple] Попытка автоматического входа через Game Center...");

        try
        {
            await GKLocalPlayer.Authenticate();
            var localPlayer = GKLocalPlayer.Local;

            if (!localPlayer.IsAuthenticated)
            {
                Debug.Log("[Auth-Apple] Автовход Game Center не удался.");
                if (curtain != null) curtain.SetActive(false);
                return;
            }

            Debug.Log("[Auth-Apple] ========== GAME CENTER СЕССИЯ ==========");
            Debug.Log($"[Auth-Apple] Имя: {localPlayer.DisplayName}");
            Debug.Log("[Auth-Apple] =========================================");

            isGameCenterAuthenticated = true;
            await FetchGameCenterCredentials();
            SignInWithGameCenter(tryLinkGuest: false);
        }
        catch (Exception e)
        {
            Debug.Log($"[Auth-Apple] Автовход Game Center не удался: {e.Message}");
            if (curtain != null) curtain.SetActive(false);
        }
#else
        Debug.Log("[Auth-Apple] Платформа не iOS. Ожидание выбора способа авторизации.");
        if (curtain != null) curtain.SetActive(false);
#endif
    }
    #endregion

    #region Game Center
#if UNITY_IOS
    private async Task FetchGameCenterCredentials()
    {
        var localPlayer = GKLocalPlayer.Local;
        var fetchResult = await localPlayer.FetchItems();

        m_Signature   = Convert.ToBase64String(fetchResult.GetSignature());
        m_Salt        = Convert.ToBase64String(fetchResult.GetSalt());
        m_PublicKeyUrl = fetchResult.PublicKeyUrl;
        m_Timestamp   = fetchResult.Timestamp;
        m_TeamPlayerId = localPlayer.TeamPlayerId; // ← берётся с localPlayer, не с fetchResult

        Debug.Log("[Auth-Apple] Game Center credentials получены");
        Debug.Log($"[Auth-Apple] TeamPlayerId: {m_TeamPlayerId}");
    }

    /// <summary>
    /// Ручной вход через Game Center.
    /// </summary>
    public async void LoginGameCenter()
    {
        Debug.Log("[Auth-Apple] Попытка входа через Game Center...");

        try
        {
            await GKLocalPlayer.Authenticate();
            var localPlayer = GKLocalPlayer.Local;

            if (!localPlayer.IsAuthenticated)
            {
                Debug.LogWarning("[Auth-Apple] Вход в Game Center не удался");
                OnLoginFailed?.Invoke("Game Center вход не удался");
                return;
            }

            Debug.Log("[Auth-Apple] Game Center: вход успешен");
            Debug.Log($"[Auth-Apple] Имя: {localPlayer.DisplayName}");

            isGameCenterAuthenticated = true;
            await FetchGameCenterCredentials();
            SignInWithGameCenter(tryLinkGuest: true);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Auth-Apple] Вход в Game Center не удался: {e.Message}");
            OnLoginFailed?.Invoke(e.Message);
        }
    }
#endif
    #endregion

    #region Гостевой вход
    public async void SignInAsGuest()
    {
        Debug.Log("[Auth-Apple] Гостевой вход...");

        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("[Auth-Apple] Пользователь уже авторизован");
                isAuthenticated = true;
                OnLoginSuccess?.Invoke(AuthenticationService.Instance.PlayerId);

                if (autoLoadSceneOnLogin)
                    LoadGameScene();
                return;
            }

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            isAuthenticated = true;
            string playerId = AuthenticationService.Instance.PlayerId;

            Debug.Log("[Auth-Apple] ========== ГОСТЕВОЙ ВХОД УСПЕШЕН ==========");
            Debug.Log($"[Auth-Apple] Unity Player ID: {playerId}");
            Debug.Log("[Auth-Apple] ============================================");

            OnLoginSuccess?.Invoke(playerId);

            if (autoLoadSceneOnLogin)
                LoadGameScene();
        }
        catch (Exception ex) when (ex is AuthenticationException || ex is RequestFailedException)
        {
            Debug.LogWarning($"[Auth-Apple] Ошибка гостевого входа: {ex.Message}");

            bool isInvalidTokenError = ex.Message.Contains("INVALID_SESSION_TOKEN") ||
                                       ex.Message.Contains("401") ||
                                       ex.Message.Contains("session token is not valid") ||
                                       (ex is RequestFailedException rfe && rfe.ErrorCode == 401);

            if (isInvalidTokenError || AuthenticationService.Instance.SessionTokenExists)
            {
                Debug.Log("[Auth-Apple] Очистка токена и создание нового гостевого аккаунта...");
                AuthenticationService.Instance.ClearSessionToken();
                await RetryGuestSignIn();
                return;
            }

            OnLoginFailed?.Invoke(ex.Message);
        }
    }

    private async Task RetryGuestSignIn()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            isAuthenticated = true;
            string playerId = AuthenticationService.Instance.PlayerId;

            Debug.Log("[Auth-Apple] ========== НОВЫЙ ГОСТЕВОЙ АККАУНТ ==========");
            Debug.Log($"[Auth-Apple] Unity Player ID: {playerId}");
            Debug.Log("[Auth-Apple] =============================================");

            OnLoginSuccess?.Invoke(playerId);

            if (autoLoadSceneOnLogin)
                LoadGameScene();
        }
        catch (Exception retryEx)
        {
            Debug.LogError($"[Auth-Apple] Повторная попытка не удалась: {retryEx.Message}");
            OnLoginFailed?.Invoke(retryEx.Message);
        }
    }
    #endregion

    #region Unity Authentication
    private async void SignInWithGameCenter(bool tryLinkGuest)
    {
#if UNITY_IOS
        try
        {
            bool hasGuestSession = AuthenticationService.Instance.SessionTokenExists;

            if (tryLinkGuest && hasGuestSession && !AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("[Auth-Apple] Найден гостевой аккаунт. Пытаемся привязать к Game Center...");

                try
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    string guestId = AuthenticationService.Instance.PlayerId;
                    Debug.Log($"[Auth-Apple] Вошли в гостевой аккаунт: {guestId}");

                    await AuthenticationService.Instance.LinkWithAppleGameCenterAsync(
                        m_Signature, m_TeamPlayerId, m_PublicKeyUrl, m_Salt, m_Timestamp);
                    Debug.Log("[Auth-Apple] Гостевой аккаунт успешно привязан к Game Center!");
                }
                catch (Exception linkEx) when (linkEx is AuthenticationException || linkEx is RequestFailedException)
                {
                    bool isInvalidTokenError = linkEx.Message.Contains("INVALID_SESSION_TOKEN") ||
                                               linkEx.Message.Contains("401") ||
                                               linkEx.Message.Contains("session token is not valid");

                    if (isInvalidTokenError)
                        Debug.LogWarning($"[Auth-Apple] Гостевой аккаунт недействителен: {linkEx.Message}");
                    else
                        Debug.LogWarning($"[Auth-Apple] Не удалось привязать: {linkEx.Message}");

                    Debug.Log("[Auth-Apple] Очищаем гостевую сессию и входим через Game Center...");

                    if (AuthenticationService.Instance.IsSignedIn)
                        AuthenticationService.Instance.SignOut();

                    AuthenticationService.Instance.ClearSessionToken();

                    await AuthenticationService.Instance.SignInWithAppleGameCenterAsync(
                        m_Signature, m_TeamPlayerId, m_PublicKeyUrl, m_Salt, m_Timestamp);
                }
            }
            else if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("[Auth-Apple] Вход в Unity Services через Game Center...");
                await AuthenticationService.Instance.SignInWithAppleGameCenterAsync(
                    m_Signature, m_TeamPlayerId, m_PublicKeyUrl, m_Salt, m_Timestamp);
            }

            isAuthenticated = true;
            string playerId = AuthenticationService.Instance.PlayerId;

            Debug.Log("[Auth-Apple] ========== UNITY AUTH УСПЕШНО ==========");
            Debug.Log($"[Auth-Apple] Unity Player ID: {playerId}");
            Debug.Log($"[Auth-Apple] Гостевой: {IsGuest}");
            Debug.Log("[Auth-Apple] =========================================");

            OnLoginSuccess?.Invoke(playerId);

            if (autoLoadSceneOnLogin)
                LoadGameScene();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"[Auth-Apple] Ошибка аутентификации: {ex.Message}");
            OnLoginFailed?.Invoke(ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"[Auth-Apple] Ошибка запроса: {ex.Message}");
            OnLoginFailed?.Invoke(ex.Message);
        }
#endif
    }
    #endregion

    #region Выход
    public void SignOut()
    {
        Debug.Log("[Auth-Apple] Выход из аккаунта...");

        AuthenticationService.Instance.SignOut();

        isAuthenticated = false;
        isGameCenterAuthenticated = false;

        Debug.Log("[Auth-Apple] Выход выполнен");
        OnLogoutSuccess?.Invoke();
    }
    #endregion

    #region Загрузка сцены
    public void LoadGameScene()
    {
        Debug.Log($"[Auth-Apple] Загрузка сцены: {gameSceneName}");
        StartCoroutine(LoadSceneWithCurtain(gameSceneName));
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"[Auth-Apple] Загрузка сцены: {sceneName}");
        StartCoroutine(LoadSceneWithCurtain(sceneName));
    }

    private IEnumerator LoadSceneWithCurtain(string sceneName)
    {
        if (curtain != null) curtain.SetActive(true);
        yield return null;
        SceneManager.LoadScene(sceneName);
    }
    #endregion

    #region Публичные свойства и методы
    public bool IsAuthenticated => isAuthenticated && AuthenticationService.Instance.IsSignedIn;

    public bool IsGuest => AuthenticationService.Instance.IsSignedIn &&
                           AuthenticationService.Instance.PlayerInfo?.Identities?.Count == 0;

    public string GetUserId()
    {
        if (AuthenticationService.Instance.IsSignedIn)
            return AuthenticationService.Instance.PlayerId;
        return null;
    }

    public string GetUserName()
    {
#if UNITY_IOS
        if (isGameCenterAuthenticated)
            return GKLocalPlayer.Local.DisplayName;
#endif
        return null;
    }
    #endregion
}
