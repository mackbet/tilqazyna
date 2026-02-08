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
    [SerializeField] private Button signOutButton;

    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Start()
    {
        // Подписка на события
        if (LoginManager.Instance != null)
        {
            LoginManager.Instance.OnLoginSuccess += OnLoginSuccess;
            LoginManager.Instance.OnLoginFailed += OnLoginFailed;
            LoginManager.Instance.OnLogoutSuccess += OnLogoutSuccess;
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
        if (LoginManager.Instance != null)
        {
            LoginManager.Instance.OnLoginSuccess -= OnLoginSuccess;
            LoginManager.Instance.OnLoginFailed -= OnLoginFailed;
            LoginManager.Instance.OnLogoutSuccess -= OnLogoutSuccess;
        }
    }

    // ========== ОБРАБОТЧИКИ КНОПОК ==========

    private void OnGoogleSignInClick()
    {
        UpdateStatusText("Вход через Google...");
        ShowLoadingScreen("Авторизация через Google");
#if UNITY_ANDROID
        LoginManager.Instance?.ManuallyLoginGooglePlayGames();
#else
        UpdateStatusText("Google Play Games доступен только на Android");
        ShowLoginScreen();
#endif
    }

    private void OnAppleSignInClick()
    {
        UpdateStatusText("Вход через Apple...");
        ShowLoadingScreen("Авторизация через Apple");
        // TODO: Реализовать Sign In with Apple для iOS
        UpdateStatusText("Apple Sign-In пока не реализован");
        ShowLoginScreen();
    }

    private void OnSignOutClick()
    {
        LoginManager.Instance?.SignOut();
    }

    // ========== ОБРАБОТЧИКИ СОБЫТИЙ ==========

    private void OnLoginSuccess(string odl)
    {
        string userName = LoginManager.Instance?.GetUserName() ?? "Игрок";
        Debug.Log($"UI: Успешный вход - {userName} (ID: {odl})");
        ShowProfileScreen(odl, userName);
        UpdateStatusText($"Добро пожаловать, {userName}!");
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

    private void ShowProfileScreen(string odl, string userName)
    {
        loginPanel?.SetActive(false);
        profilePanel?.SetActive(true);
        loadingPanel?.SetActive(false);

        if (userNameText != null)
            userNameText.text = $"Имя: {userName}";

        if (userIdText != null)
            userIdText.text = $"ID: {odl}";
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
