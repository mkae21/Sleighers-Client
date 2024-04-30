using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Protocol;
using Reader;
/* WorldManager.cs
 * - 인게임 내의 모든 것을 관리
 * - 인게임 내에서 프로토콜 수신 및 처리
 * - 인게임 내에서 플레이어 생성 및 삭제
 */
public class WorldManager : MonoBehaviour
{
    class SessionInfo
    {
        public int totalPlayerCount;
    }
#region PrivateVariables
    private NetworkStream stream = null;
    private GameObject playerPrefab;
    private SessionInfo sessionInfo;
    private Dictionary<int, Player> players;
    private int myPlayerId;
    private bool isGameStart = false;
#endregion

#region PublicVariables
    static public WorldManager instance;
    public GameObject playerPool;
    public Transform[] startingPoints;
#endregion

#region PrivateMethod
    // 애플리케이션 종료 시
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
        sessionInfo = new SessionInfo();
        InitializeGame();
        GameManager.InGame += SendSyncPositionEvent;
    }
    private void Update()
    {
        if (stream.DataAvailable)
            OnReceive();
    }
    // 인게임 초기화
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


    // 서버로부터 받는 데이터 처리 핸들러
    private void OnReceive()
    {
        byte[] data = new byte[1024];
        _ = stream.Read(data, 0, data.Length);
        ByteReader br = new ByteReader(data);
        Protocol.Type type = (Protocol.Type)br.ReadByte();

        if (type != Protocol.Type.SyncPosition)
            Debug.Log("[OnReceive] 프로토콜 : " + type);
        
        switch(type)
        {
            case Protocol.Type.PlayerMove:
                ReceivePlayerMoveEvent(br);
                break;

            case Protocol.Type.PlayerReconnect:
                break;

            case Protocol.Type.PlayerDisconnect:
                ReceivePlayerDisconnectEvent(br);
                break;

            case Protocol.Type.OtherPlayerConnect:
                ReceiveOtherPlayerConnectEvent(br);
                break;

            case Protocol.Type.OtherPlayerReconnect:
                break;

            case Protocol.Type.SyncPosition:
                ReceiveSyncPositionEvent(br);
                break;

            case Protocol.Type.LoadGameScene:
                ReceiveLoadGameSceneEvent(br);
                break;

            case Protocol.Type.GameStartCountDown:
                ReceiveGameStartCountDownEvent(br);
                break;

            case Protocol.Type.GameStart:
                ReceiveGameStartEvent(br);
                break;

            case Protocol.Type.PlayerGoal:
                break;

            case Protocol.Type.GameEndCountDown:
                break;

            case Protocol.Type.GameEnd:
                ReceiveGameEndEvent(br);
                break;

            default:
                Debug.LogWarning("[OnReceive] 알 수 없는 프로토콜");
                break;
        }
    }
#region Receive 프로토콜 처리
    // 플레이어 이동 이벤트 처리
    private void ReceivePlayerMoveEvent(ByteReader br)
    {
        int userId = br.ReadInt();
        Vector3 movePosition = br.ReadVector3();
        players[userId].SetMoveVector(movePosition);
    }
    // 다른 플레이어 접속 이벤트 처리
    private void ReceiveOtherPlayerConnectEvent(ByteReader br)
    {
        int newId = br.ReadInt();
        GameObject newInstance = Instantiate(playerPrefab, startingPoints[sessionInfo.totalPlayerCount].transform.position, Quaternion.identity, playerPool.transform);
        players.Add(newId, newInstance.GetComponent<Player>());
        sessionInfo.totalPlayerCount++;
    }
    // 게임 씬 로드 이벤트 처리
    private void ReceiveLoadGameSceneEvent(ByteReader br)
    {
        myPlayerId = br.ReadInt();
        int totalPlayerCount = br.ReadInt();
        sessionInfo.totalPlayerCount = totalPlayerCount + 1;
        InputManager.instance.playerId = myPlayerId;

        GameObject myPlayer = Instantiate(playerPrefab, startingPoints[totalPlayerCount].transform.position, startingPoints[0].transform.rotation, playerPool.transform);
        myPlayer.GetComponent<Player>().Initialize(true, myPlayerId, "Player" + myPlayerId);
        players.Add(myPlayerId, myPlayer.GetComponent<Player>());
        Debug.LogFormat("[WorldManager] 내 플레이어 생성 완료 : {0}", myPlayerId);
        for (int i = 0; i < totalPlayerCount; i++)
        {
            int userIndex = br.ReadInt();            
            GameObject otherPlayer = Instantiate(playerPrefab, startingPoints[i].transform.position, Quaternion.identity, playerPool.transform);
            otherPlayer.GetComponent<Player>().Initialize(false, userIndex, "Player" + userIndex);
            players.Add(userIndex, otherPlayer.GetComponent<Player>());
        }
    }
    // 게임 시작 카운트 다운 이벤트 처리
    private void ReceiveGameStartCountDownEvent(ByteReader br)
    {
        int count = br.ReadInt();
        Debug.LogFormat("[OnReceive] GameStartCountDownEvent : {0}", count);
    }
    // 게임 시작 이벤트 처리
    private void ReceiveGameStartEvent(ByteReader br)
    {
        Debug.LogFormat("[OnReceive] ReceiveGameStartEvent");
        GameManager.Instance().ChangeState(GameManager.GameState.InGame);
    }
    // 다른 플레이어 접속 끊김 이벤트 처리
    private void ReceivePlayerDisconnectEvent(ByteReader br)
    {
        int userId = br.ReadInt();
        Destroy(players[userId].gameObject);
        players.Remove(userId);
    }
    // 플레이어 위치 및 회전 동기화 이벤트 처리
    private void ReceiveSyncPositionEvent(ByteReader br)
    {
        int userId = br.ReadInt();
        Vector3 syncPosition = br.ReadVector3();
        Vector3 syncRotation = br.ReadVector3();
        players[userId].transform.position = syncPosition;
        players[userId].transform.rotation = Quaternion.Euler(syncRotation);
    }
    // 게임 종료 이벤트 처리
    private void ReceiveGameEndEvent(ByteReader br)
    {
        int userId = br.ReadInt();
        Debug.LogFormat("플레이어 {0}가 승리했습니다.", userId);
    }
