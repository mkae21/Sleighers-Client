using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using System.Threading;
using UnityEngine;

public class GoogleAuth : MonoBehaviour
{

#region PrivateVariables
#endregion

#region PublicMethod

#endregion

#region PrivateMethod
    public async void GoogleOAuth()
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
            // 서버로 이메일 주소 전송
            Debug.Log("이메일 주소 :" + profile.EmailAddress);
            OutGameServerManager.instance.LoginSucc(profile.EmailAddress);
            OutGameUI.instance.panels[0].SetActive(false);  // auth panel
            OutGameUI.instance.panels[1].SetActive(true);   // lobby panel
            OutGameUI.instance.topBar.SetActive(true);

        }
        else
        {
            Debug.LogError("사용자 이메일 주소를 가져오는데 실패했습니다.");
        }
    }
#endregion
}
