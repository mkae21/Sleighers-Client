using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Protocol;
using Reader;

public class WorldManager : MonoBehaviour
{
#region PrivateVariables
    private NetworkStream stream = null;
    private GameObject playerPrefab;
    private Dictionary<int, Player> players;
    private int myPlayerId;
#endregion

#region PublicVariables
    static public WorldManager instance;
    public GameObject playerPool;
    public Transform[] startingPoints;
#endregion

#region PrivateMethod
    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        ServerManager.Instance().SetIsConnect(false);
        if (stream != null)
            stream.Close();
        if (ServerManager.Instance().GetTcpClient() != null)
            ServerManager.Instance().CloseTcpClient();
    }
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        stream = ServerManager.Instance().GetStream();
        players = new Dictionary<int, Player>();
        InitializeGame();
        GameManager.InGame += SyncPosition;
    }
    private void Update()
    {
        if (stream.DataAvailable)
            OnReceive();
    }
    private bool InitializeGame()
    {

        if (!playerPool)
        {
            Debug.Log("[World Manager] Player Pool이 존재하지 않습니다.");
            return false;
        }
        Debug.Log("[World Manager] 게임 초기화 진행");

        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;

        return true;
    }
    private void SyncPosition()
    {
        PlayerMoveMessage playerMoveMessage = new PlayerMoveMessage(Type.SyncPosition, myPlayerId, players[myPlayerId].transform.position);
        byte[] data = new byte[1024];
        ByteWriter bw = new ByteWriter(data);
        bw.WriteBytes(bw.PlayerMoveMessageToByte(playerMoveMessage));
        ServerManager.Instance().SendMessage(data);
    }

    private void OnReceive()
    {
        // byte[] data = new byte[1024];
        // int bytesRead = stream.Read(data, 0, data.Length);
        // Debug.Log("[OnReceive] 데이터 수신 : " + bytesRead);
        // if (bytesRead <= 0)
        // {
        //     Debug.LogWarning("[OnReceive] 데이터 <= 0");
        //     data = null; // 메모리 명시적 해제
        //     return;
        // }

        byte[] data = new byte[1024];
        int bytesRead = stream.Read(data, 0, data.Length);
        ByteReader br = new ByteReader(data);
        Protocol.Type type = (Protocol.Type)br.ReadByte();
        Debug.Log("[OnReceive] 프로토콜 : " + type);
        
        switch(type)
        {
            case Protocol.Type.PlayerMove:
                ProcessPlayerMoveEvent(br);
                break;
            case Protocol.Type.OtherPlayerConnect:
                ProcessOtherPlayerConnectEvent(br);
                break;
            case Protocol.Type.LoadGameScene:
                ProcessLoadGameSceneEvent(br);
                break;
            case Protocol.Type.PlayerDisconnect:
                ProcessPlayerDisconnectEvent(br);
                break;
            case Protocol.Type.SyncPosition:
                ProcessSyncPositionEvent(br);
                break;
            default:
                Debug.LogWarning("[OnReceive] 알 수 없는 프로토콜");
                break;
        }
    }

    private void ProcessPlayerMoveEvent(ByteReader br)
    {
        int userId = br.ReadInt();
        Vector3 movePosition = br.ReadVector3();
        players[userId].SetMoveVector(movePosition);
    }
    private void ProcessOtherPlayerConnectEvent(ByteReader br)
    {
        int newId = br.ReadInt();
        GameObject newInstance = Instantiate(playerPrefab, startingPoints[0].transform.position, Quaternion.identity, playerPool.transform);
        players.Add(newId, newInstance.GetComponent<Player>());
    }
    private void ProcessLoadGameSceneEvent(ByteReader br)
    {
        myPlayerId = br.ReadInt();
        int totalPlayerCount = br.ReadInt();
        InputManager.instance.playerId = myPlayerId;

        GameObject myPlayer = Instantiate(playerPrefab, startingPoints[0].transform.position, Quaternion.identity, playerPool.transform);
        myPlayer.GetComponent<Player>().Initialize(true, myPlayerId, "Player" + myPlayerId);
        players.Add(myPlayerId, myPlayer.GetComponent<Player>());
        Debug.LogFormat("[WorldManager] 내 플레이어 생성 완료 : {0}", myPlayerId);
        for (int i = 0; i < totalPlayerCount; i++)
        {
            int userIndex = br.ReadInt();
            Vector3 otherPlayerPosition = br.ReadVector3();
            
            GameObject otherPlayer = Instantiate(playerPrefab, otherPlayerPosition, Quaternion.identity, playerPool.transform);
            otherPlayer.GetComponent<Player>().Initialize(false, userIndex, "Player" + userIndex);
            players.Add(userIndex, otherPlayer.GetComponent<Player>());
        }
    }
    private void ProcessPlayerDisconnectEvent(ByteReader br)
    {
        int userId = br.ReadInt();
        Destroy(players[userId].gameObject);
        players.Remove(userId);
    }
    private void ProcessSyncPositionEvent(ByteReader br)
    {
        int userId = br.ReadInt();
        Vector3 syncPosition = br.ReadVector3();
        players[userId].transform.position = syncPosition;
    }


#endregion

#region PublicMethod
    public Player GetMyPlayer()
    {
        return players[myPlayerId];
    }
    public Player GetMyPlayerFromId(int _id)
    {
        return players[_id];
    }
#endregion
}