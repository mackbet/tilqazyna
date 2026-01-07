using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI для авторизации
/// </summary>
public class AuthUI : MonoBehaviour
{
    [Header("Login Screen")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private Button googleSignInButton;
    [SerializeField] private Button appleSignInButton;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("User Profile Screen")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TextMeshProUGUI userIdText;
    [SerializeField] private TextMeshProUGUI providerText;
    [SerializeField] private Button signOutButton;

    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Start()
    {
        // Подписка на события
        if (AuthenticationManager.Instance != null)
        {
            AuthenticationManager.Instance.OnLoginSuccess += OnLoginSuccess;
            AuthenticationManager.Instance.OnLoginFailed += OnLoginFailed;
            AuthenticationManager.Instance.OnLogoutSuccess += OnLogoutSuccess;
        }

        // Привязка кнопок
        googleSignInButton?.onClick.AddListener(OnGoogleSignInClick);
        appleSignInButton?.onClick.AddListener(OnAppleSignInClick);
        signOutButton?.onClick.AddListener(OnSignOutClick);

        // Начальное состояние
        ShowLoginScreen();
        UpdateStatusText("Выберите способ входа");
    }

    private void OnDestroy()
    {
        // Отписка от событий
        if (AuthenticationManager.Instance != null)
        {
            AuthenticationManager.Instance.OnLoginSuccess -= OnLoginSuccess;
            AuthenticationManager.Instance.OnLoginFailed -= OnLoginFailed;
            AuthenticationManager.Instance.OnLogoutSuccess -= OnLogoutSuccess;
        }
    }

    // ========== ОБРАБОТЧИКИ КНОПОК ==========

    private void OnGoogleSignInClick()
    {
        UpdateStatusText("Вход через Google...");
        ShowLoadingScreen("Авторизация через Google");
        AuthenticationManager.Instance?.SignInWithGoogle();
    }

    private void OnAppleSignInClick()
    {
        UpdateStatusText("Вход через Apple...");
        ShowLoadingScreen("Авторизация через Apple");
        AuthenticationManager.Instance?.SignInWithApple();
    }

    private void OnSignOutClick()
    {
        AuthenticationManager.Instance?.SignOut();
    }

    // ========== ОБРАБОТЧИКИ СОБЫТИЙ ==========

    private void OnLoginSuccess(UserData userData)
    {
        Debug.Log($"UI: Успешный вход - {userData}");
        ShowProfileScreen(userData);
        UpdateStatusText($"Добро пожаловать, {userData.userName}!");
    }

    private void OnLoginFailed(string error)
    {
        Debug.LogWarning($"UI: Ошибка входа - {error}");
        ShowLoginScreen();
        UpdateStatusText($"Ошибка: {error}");
    }

    private void OnLogoutSuccess()
    {
        Debug.Log("UI: Выход выполнен");
        ShowLoginScreen();
        UpdateStatusText("Выход выполнен");
    }

    // ========== УПРАВЛЕНИЕ ЭКРАНАМИ ==========

    private void ShowLoginScreen()
    {
        loginPanel?.SetActive(true);
        profilePanel?.SetActive(false);
        loadingPanel?.SetActive(false);
    }

    private void ShowProfileScreen(UserData userData)
    {
        loginPanel?.SetActive(false);
        profilePanel?.SetActive(true);
        loadingPanel?.SetActive(false);

        // Обновление информации о пользователе
        if (userNameText != null)
            userNameText.text = $"Имя: {userData.userName}";

        if (userIdText != null)
            userIdText.text = $"ID: {userData.userId}";

        if (providerText != null)
            providerText.text = $"Провайдер: {userData.provider}";
    }

    private void ShowLoadingScreen(string message)
    {
        loginPanel?.SetActive(false);
        profilePanel?.SetActive(false);
        loadingPanel?.SetActive(true);

        if (loadingText != null)
            loadingText.text = message;
    }

    private void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log($"Status: {message}");
    }
}