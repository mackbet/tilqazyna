using UnityEngine;

/// <summary>
/// Пример использования системы авторизации
/// </summary>
public class AuthExample : MonoBehaviour
{
    private void Start()
    {
        // Подписка на события авторизации
        if (AndroidLoginManager.Instance != null)
        {
            AndroidLoginManager.Instance.OnLoginSuccess += HandleLoginSuccess;
            AndroidLoginManager.Instance.OnLoginFailed += HandleLoginFailed;
            AndroidLoginManager.Instance.OnLogoutSuccess += HandleLogout;

            // Проверка состояния при запуске
            if (AndroidLoginManager.Instance.IsAuthenticated)
            {
                Debug.Log("Пользователь уже авторизован");
                ShowMainGame();
            }
            else
            {
                Debug.Log("Пользователь не авторизован");
                ShowLoginScreen();
            }
        }
    }

    private void OnDestroy()
    {
        // Отписка от событий
        if (AndroidLoginManager.Instance != null)
        {
            AndroidLoginManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
            AndroidLoginManager.Instance.OnLoginFailed -= HandleLoginFailed;
            AndroidLoginManager.Instance.OnLogoutSuccess -= HandleLogout;
        }
    }

    // ========== ОБРАБОТЧИКИ СОБЫТИЙ ==========

    private void HandleLoginSuccess(string odl)
    {
        string userName = AndroidLoginManager.Instance?.GetUserName();

        Debug.Log($"Авторизация успешна!");
        Debug.Log($"   Player ID: {odl}");
        Debug.Log($"   User Name: {userName}");

        ShowMainGame();
    }

    private void HandleLoginFailed(string error)
    {
        Debug.LogError($"Ошибка авторизации: {error}");
        ShowErrorMessage(error);
    }

    private void HandleLogout()
    {
        Debug.Log("Пользователь вышел из аккаунта");
        ShowLoginScreen();
    }

    // ========== ИГРОВАЯ ЛОГИКА ==========

    private void ShowMainGame()
    {
        Debug.Log("Загрузка главного экрана игры...");
        // TODO: Загрузить главную сцену игры
    }

    private void ShowLoginScreen()
    {
        Debug.Log("Показ экрана входа...");
        // TODO: Показать UI входа
    }

    private void ShowErrorMessage(string error)
    {
        Debug.Log($"Показ ошибки пользователю: {error}");
        // TODO: Показать popup с ошибкой
    }

    // ========== ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ КНОПОК ==========

    public void OnGoogleLoginButtonClick()
    {
        Debug.Log("Кнопка Google нажата");
#if UNITY_ANDROID
        LoginManager.Instance?.ManuallyLoginGooglePlayGames();
#else
        Debug.LogWarning("Google Play Games доступен только на Android");
#endif
    }

    public void OnAppleLoginButtonClick()
    {
        Debug.Log("Кнопка Apple нажата");
        // TODO: Реализовать Sign In with Apple для iOS
        Debug.LogWarning("Apple Sign-In пока не реализован");
    }

    public void OnLogoutButtonClick()
    {
        Debug.Log("Кнопка выхода нажата");
        AndroidLoginManager.Instance?.SignOut();
    }

    // ========== ДОПОЛНИТЕЛЬНЫЕ УТИЛИТЫ ==========

    /// <summary>
    /// Получить ID текущего пользователя
    /// </summary>
    public string GetCurrentUserId()
    {
        return AndroidLoginManager.Instance?.GetUserId();
    }

    /// <summary>
    /// Проверка авторизации
    /// </summary>
    public bool IsUserLoggedIn()
    {
        return AndroidLoginManager.Instance != null && AndroidLoginManager.Instance.IsAuthenticated;
    }
}
