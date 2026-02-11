using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif


public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    #region Настройки
    [Header("Настройки сцены")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private bool autoLoadSceneOnLogin = true;
    [SerializeField] private GameObject curtain;
    #endregion

    #region События
    public event Action<string> OnLoginSuccess;  // передаёт playerId
    public event Action<string> OnLoginFailed;
    public event Action OnLogoutSuccess;
    #endregion

    #region Состояние
    private static bool s_Activated = false;
    private string m_GooglePlayGamesToken;
    private bool isAuthenticated = false;
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
        InitializeGooglePlayGames();

        // Пробуем автовход только через Google Play Games (не гостевой)
        // Если не получится - покажется экран выбора авторизации
        TryAutoLoginGooglePlayGames();
    }
    #endregion

    #region Инициализация
    private async Task InitializeUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            Debug.Log("[Auth] Инициализация Unity Services...");
            await UnityServices.InitializeAsync();
            Debug.Log("[Auth] Unity Services инициализированы");
        }
    }

    private void InitializeGooglePlayGames()
    {
#if UNITY_ANDROID
        if (!s_Activated)
        {
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
            s_Activated = true;
            Debug.Log("[Auth] Google Play Games активирован");
        }
#endif
    }
    #endregion

    #region Автоматический вход (только для не-гостевых аккаунтов)
    /// <summary>
    /// Пробует автоматический вход через Google Play Games (только Android).
    /// Гостевой вход НЕ восстанавливается автоматически - пользователь должен выбрать.
    /// </summary>
    private void TryAutoLoginGooglePlayGames()
    {
#if UNITY_ANDROID
        Debug.Log("[Auth] Попытка автоматического входа через Google Play Games...");

        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("[Auth] ========== GOOGLE PLAY GAMES СЕССИЯ ==========");
                Debug.Log($"[Auth] ID игрока: {PlayGamesPlatform.Instance.GetUserId()}");
                Debug.Log($"[Auth] Имя: {PlayGamesPlatform.Instance.GetUserDisplayName()}");
                Debug.Log("[Auth] ===============================================");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    m_GooglePlayGamesToken = code;
                    // При автовходе НЕ пытаемся привязать гостевой - пользователь уже входил через Google
                    SignInWithGooglePlayGames(tryLinkGuest: false);
                });
            }
            else
            {
                Debug.Log($"[Auth] Автовход Google Play Games не удался (статус: {status}).");
                Debug.Log("[Auth] Ожидание выбора способа авторизации (гостевой или Google).");
                if (curtain != null) curtain.SetActive(false);
            }
        });
#else
        Debug.Log("[Auth] Платформа не Android. Ожидание выбора способа авторизации.");
        if (curtain != null) curtain.SetActive(false);
#endif
    }
    #endregion

    #region Google Play Games
#if UNITY_ANDROID
    /// <summary>
    /// Автоматический вход через Google Play Games (без диалога выбора аккаунта).
    /// </summary>
    public void LoginGooglePlayGames()
    {
        Debug.Log("[Auth] Попытка входа через Google Play Games...");

        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("[Auth] Google Play Games: вход успешен");
                Debug.Log($"[Auth] Имя: {PlayGamesPlatform.Instance.GetUserDisplayName()}");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("[Auth] Код авторизации получен");
                    m_GooglePlayGamesToken = code;
                    // При ручном входе пытаемся привязать гостевой аккаунт если он есть
                    SignInWithGooglePlayGames(tryLinkGuest: true);
                });
            }
            else
            {
                Debug.LogWarning($"[Auth] Вход не удался. Статус: {status}");
                OnLoginFailed?.Invoke($"Google Play Games вход не удался: {status}");
            }
        });
    }

    /// <summary>
    /// Ручной вход с диалогом выбора аккаунта Google Play Games.
    /// </summary>
    public void ManuallyLoginGooglePlayGames()
    {
        Debug.Log("[Auth] Открытие диалога выбора аккаунта Google Play Games...");

        PlayGamesPlatform.Instance.ManuallyAuthenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("[Auth] ========== ВХОД ВЫПОЛНЕН УСПЕШНО ==========");
                Debug.Log($"[Auth] ID игрока: {PlayGamesPlatform.Instance.GetUserId()}");
                Debug.Log($"[Auth] Имя: {PlayGamesPlatform.Instance.GetUserDisplayName()}");
                Debug.Log($"[Auth] Аватар: {PlayGamesPlatform.Instance.GetUserImageUrl()}");
                Debug.Log("[Auth] ============================================");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("[Auth] Код авторизации получен");
                    m_GooglePlayGamesToken = code;
                    // При ручном входе пытаемся привязать гостевой аккаунт если он есть
                    SignInWithGooglePlayGames(tryLinkGuest: true);
                });
            }
            else
            {
                Debug.LogWarning($"[Auth] Вход отменён или не удался. Статус: {status}");
                OnLoginFailed?.Invoke($"Вход отменён: {status}");
            }
        });
    }
