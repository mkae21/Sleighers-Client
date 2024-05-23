using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using System.Threading;
using UnityEngine;

public class GoogleAuth : MonoBehaviour
{
#region PublicMethod
    public static async void GoogleOAuth()
    {
        var clientId = SecretLoader.googleAuth.id;
        var clientSecret = SecretLoader.googleAuth.secret;

        var scopes = new[] { GmailService.Scope.GmailReadonly };

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
            scopes,
            "user",
            CancellationToken.None);

        var service = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Sleighers"
        });

        var profile = await service.Users.GetProfile("me").ExecuteAsync();
        OutGameUI.instance.loadingPanel.SetActive(false);
        OutGameUI.instance.loginPanel.SetActive(true);

        if (!string.IsNullOrEmpty(profile.EmailAddress))
        {
            Debug.LogFormat("구글 로그인 성공 : {0}", profile.EmailAddress);
            PlayerInfo playerInfo = new PlayerInfo
            {
                email = profile.EmailAddress
            };
            ServerManager.instance.OnSendOutGame(API.Type.loginSucc, playerInfo);
            OutGameUI.instance.OnLobbyPanel();
            GameManager.Instance().ChangeState(GameManager.GameState.Lobby);
        }
        else
        {
            Debug.LogWarning("구글 로그인 실패");
        }
    }
#endregion
}