using UnityEngine;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

/// <summary>
/// Авторизация для Android через Google Play Games.
/// Бывший LoginManager.
/// </summary>
public class AndroidLoginManager : BaseLoginManager
{
    #region Состояние
    private static bool s_Activated = false;
    private string m_GooglePlayGamesToken;
    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnStart()
    {
        InitializeGooglePlayGames();
        TryAutoLoginGooglePlayGames();
    }
    #endregion

    #region Инициализация Google Play Games
    private void InitializeGooglePlayGames()
    {
#if UNITY_ANDROID
        if (!s_Activated)
        {
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
            s_Activated = true;
            Debug.Log("[Auth-Android] Google Play Games активирован");
        }
#endif
    }
    #endregion

    #region Автоматический вход
    private void TryAutoLoginGooglePlayGames()
    {
#if UNITY_ANDROID
        Debug.Log("[Auth-Android] Попытка автоматического входа через Google Play Games...");

        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("[Auth-Android] ========== GOOGLE PLAY GAMES СЕССИЯ ==========");
                Debug.Log($"[Auth-Android] ID игрока: {PlayGamesPlatform.Instance.GetUserId()}");
                Debug.Log($"[Auth-Android] Имя: {PlayGamesPlatform.Instance.GetUserDisplayName()}");
                Debug.Log("[Auth-Android] ===============================================");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    m_GooglePlayGamesToken = code;
                    SignInWithGooglePlayGames(tryLinkGuest: false);
                });
            }
            else
            {
                Debug.Log($"[Auth-Android] Автовход Google Play Games не удался (статус: {status}).");
                if (curtain != null) curtain.SetActive(false);
            }
        });
#else
        Debug.Log("[Auth-Android] Платформа не Android. Ожидание выбора способа авторизации.");
        if (curtain != null) curtain.SetActive(false);
#endif
    }
    #endregion

    #region Google Play Games
    public void LoginGooglePlayGames()
    {
#if UNITY_ANDROID
        Debug.Log("[Auth-Android] Попытка входа через Google Play Games...");

        PlayGamesPlatform.Instance.Authenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("[Auth-Android] Google Play Games: вход успешен");
                Debug.Log($"[Auth-Android] Имя: {PlayGamesPlatform.Instance.GetUserDisplayName()}");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    m_GooglePlayGamesToken = code;
                    SignInWithGooglePlayGames(tryLinkGuest: true);
                });
            }
            else
            {
                Debug.LogWarning($"[Auth-Android] Вход не удался. Статус: {status}");
                InvokeLoginFailed($"Google Play Games вход не удался: {status}");
            }
        });
#endif
    }

    public void ManuallyLoginGooglePlayGames()
    {
#if UNITY_ANDROID
        Debug.Log("[Auth-Android] Открытие диалога выбора аккаунта Google Play Games...");

        PlayGamesPlatform.Instance.ManuallyAuthenticate((status) =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("[Auth-Android] ========== ВХОД ВЫПОЛНЕН УСПЕШНО ==========");
                Debug.Log($"[Auth-Android] ID игрока: {PlayGamesPlatform.Instance.GetUserId()}");
                Debug.Log($"[Auth-Android] Имя: {PlayGamesPlatform.Instance.GetUserDisplayName()}");
                Debug.Log("[Auth-Android] ============================================");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    m_GooglePlayGamesToken = code;
                    SignInWithGooglePlayGames(tryLinkGuest: true);
                });
            }
            else
            {
                Debug.LogWarning($"[Auth-Android] Вход отменён или не удался. Статус: {status}");
                InvokeLoginFailed($"Вход отменён: {status}");
            }
        });
#endif
    }
    #endregion

    #region Unity Authentication
    private async void SignInWithGooglePlayGames(bool tryLinkGuest)
    {
        if (string.IsNullOrEmpty(m_GooglePlayGamesToken))
        {
            Debug.LogWarning("[Auth-Android] Код авторизации пустой!");
            InvokeLoginFailed("Код авторизации пустой");
            return;
        }

        try
        {
            bool hasGuestSession = AuthenticationService.Instance.SessionTokenExists;

            if (tryLinkGuest && hasGuestSession && !AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("[Auth-Android] Найден гостевой аккаунт. Пытаемся привязать к Google...");

                try
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    string guestId = AuthenticationService.Instance.PlayerId;
                    Debug.Log($"[Auth-Android] Вошли в гостевой аккаунт: {guestId}");

                    await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
                    Debug.Log("[Auth-Android] Гостевой аккаунт успешно привязан к Google Play Games!");
                }
                catch (Exception linkEx) when (linkEx is AuthenticationException || linkEx is RequestFailedException)
                {
                    bool isInvalidTokenError = linkEx.Message.Contains("INVALID_SESSION_TOKEN") ||
                                               linkEx.Message.Contains("401") ||
                                               linkEx.Message.Contains("session token is not valid");

                    if (isInvalidTokenError)
                        Debug.LogWarning($"[Auth-Android] Гостевой аккаунт недействителен: {linkEx.Message}");
                    else
                        Debug.LogWarning($"[Auth-Android] Не удалось привязать гостевой аккаунт: {linkEx.Message}");

                    Debug.Log("[Auth-Android] Очищаем гостевую сессию и входим через Google...");

                    if (AuthenticationService.Instance.IsSignedIn)
                        AuthenticationService.Instance.SignOut();

                    AuthenticationService.Instance.ClearSessionToken();

                    await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
                }
            }
            else if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("[Auth-Android] Вход в Unity Services через Google Play Games...");
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(m_GooglePlayGamesToken);
            }

            isAuthenticated = true;
            string playerId = AuthenticationService.Instance.PlayerId;

            Debug.Log("[Auth-Android] ========== UNITY AUTH УСПЕШНО ==========");
            Debug.Log($"[Auth-Android] Unity Player ID: {playerId}");
            Debug.Log($"[Auth-Android] Гостевой: {IsGuest}");
            Debug.Log("[Auth-Android] =========================================");

            InvokeLoginSuccess(playerId);
            if (autoLoadSceneOnLogin) LoadGameScene();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"[Auth-Android] Ошибка аутентификации: {ex.Message}");
            InvokeLoginFailed(ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"[Auth-Android] Ошибка запроса: {ex.Message}");
            InvokeLoginFailed(ex.Message);
        }
    }
    #endregion

    #region Выход
    public override void SignOut()
    {
        base.SignOut();
        // Дополнительная логика для Android если нужна
    }
    #endregion

    #region Публичные свойства
    public override string GetUserName()
    {
#if UNITY_ANDROID
        if (PlayGamesPlatform.Instance.IsAuthenticated())
            return PlayGamesPlatform.Instance.GetUserDisplayName();
#endif
        return null;
    }

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
