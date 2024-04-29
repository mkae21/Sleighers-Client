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
    private int myPlayerIndex;
#endregion

#region PublicVariables
    static public WorldManager instance;
    public GameObject playerPool;
    public Transform[] startingPoints;
#endregion

#region PrivateMethod
    private void OnApplicationQuit()
    {
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
    }
    private void Update()
    {
        if (stream.DataAvailable)
        {
            OnReceive();
        }
    }
    public bool InitializeGame()
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

    private void OnReceive()
    {
        byte[] data = new byte[1024];
        int bytesRead = stream.Read(data, 0, data.Length);
        ByteReader br = new ByteReader(data);
        Protocol.Type type = (Protocol.Type)br.ReadInt();
        Debug.Log("[OnReceive] 프로토콜 : " + type);
        
        switch(type)
        {
            case Protocol.Type.s_NewUser:
                int newId = br.ReadInt();
                ProcessNewPlayer(newId);
                break;

            case Protocol.Type.s_PlayerConnect:
                myPlayerIndex = br.ReadInt();
                InputManager.instance.playerIndex = myPlayerIndex;
                int totalPlayerNumber = br.ReadInt();
                ProcessPlayerConnect(br, totalPlayerNumber, myPlayerIndex);
                break;
                
            case Protocol.Type.s_PlayerPosition:
                int userIndex = br.ReadInt();
                Vector3 moveVector = br.ReadVector3();
                ProcessPlayerKeyEvent(userIndex, moveVector);
                break;
        }
    }

    private void ProcessNewPlayer(int _newId)
    {
        GameObject newInstance = Instantiate(playerPrefab, playerPool.transform.position, Quaternion.identity, playerPool.transform);
        players.Add(_newId, newInstance.GetComponent<Player>());
    }
    private void ProcessPlayerConnect(ByteReader _br, int _totalPlayerNumber, int _playerIndex)
    {
        GameObject myPlayer = Instantiate(playerPrefab, startingPoints[0].transform.position, Quaternion.identity, playerPool.transform);
        myPlayer.GetComponent<Player>().Initialize(true, _playerIndex, "Player" + _playerIndex);
        players.Add(_playerIndex, myPlayer.GetComponent<Player>());

        for (int i = 0; i < _totalPlayerNumber; i++)
        {
            int userIndex = _br.ReadInt();
            GameObject otherPlayer = Instantiate(playerPrefab, startingPoints[i + 1].transform.position, Quaternion.identity, playerPool.transform);
            otherPlayer.GetComponent<Player>().Initialize(false, userIndex, "Player" + userIndex);
            players.Add(userIndex, otherPlayer.GetComponent<Player>());
        }
    }
    private void ProcessPlayerKeyEvent(int userIndex, Vector3 moveVector)
    {
        players[userIndex].SetMoveVector(moveVector);
    }


#endregion

#region PublicMethod
    public Player GetMyPlayer()
    {
        return players[myPlayerIndex];
    }
#endregion
}