#endif
    #endregion

    #region Гостевой вход
    /// <summary>
    /// Гостевой (анонимный) вход без привязки к аккаунту.
    /// Если уже есть сохранённая гостевая сессия - восстанавливает её.
    /// Если нет - создаёт новый гостевой аккаунт.
    /// </summary>
    public async void SignInAsGuest()
    {
        Debug.Log("[Auth] Гостевой вход...");

        try
        {
            // Если уже авторизован - используем текущую сессию
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("[Auth] Пользователь уже авторизован");
                isAuthenticated = true;
                string odl = AuthenticationService.Instance.PlayerId;
                OnLoginSuccess?.Invoke(odl);

                if (autoLoadSceneOnLogin)
                {
                    LoadGameScene();
                }
                return;
            }

            // Пробуем войти (если есть сохранённый токен - восстановит сессию)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            isAuthenticated = true;
            string odl2 = AuthenticationService.Instance.PlayerId;

            Debug.Log("[Auth] ========== ГОСТЕВОЙ ВХОД УСПЕШЕН ==========");
            Debug.Log($"[Auth] Unity Player ID: {odl2}");
            Debug.Log("[Auth] ============================================");

            OnLoginSuccess?.Invoke(odl2);

            if (autoLoadSceneOnLogin)
            {
                LoadGameScene();
            }
        }
        catch (Exception ex) when (ex is AuthenticationException || ex is RequestFailedException)
        {
            Debug.LogWarning($"[Auth] Ошибка гостевого входа: {ex.Message}");

            // Проверяем, связана ли ошибка с невалидным/удалённым аккаунтом
            bool isInvalidTokenError = ex.Message.Contains("INVALID_SESSION_TOKEN") ||
                                       ex.Message.Contains("401") ||
                                       ex.Message.Contains("session token is not valid") ||
                                       (ex is RequestFailedException rfe && rfe.ErrorCode == 401);

            // Если ошибка связана с невалидным токеном ИЛИ есть сохранённый токен - очищаем и создаём новый аккаунт
            if (isInvalidTokenError || AuthenticationService.Instance.SessionTokenExists)
            {
                Debug.Log("[Auth] Обнаружена проблема с сессией. Очистка токена и создание нового гостевого аккаунта...");
                AuthenticationService.Instance.ClearSessionToken();

                await RetryGuestSignIn();
                return;
            }

            OnLoginFailed?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Повторная попытка гостевого входа после очистки токена
    /// </summary>
    private async Task RetryGuestSignIn()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            isAuthenticated = true;
            string odl = AuthenticationService.Instance.PlayerId;

            Debug.Log("[Auth] ========== НОВЫЙ ГОСТЕВОЙ АККАУНТ ==========");
            Debug.Log($"[Auth] Unity Player ID: {odl}");
            Debug.Log("[Auth] =============================================");

            OnLoginSuccess?.Invoke(odl);

            if (autoLoadSceneOnLogin)
            {
                LoadGameScene();
            }
        }
        catch (Exception retryEx)
        {
            Debug.LogError($"[Auth] Повторная попытка не удалась: {retryEx.Message}");
            OnLoginFailed?.Invoke(retryEx.Message);
        }
    }
    #endregion

    #region Unity Authentication
    /// <summary>
    /// Вход в Unity Services через Google Play Games.
    /// </summary>
    /// <param name="tryLinkGuest">Пытаться ли привязать гостевой аккаунт к Google (true при ручном входе, false при автовходе)</param>
    private async void SignInWithGooglePlayGames(bool tryLinkGuest)
    {
        if (string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.LogWarning("[Auth] Код авторизации пустой!");
            OnLoginFailed?.Invoke("Код авторизации пустой");
            return;
        }

        try
        {
            bool hasGuestSession = AuthenticationService.Instance.SessionTokenExists;

            // Пытаемся привязать гостевой аккаунт только если:
            // 1. tryLinkGuest = true (ручной вход, не автовход)
            // 2. Есть сохранённая сессия
            // 3. Ещё не авторизованы
            if (tryLinkGuest && hasGuestSession && !AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("[Auth] Найден гостевой аккаунт. Пытаемся привязать к Google...");

                try
                {
                    // Входим в гостевой аккаунт
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    string guestId = AuthenticationService.Instance.PlayerId;
                    Debug.Log($"[Auth] Вошли в гостевой аккаунт: {guestId}");

                    // Пробуем привязать Google к гостевому аккаунту
                    await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
                    Debug.Log("[Auth] Гостевой аккаунт успешно привязан к Google Play Games!");
                }
                catch (Exception linkEx) when (linkEx is AuthenticationException || linkEx is RequestFailedException)
                {
                    // Проверяем, связана ли ошибка с невалидным/удалённым гостевым аккаунтом
                    bool isInvalidTokenError = linkEx.Message.Contains("INVALID_SESSION_TOKEN") ||
                                               linkEx.Message.Contains("401") ||
                                               linkEx.Message.Contains("session token is not valid");

                    if (isInvalidTokenError)
                    {
                        Debug.LogWarning($"[Auth] Гостевой аккаунт недействителен: {linkEx.Message}");
                    }
                    else
                    {
                        Debug.LogWarning($"[Auth] Не удалось привязать гостевой аккаунт: {linkEx.Message}");
                    }

                    Debug.Log("[Auth] Очищаем гостевую сессию и входим через Google...");

                    // Выходим из гостевого (если был вход)
                    if (AuthenticationService.Instance.IsSignedIn)
                    {
                        AuthenticationService.Instance.SignOut();
                    }
                    // Очищаем гостевой токен
                    AuthenticationService.Instance.ClearSessionToken();

                    // Входим напрямую через Google
                    await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
                }
            }
            else if (!AuthenticationService.Instance.IsSignedIn)
            {
                // Просто входим через Google (автовход или нет гостевой сессии)
                Debug.Log("[Auth] Вход в Unity Services через Google Play Games...");
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
            }

            isAuthenticated = true;
            string odl = AuthenticationService.Instance.PlayerId;

            Debug.Log("[Auth] ========== UNITY AUTH УСПЕШНО ==========");
            Debug.Log($"[Auth] Unity Player ID: {odl}");
            Debug.Log($"[Auth] Гостевой: {IsGuest}");
            Debug.Log("[Auth] =========================================");

            OnLoginSuccess?.Invoke(odl);

            if (autoLoadSceneOnLogin)
            {
                LoadGameScene();
            }
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"[Auth] Ошибка аутентификации: {ex.Message}");
            OnLoginFailed?.Invoke(ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"[Auth] Ошибка запроса: {ex.Message}");
            OnLoginFailed?.Invoke(ex.Message);
        }
    }
    #endregion

    #region Выход
    public void SignOut()
    {
        Debug.Log("[Auth] Выход из аккаунта...");

        AuthenticationService.Instance.SignOut();

        isAuthenticated = false;

        Debug.Log("[Auth] Выход выполнен");
        OnLogoutSuccess?.Invoke();
    }
    #endregion

    #region Загрузка сцены
    /// <summary>
    /// Загрузить игровую сцену
    /// </summary>
    public void LoadGameScene()
    {
        Debug.Log($"[Auth] Загрузка сцены: {gameSceneName}");
        StartCoroutine(LoadSceneWithCurtain(gameSceneName));
    }

    /// <summary>
    /// Загрузить указанную сцену
    /// </summary>
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

    /// <summary>
    /// Проверить, гостевой ли это аккаунт
    /// </summary>
    public bool IsGuest => AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.PlayerInfo?.Identities?.Count == 0;

    /// <summary>
    /// Получить Unity Player ID для сохранения данных
    /// </summary>
    public string GetUserId()
    {
        if (AuthenticationService.Instance.IsSignedIn)
            return AuthenticationService.Instance.PlayerId;

        return null;
    }

    /// <summary>
    /// Получить имя пользователя из Google Play Games
    /// </summary>
    public string GetUserName()
    {
#if UNITY_ANDROID
        if (PlayGamesPlatform.Instance.IsAuthenticated())
            return PlayGamesPlatform.Instance.GetUserDisplayName();
#endif
        return null;
    }

    /// <summary>
    /// Получить URL аватара из Google Play Games
    /// </summary>
    public string GetAvatarUrl()
    {
#if UNITY_ANDROID
        if (PlayGamesPlatform.Instance.IsAuthenticated())
            return PlayGamesPlatform.Instance.GetUserImageUrl();
#endif
        return null;
    }
    #endregion
}
