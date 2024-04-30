using System.Collections.Generic;
using UnityEngine;

namespace Protocol
{
    // 이벤트 타입
    public enum Type : byte
    {
        Key = 1,            // 키 입력
        PlayerMove = 30,    // 플레이어 이동
        PlayerReconnect,    // 플레이어 재접속
        PlayerDisconnect,   // 플레이어 접속 끊김

        OtherPlayerConnect, // 다른 플레이어 접속
        OtherPlayerReconnect, // 다른 플레이어 재접속
        GameSync, // 위치 동기화

        LoadGameScene = 50, // 인게임 씬 로드
        GameStartCountDown, // 게임 시작 전 카운트 다운
        GameStart, // 게임 시작
        PlayerGoal, // 플레이어 골인
        GameEndCountDown, // 1등 도착 후 카운트 다운 시작
        GameEnd, // 게임 종료
        ResetServer = 100, // 서버 리셋
    }

    // 키 이벤트 코드
    public static class KeyEventCode
    {
        public const int NONE = 0;
        public const int MOVE = 1;      // 이동 메시지
        public const int BREAK = 2;     // 브레이크 메시지
        public const int ITEM = 4;      // 아이템 사용 메시지
        public const int NO_MOVE = 8;   // 이동 멈춤 메시지
    }

    public class Message
    {
        public Type type;
        public int playerId;
        public Message(Type _type)
        {
            this.type = _type;
        }
    }

    public class KeyMessage : Message
    {
        public int keyData;
        public float x;
        public float y;
        public float z;

        public KeyMessage(int _id, int _data, Vector3 _pos) : base(Type.Key)
        {
            this.playerId = _id;
            this.keyData = _data;
            this.x = _pos.x;
            this.y = _pos.y;
            this.z = _pos.z;
        }
    }
    public class PlayerMoveMessage : Message
    {
        public float xPos;
        public float yPos;
        public float zPos;
        public float xDir;
        public float yDir;
        public float zDir;
        public PlayerMoveMessage(int _id, Vector3 _pos, Vector3 _dir) : base(Type.PlayerMove)
        {
            this.playerId = _id;
            this.xPos = _pos.x;
            this.yPos = _pos.y;
            this.zPos = _pos.z;
            this.xDir = _dir.x;
            this.yDir = _dir.y;
            this.zDir = _dir.z;
        }
    }

    public class GameSyncMessage : Message
    {
        public int host;
        public int count = 0;
        public float[] xPos = null;
        public float[] zPos = null;
        public bool[] onlineInfo = null;

        public GameSyncMessage(int _id, int count, float[] x, float[] z, bool[] online) : base(Type.GameSync)
        {
            this.host = _id;
            this.count = count;
            this.xPos = new float[count];
            this.zPos = new float[count];
            this.onlineInfo = new bool[count];

            for (int i = 0; i < count; ++i)
            {
                xPos[i] = x[i];
                zPos[i] = z[i];
                onlineInfo[i] = online[i];
            }
        }
    }
    public class LoadGameSceneMessage : Message
    {
        public int userCount;
        public List<int> userList;
        public bool isHost;
        public LoadGameSceneMessage(int _id, int _count, List<int> _userList, bool _host) : base(Type.LoadGameScene)
        {
            this.playerId = _id;
            this.userCount = _count;
            this.userList = new List<int>(_userList);
            this.isHost = _host;
        }
    }

    // public class PlayerSyncMessage : Message
    // {
    //     public int playerId;
    //     public Vector3 position;
    //     public Vector3 rotation;
    //     public PlayerSyncMessage(Protocol.Type _type, int _playerId, Vector3 _position, Vector3 _rotation) : base(_type)
    //     {
    //         this.type = _type;
    //         this.playerId = _playerId;
    //         this.position = _position;
    //         this.rotation = _rotation;
    //     }
    // }
    // public class PlayerSyncMessage : Message
    // {
    //     public int playerId;
    //     public Vector3 position;
    //     public Vector3 rotation;
    //     public PlayerSyncMessage(Protocol.Type _type, int _playerId, Vector3 _position, Vector3 _rotation) : base(_type)
    //     {
    //         this.type = _type;
    //         this.playerId = _playerId;
    //         this.position = _position;
    //         this.rotation = _rotation;
    //     }
    // }
}