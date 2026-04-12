using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

/// <summary>
/// Базовый класс для авторизации. Наследуется AndroidLoginManager и AppleLoginManager.
/// GameManager и другие скрипты работают только с BaseLoginManager.Instance.
/// </summary>
public abstract class BaseLoginManager : MonoBehaviour
{
    public static BaseLoginManager Instance { get; protected set; }

    #region Настройки
    [Header("Настройки сцены")]
    [SerializeField] protected string gameSceneName = "GameScene";
    [SerializeField] protected bool autoLoadSceneOnLogin = true;
    [SerializeField] protected GameObject curtain;
    #endregion

    #region События
    public event Action<string> OnLoginSuccess;
    public event Action<string> OnLoginFailed;
    public event Action OnLogoutSuccess;
    #endregion

    #region Состояние
    protected bool isAuthenticated = false;
    #endregion

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    protected virtual async void Start()
    {
        await InitializeUnityServices();
        OnStart();
    }
    #endregion

    #region Инициализация
    protected async Task InitializeUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            Debug.Log("[Auth] Инициализация Unity Services...");
            await UnityServices.InitializeAsync();
            Debug.Log("[Auth] Unity Services инициализированы");
        }
    }

    /// <summary>
    /// Вызывается после InitializeUnityServices в Start.
    /// Переопределяется в наследниках для платформенной инициализации.
    /// </summary>
    protected abstract void OnStart();
    #endregion

    #region Гостевой вход
    public async void SignInAsGuest()
    {
        Debug.Log("[Auth] Гостевой вход...");

        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("[Auth] Пользователь уже авторизован");
                isAuthenticated = true;
                InvokeLoginSuccess(AuthenticationService.Instance.PlayerId);
                if (autoLoadSceneOnLogin) LoadGameScene();
                return;
            }

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            isAuthenticated = true;
            string playerId = AuthenticationService.Instance.PlayerId;

            Debug.Log("[Auth] ========== ГОСТЕВОЙ ВХОД УСПЕШЕН ==========");
            Debug.Log($"[Auth] Unity Player ID: {playerId}");
            Debug.Log("[Auth] ============================================");

            InvokeLoginSuccess(playerId);
            if (autoLoadSceneOnLogin) LoadGameScene();
        }
        catch (Exception ex) when (ex is AuthenticationException || ex is RequestFailedException)
        {
            Debug.LogWarning($"[Auth] Ошибка гостевого входа: {ex.Message}");

            bool isInvalidTokenError = ex.Message.Contains("INVALID_SESSION_TOKEN") ||
                                       ex.Message.Contains("401") ||
                                       ex.Message.Contains("session token is not valid") ||
                                       (ex is RequestFailedException rfe && rfe.ErrorCode == 401);

            if (isInvalidTokenError || AuthenticationService.Instance.SessionTokenExists)
            {
                Debug.Log("[Auth] Очистка токена и создание нового гостевого аккаунта...");
                AuthenticationService.Instance.ClearSessionToken();
                await RetryGuestSignIn();
                return;
            }

            InvokeLoginFailed(ex.Message);
        }
    }

    private async Task RetryGuestSignIn()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            isAuthenticated = true;
            string playerId = AuthenticationService.Instance.PlayerId;

            Debug.Log("[Auth] ========== НОВЫЙ ГОСТЕВОЙ АККАУНТ ==========");
            Debug.Log($"[Auth] Unity Player ID: {playerId}");
            Debug.Log("[Auth] =============================================");

            InvokeLoginSuccess(playerId);
            if (autoLoadSceneOnLogin) LoadGameScene();
        }
        catch (Exception retryEx)
        {
            Debug.LogError($"[Auth] Повторная попытка не удалась: {retryEx.Message}");
            InvokeLoginFailed(retryEx.Message);
        }
    }
    #endregion

    #region Выход
    public virtual void SignOut()
    {
        Debug.Log("[Auth] Выход из аккаунта...");
        AuthenticationService.Instance.SignOut();
        isAuthenticated = false;
        Debug.Log("[Auth] Выход выполнен");
        OnLogoutSuccess?.Invoke();
    }
    #endregion

    #region Загрузка сцены
    public void LoadGameScene()
    {
        Debug.Log($"[Auth] Загрузка сцены: {gameSceneName}");
        StartCoroutine(LoadSceneWithCurtain(gameSceneName));
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"[Auth] Загрузка сцены: {sceneName}");
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

    public abstract string GetUserName();
    #endregion

    #region Вспомогательные методы для вызова событий из наследников
    protected void InvokeLoginSuccess(string playerId) => OnLoginSuccess?.Invoke(playerId);
    protected void InvokeLoginFailed(string error) => OnLoginFailed?.Invoke(error);
    #endregion
}
