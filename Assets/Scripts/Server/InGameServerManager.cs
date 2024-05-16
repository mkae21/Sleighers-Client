using UnityEngine;
using System.Net.Sockets;
using Protocol;
using System;
/* InGameServeManager.cs
 * - 인게임 서버와의 통신을 관리
 */
public partial class ServerManager : MonoBehaviour
{
#region PrivateVariables
    private TcpClient client;
    private NetworkStream stream;
    private string inGameServerIP = string.Empty;
    private int inGameServerPort = 0;
#endregion

#region PublicVariables
    public bool isConnectInGame = false;
    public TcpClient Client
    {
        get 
        { 
            if (client == null)
            {
                Debug.LogWarningFormat("[ServerManager] Client가 null입니다.");
                return null;
            }
            return client;
        }
        set { client = value; }
    }
    public NetworkStream Stream
    {
        get
        {
            if (stream == null)
            {
                Debug.LogWarningFormat("[ServerManager] Stream이 null입니다.");
                return null;
            }
            return stream; 
        }
        set { stream = value; }
    }
#endregion

#region PublicMethod
    public void ConnectInGame()
    {
        try
        {
            Client = new TcpClient(inGameServerIP, inGameServerPort);
            Debug.LogFormat("[ServerManager] 인게임 서버 접속 성공 {0}:{1}", inGameServerIP, inGameServerPort);
            Stream = Client.GetStream();
            isConnectInGame = true;
            Message msg = new Message(Protocol.Type.Login, roomData.roomID, UserData.instance.nickName);
            SendDataToInGame(msg);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ServerManager] 인게임 서버 접속 실패: " + e.Message);
            isConnectInGame = false;
            return;
        }
    }
    public void DisconnectInGame()
    {
        if (isConnectInGame)
        {
            Stream.Close();
            Client.Close();
            isConnectInGame = false;
            Debug.LogFormat("[ServerManager] 인게임 서버 접속 해제 {0}:{1}", inGameServerIP, inGameServerPort);
        }
        else
            Debug.LogFormat("[ServerManager] 인게임 서버에 접속중이지 않습니다. {0}:{1}", inGameServerIP, inGameServerPort);
    }
    // 서버로 데이터 전송
    public void SendDataToInGame<T>(T msg)
    {
        var byteArray = DataParser.DataToJsonData<T>(msg);
        Stream.WriteAsync(byteArray, 0, byteArray.Length);
    }
#endregion
}
