using UnityEngine;
using System;
using System.Threading.Tasks;
#if UNITY_ANDROID
using GooglePlayGames;
#endif
using Firebase.Auth;

/// <summary>
/// Менеджер аутентификации для Google Play Games и Firebase
/// </summary>
public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    // События
    public event Action<UserData> OnLoginSuccess;
    public event Action<string> OnLoginFailed;
    public event Action OnLogoutSuccess;

    // Состояние
    private UserData currentUser;
    private bool isAuthenticated = false;
    private FirebaseAuth firebaseAuth;
    private FirebaseUser firebaseUser;

    // Настройки
    [Header("Settings")]
    [SerializeField] private bool testMode = false;
    [SerializeField] private float simulatedDelay = 1.5f;
    [SerializeField] private bool autoSignInOnStart = true;

    void Awake()
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

        InitializeGooglePlayGames();
    }

    private void Start()
    {
        // Инициализируем Firebase Auth
        firebaseAuth = FirebaseAuth.DefaultInstance;

        if (autoSignInOnStart && !testMode)
        {
            // Автоматический вход при запуске
            SignInWithGoogle();
        }
    }

    private void InitializeGooglePlayGames()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        Debug.Log("Google Play Games initialized");
#endif
    }

    /// <summary>
    /// Вход через Google Play Games + Firebase
    /// </summary>
    public async void SignInWithGoogle()
    {
        Debug.Log("Начало входа через Google Play Games...");

        if (testMode)
        {
            await SimulateGoogleSignIn();
            return;
        }

#if UNITY_ANDROID
        await RealGoogleSignIn();
#else
        Debug.LogWarning("Google Play Games доступен только на Android");
        OnLoginFailed?.Invoke("Google Play Games доступен только на Android");
#endif
    }

    /// <summary>
    /// Вход через Apple (для iOS)
    /// </summary>
    public async void SignInWithApple()
    {
        Debug.Log("Начало входа через Apple...");

        if (testMode)
        {
            await SimulateAppleSignIn();
            return;
        }

#if UNITY_IOS
        await RealAppleSignIn();
#else
        Debug.LogWarning("Apple Sign-In доступен только на iOS");
        OnLoginFailed?.Invoke("Apple Sign-In доступен только на iOS");
#endif
    }

    /// <summary>
    /// Выход из аккаунта
    /// </summary>
    public void SignOut()
    {
        Debug.Log("Выход из аккаунта");

        // Выход из Firebase
        if (firebaseAuth != null)
        {
            firebaseAuth.SignOut();
        }

        // В новой версии Google Play Games SDK нет метода SignOut
        // Пользователь остается залогиненным в Google Play Games

        currentUser = null;
        firebaseUser = null;
        isAuthenticated = false;

        OnLogoutSuccess?.Invoke();
    }

    // ========== РЕАЛЬНАЯ АВТОРИЗАЦИЯ ==========

#if UNITY_ANDROID
    private async Task RealGoogleSignIn()
    {
        var tcs = new TaskCompletionSource<bool>();

        // Шаг 1: Авторизация в Google Play Games
        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == GooglePlayGames.BasicApi.SignInStatus.Success)
            {
                Debug.Log("Google Play Games: вход успешен");
                tcs.SetResult(true);
            }
            else
            {
                Debug.LogError($"Google Play Games: ошибка входа - {status}");
                tcs.SetResult(false);
            }
        });

        bool gpgSuccess = await tcs.Task;

        if (!gpgSuccess)
        {
            OnLoginFailed?.Invoke("Не удалось войти в Google Play Games");
            return;
        }

        // Шаг 2: Получаем Server Auth Code для Firebase
        var authCodeTcs = new TaskCompletionSource<string>();

        PlayGamesPlatform.Instance.RequestServerSideAccess(false, (authCode) =>
        {
            authCodeTcs.SetResult(authCode);
        });

        string serverAuthCode = await authCodeTcs.Task;

        if (string.IsNullOrEmpty(serverAuthCode))
        {
            // Если не получили auth code, используем данные из Google Play Games напрямую
            Debug.LogWarning("Не удалось получить Server Auth Code, используем локальные данные");

            string odlname = PlayGamesPlatform.Instance.GetUserDisplayName();
            string odl = PlayGamesPlatform.Instance.GetUserId();

            currentUser = new UserData
            {
                userId = odl,
                userName = odlname,
                email = null,
                provider = AuthProvider.Google,
                avatarUrl = PlayGamesPlatform.Instance.GetUserImageUrl()
            };

            isAuthenticated = true;
            Debug.Log($"Вход успешен (без Firebase): {currentUser.userName}");
            OnLoginSuccess?.Invoke(currentUser);
            return;
        }

        // Шаг 3: Авторизация в Firebase с auth code
        try
        {
            Credential credential = PlayGamesAuthProvider.GetCredential(serverAuthCode);
            firebaseUser = await firebaseAuth.SignInWithCredentialAsync(credential);

            currentUser = new UserData
            {
                userId = firebaseUser.UserId,
                userName = firebaseUser.DisplayName ?? PlayGamesPlatform.Instance.GetUserDisplayName(),
                email = firebaseUser.Email,
                provider = AuthProvider.Google,
                avatarUrl = firebaseUser.PhotoUrl?.ToString()
            };

            isAuthenticated = true;
            Debug.Log($"Firebase вход успешен: {currentUser.userName} (ID: {currentUser.userId})");
            OnLoginSuccess?.Invoke(currentUser);
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase авторизация ошибка: {e.Message}");
            OnLoginFailed?.Invoke($"Firebase ошибка: {e.Message}");
        }
    }
