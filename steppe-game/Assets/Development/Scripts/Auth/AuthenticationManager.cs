using UnityEngine;
using System;
using System.Threading.Tasks;

/// <summary>
/// Менеджер аутентификации для Google и Apple Sign-In
/// Работает как заглушка для тестирования UI и логики без реальной авторизации
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

    // Настройки для тестирования
    [Header("Test Mode Settings")]
    [SerializeField] private bool testMode = true;
    [SerializeField] private float simulatedDelay = 1.5f;

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
        }
    }

    /// <summary>
    /// Вход через Google
    /// </summary>
    public async void SignInWithGoogle()
    {
        Debug.Log("🔵 Начало входа через Google...");

        if (testMode)
        {
            await SimulateGoogleSignIn();
        }
        else
        {
            // TODO: Реальная авторизация через Google Play Games или Firebase
            RealGoogleSignIn();
        }
    }

    /// <summary>
    /// Вход через Apple
    /// </summary>
    public async void SignInWithApple()
    {
        Debug.Log("🍎 Начало входа через Apple...");

        if (testMode)
        {
            await SimulateAppleSignIn();
        }
        else
        {
            // TODO: Реальная авторизация через Apple Sign-In
            RealAppleSignIn();
        }
    }

    /// <summary>
    /// Выход из аккаунта
    /// </summary>
    public void SignOut()
    {
        Debug.Log("🚪 Выход из аккаунта");

        currentUser = null;
        isAuthenticated = false;

        OnLogoutSuccess?.Invoke();
    }

    // ========== ТЕСТОВЫЕ МЕТОДЫ (симуляция) ==========

    private async Task SimulateGoogleSignIn()
    {
        // Симулируем задержку авторизации
        await Task.Delay(TimeSpan.FromSeconds(simulatedDelay));

        // Симулируем успешный вход
        if (UnityEngine.Random.value > 0.1f) // 90% успеха
        {
            currentUser = new UserData
            {
                userId = "google_" + Guid.NewGuid().ToString().Substring(0, 8),
                userName = "Test User (Google)",
                email = "testuser@gmail.com",
                provider = AuthProvider.Google,
                avatarUrl = null
            };

            isAuthenticated = true;
            Debug.Log($"✅ Вход через Google успешен! User: {currentUser.userName}");
            OnLoginSuccess?.Invoke(currentUser);
        }
        else
        {
            Debug.LogWarning("❌ Ошибка входа через Google");
            OnLoginFailed?.Invoke("Не удалось войти через Google");
        }
    }

    private async Task SimulateAppleSignIn()
    {
        // Симулируем задержку авторизации
        await Task.Delay(TimeSpan.FromSeconds(simulatedDelay));

        // Симулируем успешный вход
        if (UnityEngine.Random.value > 0.1f) // 90% успеха
        {
            currentUser = new UserData
            {
                userId = "apple_" + Guid.NewGuid().ToString().Substring(0, 8),
                userName = "Test User (Apple)",
                email = "testuser@icloud.com",
                provider = AuthProvider.Apple,
                avatarUrl = null
            };

            isAuthenticated = true;
            Debug.Log($"✅ Вход через Apple успешен! User: {currentUser.userName}");
            OnLoginSuccess?.Invoke(currentUser);
        }
        else
        {
            Debug.LogWarning("❌ Ошибка входа через Apple");
            OnLoginFailed?.Invoke("Не удалось войти через Apple");
        }
    }

    // ========== РЕАЛЬНЫЕ МЕТОДЫ (для продакшена) ==========

    private void RealGoogleSignIn()
    {
        Debug.LogWarning("⚠️ Реальная авторизация Google не настроена");
        
        // TODO: Раскомментируйте когда настроите Google Play Games Services
        /*
        if (GooglePlayGamesAuth.Instance != null)
        {
            GooglePlayGamesAuth.Instance.Authenticate();
            GooglePlayGamesAuth.Instance.OnAuthenticationComplete += (success) =>
            {
                if (success)
                {
                    var userInfo = GooglePlayGamesAuth.Instance.GetUserInfo();
                    currentUser = new UserData
                    {
                        userId = userInfo.id,
                        userName = userInfo.userName,
                        email = null,
                        provider = AuthProvider.Google,
                        avatarUrl = null
                    };
                    isAuthenticated = true;
                    OnLoginSuccess?.Invoke(currentUser);
                }
                else
                {
                    OnLoginFailed?.Invoke("Google authentication failed");
                }
            };
        }
        */

        // Временная заглушка
        OnLoginFailed?.Invoke("Google авторизация не настроена. Включите Test Mode.");
    }

    private void RealAppleSignIn()
    {
        Debug.LogWarning("⚠️ Реальная авторизация Apple не настроена");

        // TODO: Раскомментируйте когда настроите Apple Sign-In
        /*
        #if UNITY_IOS
        if (AppleAuthManager.IsCurrentPlatformSupported)
        {
            var loginArgs = new AppleAuthLoginArgs(
                LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);

            appleAuthManager.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                    var appleIdCredential = credential as IAppleIDCredential;
                    currentUser = new UserData
                    {
                        userId = appleIdCredential.User,
                        userName = appleIdCredential.FullName?.GivenName ?? "Apple User",
                        email = appleIdCredential.Email,
                        provider = AuthProvider.Apple,
                        avatarUrl = null
                    };
                    isAuthenticated = true;
                    OnLoginSuccess?.Invoke(currentUser);
                },
                error =>
                {
                    OnLoginFailed?.Invoke($"Apple authentication failed: {error}");
                });
        }
        #endif
        */

        // Временная заглушка
        OnLoginFailed?.Invoke("Apple авторизация не настроена. Включите Test Mode.");
    }

    // ========== ПУБЛИЧНЫЕ СВОЙСТВА ==========

    public bool IsAuthenticated => isAuthenticated;
    public UserData CurrentUser => currentUser;
    public bool IsTestMode => testMode;

    /// <summary>
    /// Включить/выключить тестовый режим
    /// </summary>
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