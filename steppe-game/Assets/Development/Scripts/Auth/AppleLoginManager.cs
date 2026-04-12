using UnityEngine;

#if UNITY_IOS
using Apple.GameKit;
#endif

/// <summary>
/// Авторизация для iOS через Apple Game Center.
/// </summary>
public class AppleLoginManager : BaseLoginManager
{
    #region Состояние
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
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnStart()
    {
        TryAutoLoginGameCenter();
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

        m_Signature    = Convert.ToBase64String(fetchResult.GetSignature());
        m_Salt         = Convert.ToBase64String(fetchResult.GetSalt());
        m_PublicKeyUrl = fetchResult.PublicKeyUrl;
        m_Timestamp    = fetchResult.Timestamp;
        m_TeamPlayerId = localPlayer.TeamPlayerId;

        Debug.Log("[Auth-Apple] Game Center credentials получены");
        Debug.Log($"[Auth-Apple] TeamPlayerId: {m_TeamPlayerId}");
    }

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
                InvokeLoginFailed("Game Center вход не удался");
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
            InvokeLoginFailed(e.Message);
        }
    }
#endif
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

            InvokeLoginSuccess(playerId);
            if (autoLoadSceneOnLogin) LoadGameScene();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"[Auth-Apple] Ошибка аутентификации: {ex.Message}");
            InvokeLoginFailed(ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"[Auth-Apple] Ошибка запроса: {ex.Message}");
            InvokeLoginFailed(ex.Message);
        }
#endif
    }
    #endregion

    #region Выход
    public override void SignOut()
    {
        base.SignOut();
        isGameCenterAuthenticated = false;
    }
    #endregion

    #region Публичные свойства
    public override string GetUserName()
    {
#if UNITY_IOS
        if (isGameCenterAuthenticated)
            return GKLocalPlayer.Local.DisplayName;
#endif
        return null;
    }

    public void LoginGameCenterButton()
    {
#if UNITY_IOS
        LoginGameCenter();
#endif
    }
    #endregion
}
