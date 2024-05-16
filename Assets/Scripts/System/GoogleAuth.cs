using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using System.Threading;
using UnityEngine;

public class GoogleAuth : MonoBehaviour
{
#region PrivateMethod
    private void Start()
    {
        GameManager.Lobby += GoogleOAuth;
    }
    private async void GoogleOAuth()
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

        if (!string.IsNullOrEmpty(profile.EmailAddress))
        {
            Debug.LogFormat("구글 로그인 성공 : {0}", profile.EmailAddress);
            PlayerInfo playerInfo = new PlayerInfo
            {
                email = profile.EmailAddress
            };
            ServerManager.instance.OnSendOutGame(API.Type.loginSucc, playerInfo);
            OutGameUI.instance.SuccLoginPanel();
        }
        else
        {
            Debug.LogError("사용자의 이메일 주소를 가져오는데 실패했습니다.");
        }
    }
#endregion
}