#endif

#if UNITY_IOS
    private async Task RealAppleSignIn()
    {
        // TODO: Реализовать Apple Sign-In когда будет нужно
        // Требуется пакет: com.unity.signin.apple
        Debug.LogWarning("Apple Sign-In не реализован");
        OnLoginFailed?.Invoke("Apple Sign-In не реализован");
        await Task.CompletedTask;
    }
#endif

    // ========== ТЕСТОВЫЕ МЕТОДЫ ==========

    private async Task SimulateGoogleSignIn()
    {
        await Task.Delay(TimeSpan.FromSeconds(simulatedDelay));

        if (UnityEngine.Random.value > 0.1f)
        {
            currentUser = new UserData
            {
                userId = "test_google_" + Guid.NewGuid().ToString().Substring(0, 8),
                userName = "Test User (Google)",
                email = "testuser@gmail.com",
                provider = AuthProvider.Google,
                avatarUrl = null
            };

            isAuthenticated = true;
            Debug.Log($"[TEST] Вход через Google успешен: {currentUser.userName}");
            OnLoginSuccess?.Invoke(currentUser);
        }
        else
        {
            OnLoginFailed?.Invoke("Тестовая ошибка входа");
        }
    }

    private async Task SimulateAppleSignIn()
    {
        await Task.Delay(TimeSpan.FromSeconds(simulatedDelay));

        if (UnityEngine.Random.value > 0.1f)
        {
            currentUser = new UserData
            {
                userId = "test_apple_" + Guid.NewGuid().ToString().Substring(0, 8),
                userName = "Test User (Apple)",
                email = "testuser@icloud.com",
                provider = AuthProvider.Apple,
                avatarUrl = null
            };

            isAuthenticated = true;
            Debug.Log($"[TEST] Вход через Apple успешен: {currentUser.userName}");
            OnLoginSuccess?.Invoke(currentUser);
        }
        else
        {
            OnLoginFailed?.Invoke("Тестовая ошибка входа");
        }
    }

    // ========== ПУБЛИЧНЫЕ СВОЙСТВА ==========

    public bool IsAuthenticated => isAuthenticated;
    public UserData CurrentUser => currentUser;
    public FirebaseUser FirebaseUser => firebaseUser;
    public bool IsTestMode => testMode;

    /// <summary>
    /// Получить Firebase User ID (для сохранения данных)
    /// </summary>
    public string GetUserId()
    {
        if (firebaseUser != null)
            return firebaseUser.UserId;

        if (currentUser != null)
            return currentUser.userId;

        return null;
    }

    public void SetTestMode(bool enabled)
    {
        testMode = enabled;
        Debug.Log($"Test Mode: {(enabled ? "ON" : "OFF")}");
    }
}

/// <summary>
/// Провайдер авторизации
/// </summary>
public enum AuthProvider
{
    None,
    Google,
    Apple,
    Email
}

/// <summary>
/// Данные пользователя
/// </summary>
[Serializable]
public class UserData
{
    public string userId;
    public string userName;
    public string email;
    public AuthProvider provider;
    public string avatarUrl;

    public override string ToString()
    {
        return $"User: {userName} (ID: {userId}, Provider: {provider})";
    }
}
