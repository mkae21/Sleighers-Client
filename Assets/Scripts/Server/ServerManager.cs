using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class ServerManager : MonoBehaviour
{


#region PrivateVariables
    private static ServerManager instance = null;
    private Socket socket;
    private string serverIP = string.Empty;
    private int serverPort = 0;
#endregion

#region PublicVariables

#endregion

#region PrivateMethod
    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        Init();
    }
    private void Init()
    {
        serverIP = SecretLoader.s_serverIp;
        serverPort = SecretLoader.s_serverPort;
        Debug.LogFormat("[ServerManager] Init - {0}:{1}", serverIP, serverPort);
        // 서버에 연결
        try
        {
            using (TcpClient client = new TcpClient(serverIP, serverPort))
            {
                InfiniteLoopDetector.Run();
                // client가 서버에 잘 접속이 되었는지 확인
                Debug.LogFormat("서버에 연결 {0}", client.Connected ? "성공" : "실패");
                using (NetworkStream stream = client.GetStream())
                {
                   // 메시지 전송
                   string message = "Hello from Unity - JJM!";
                   byte[] data = Encoding.UTF8.GetBytes(message);
                   stream.Write(data, 0, data.Length);

                   // 응답 수신
                   byte[] buffer = new byte[1024];
                   int bytesRead = stream.Read(buffer, 0, buffer.Length);
                   string responseData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                   // 응답 메시지 표시
                   Debug.LogFormat("Received: {0}", responseData);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
    }
#endregion

#region PublicMethod
    public static ServerManager Instance()
    {
        if (instance == null)
        {
            Debug.LogError("ServerManager Instace is Null");
            return null;
        }

        return instance;
    }
#endregion

}