#endregion

#region Send 프로토콜 처리
    // 내 위치와 회전을 서버에 보내서 모든 플레이어에게 동기화
    private void SendSyncPositionEvent()
    {
        Transform myPlayer = players[myPlayerId].gameObject.transform;
        PlayerSyncMessage playerSyncMessage = new PlayerSyncMessage(Protocol.Type.SyncPosition, myPlayerId, myPlayer.position, myPlayer.rotation.eulerAngles);
        byte[] data = new byte[1024];
        ByteWriter bw = new ByteWriter(data);
        byte[] message = bw.PlayerSyncMessageToByte(playerSyncMessage);
        bw.WriteBytes(message);
        ServerManager.Instance().SendMessage(data);
    }
    // 게임 시작 이벤트를 서버에 알림
    private void SendGameStartEvent()
    {
        if (isGameStart)
            return;
        isGameStart = true;
        Debug.LogFormat("[OnSend] SendGameStartEvent");
        byte[] data = new byte[1024];
        ByteWriter bw = new ByteWriter(data);
        bw.WriteByte((byte)Protocol.Type.GameStart);
        ServerManager.Instance().SendMessage(data);
    }
    // 내 플레이어가 골인했음을 서버에 알림
    private void SendPlayerGoalEvent()
    {
        Debug.LogFormat("[OnSend] SendPlayerGoalEvent");
        byte[] data = new byte[1024];
        ByteWriter bw = new ByteWriter(data);
        bw.WriteByte((byte)Protocol.Type.PlayerGoal);
        bw.WriteInt(myPlayerId);
        ServerManager.Instance().SendMessage(data);
    }
    // 임시 서버 리셋
    private void SendResetServerEvent()
    {
        Debug.LogFormat("[OnSend] SendResetServerEvent");
        byte[] data = new byte[1024];
        ByteWriter bw = new ByteWriter(data);
        bw.WriteByte((byte)Protocol.Type.ResetServer);
        ServerManager.Instance().SendMessage(data);
    }
#endregion


#endregion

#region PublicMethod
    // 서버로 보내는 데이터 처리 핸들러
    public void OnSend(Protocol.Type _type)
    {
        switch (_type)
        {
            case Protocol.Type.PlayerMove:
                break;

            case Protocol.Type.PlayerReconnect:
                break;

            case Protocol.Type.PlayerDisconnect:
                break;

            case Protocol.Type.OtherPlayerConnect:
                break;
                
            case Protocol.Type.OtherPlayerReconnect:
                break;

            case Protocol.Type.SyncPosition:
                SendSyncPositionEvent();
                break;

            case Protocol.Type.LoadGameScene:
                break;

            case Protocol.Type.GameStartCountDown:
                break;

            case Protocol.Type.GameStart:
                SendGameStartEvent();
                break;

            case Protocol.Type.PlayerGoal:
                SendPlayerGoalEvent();
                break;

            case Protocol.Type.GameEndCountDown:
                break;

            case Protocol.Type.GameEnd:
                break;

            case Protocol.Type.ResetServer:
                SendResetServerEvent();
                break;

            default:
                Debug.LogWarning("[OnSend] 알 수 없는 프로토콜");
                break;
        }
    }

    public Player GetMyPlayer()
    {
        return players[myPlayerId];
    }
    public Player GetPlayerFromId(int _id)
    {
        return players[_id];
    }
#endregion
}