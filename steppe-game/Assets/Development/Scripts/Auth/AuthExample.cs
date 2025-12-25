using UnityEngine;

/// <summary>
/// Пример использования системы авторизации
/// </summary>
public class AuthExample : MonoBehaviour
{
    private void Start()
    {
        // Подписка на события авторизации
        AuthenticationManager.Instance.OnLoginSuccess += HandleLoginSuccess;
        AuthenticationManager.Instance.OnLoginFailed += HandleLoginFailed;
        AuthenticationManager.Instance.OnLogoutSuccess += HandleLogout;

        // Проверка состояния при запуске
        if (AuthenticationManager.Instance.IsAuthenticated)
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

    private void OnDestroy()
    {
        // Отписка от событий
        if (AuthenticationManager.Instance != null)
        {
            AuthenticationManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
            AuthenticationManager.Instance.OnLoginFailed -= HandleLoginFailed;
            AuthenticationManager.Instance.OnLogoutSuccess -= HandleLogout;
        }
    }

    // ========== ОБРАБОТЧИКИ СОБЫТИЙ ==========

    private void HandleLoginSuccess(UserData userData)
    {
        Debug.Log($"✅ Авторизация успешна!");
        Debug.Log($"   User ID: {userData.userId}");
        Debug.Log($"   User Name: {userData.userName}");
        Debug.Log($"   Provider: {userData.provider}");
        Debug.Log($"   Email: {userData.email ?? "не указан"}");

        // Здесь можно отправить данные на сервер
        SendUserDataToServer(userData);

        // Показать главный экран игры
        ShowMainGame();
    }

    private void HandleLoginFailed(string error)
    {
        Debug.LogError($"❌ Ошибка авторизации: {error}");
        
        // Показать сообщение об ошибке пользователю
        ShowErrorMessage(error);
    }

    private void HandleLogout()
    {
        Debug.Log("🚪 Пользователь вышел из аккаунта");
        
        // Вернуться к экрану входа
        ShowLoginScreen();
    }

    // ========== ИГРОВАЯ ЛОГИКА ==========

    private void SendUserDataToServer(UserData userData)
    {
        // TODO: Отправка данных на ваш backend
        Debug.Log($"📤 Отправка данных пользователя на сервер...");
        
        // Пример:
        // StartCoroutine(SendToServerCoroutine(userData));
    }

    private void ShowMainGame()
    {
        Debug.Log("🎮 Загрузка главного экрана игры...");
        // TODO: Загрузить главную сцену игры
        // SceneManager.LoadScene("MainGame");
    }

    private void ShowLoginScreen()
    {
        Debug.Log("🔐 Показ экрана входа...");
        // TODO: Показать UI входа
    }

    private void ShowErrorMessage(string error)
    {
        Debug.Log($"💬 Показ ошибки пользователю: {error}");
        // TODO: Показать popup с ошибкой
    }

    // ========== ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ КНОПОК ==========

    public void OnGoogleLoginButtonClick()
    {
        Debug.Log("🔵 Кнопка Google нажата");
        AuthenticationManager.Instance?.SignInWithGoogle();
    }

    public void OnAppleLoginButtonClick()
    {
        Debug.Log("🍎 Кнопка Apple нажата");
        AuthenticationManager.Instance?.SignInWithApple();
    }

    public void OnLogoutButtonClick()
    {
        Debug.Log("🚪 Кнопка выхода нажата");
        AuthenticationManager.Instance?.SignOut();
    }

    // ========== ДОПОЛНИТЕЛЬНЫЕ УТИЛИТЫ ==========

    /// <summary>
    /// Получить текущего пользователя
    /// </summary>
    public UserData GetCurrentUser()
    {
        if (AuthenticationManager.Instance.IsAuthenticated)
        {
            return AuthenticationManager.Instance.CurrentUser;
        }
        return null;
    }

    /// <summary>
    /// Проверка авторизации
    /// </summary>
    public bool IsUserLoggedIn()
    {
        return AuthenticationManager.Instance.IsAuthenticated;
    }

    /// <summary>
    /// Переключить тестовый режим
    /// </summary>
    public void ToggleTestMode()
    {
        bool currentMode = AuthenticationManager.Instance.IsTestMode;
        AuthenticationManager.Instance.SetTestMode(!currentMode);
        Debug.Log($"Test Mode переключен: {!currentMode}");
    }
}

// ========== ПРИМЕР ИНТЕГРАЦИИ С СЕРВЕРОМ ==========

/*
using System.Collections;
using UnityEngine.Networking;

public class ServerAPI
{
    private const string SERVER_URL = "https://your-server.com/api";

    public static IEnumerator SendUserData(UserData userData)
    {
        string json = JsonUtility.ToJson(userData);
        
        using (UnityWebRequest request = UnityWebRequest.Post($"{SERVER_URL}/auth", json))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Данные успешно отправлены на сервер");
                string response = request.downloadHandler.text;
                Debug.Log($"Ответ сервера: {response}");
            }
            else
            {
                Debug.LogError($"Ошибка отправки: {request.error}");
            }
        }
    }
}
*/