using System.Collections.Generic;
using UnityEngine;

namespace Protocol
{
    // 이벤트 타입
    public enum Type : byte
    {
        Key = 1,            // 키 입력
        PlayerBreak = 28,    // 플레이어 브레이크
        PlayerMove = 30,    // 플레이어 이동
        PlayerReconnect,    // 플레이어 재접속
        PlayerDisconnect,   // 플레이어 접속 끊김

        OtherPlayerConnect, // 다른 플레이어 접속
        OtherPlayerReconnect, // 다른 플레이어 재접속
        GameSync, // 플레이어 재접속 시 게임 현재 상황 싱크

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
        public int id;
        public Message(Type _type, int _id)
        {
            this.type = _type;
            this.id = _id;
        }
    }

    public class KeyMessage : Message
    {
        public int keyData;
        public Vector3 position;
        public KeyMessage(int _id, int _data, Vector3 _pos) : base(Type.Key, _id)
        {
            this.id = _id;
            this.keyData = _data;
            this.position = _pos;
        }
    }
    public class PlayerMoveMessage : Message
    {
        public Vector3 position;
        public Vector3 direction;
        public PlayerMoveMessage(int _id, Vector3 _pos, Vector3 _dir) : base(Type.PlayerMove, _id)
        {
            this.id = _id;
            this.position = _pos;
            this.direction = _dir;
        }
    }
    public class PlayerBreakMessage : Message
    {
        public PlayerBreakMessage(int _id) : base(Type.PlayerBreak, _id)
        {
            this.id = _id;
        }
    }
    public class LoadGameSceneMessage : Message
    {
        public int count;
        public List<int> list;
        public bool ishost;
        public LoadGameSceneMessage(int _id, int _count, List<int> _userList, bool _host) : base(Type.LoadGameScene, _id)
        {
            this.id = _id;
            this.count = _count;
            this.list = new List<int>(_userList);
            this.ishost = _host;
        }
    }

    public class GameSyncMessage : Message
    {
        public int count = 0;
        public Vector3[] positions = null;
        public bool[] onlineInfo = null;

        public GameSyncMessage(int _host, int _count, Vector3[] _positions, bool[] _online) : base(Type.GameSync, _host)
        {
            this.id = _host;
            this.count = _count;
            this.onlineInfo = new bool[_count];

            for (int i = 0; i < _count; ++i)
            {
                positions[i] = _positions[i];
                onlineInfo[i] = _online[i];
            }
        }
    }

    public class GameStartCountDownMessage : Message
    {
        public int count;
        public GameStartCountDownMessage(int _id, int _count) : base(Type.GameStartCountDown, _id)
        {
            this.id = _id;
            this.count = _count;
        }
    }

    public class GameStartMessage : Message
    {
        public GameStartMessage(int _id) : base(Type.GameStart, _id) 
        { 
            this.id = _id;
        }
    }

    public class GameEndMessage : Message
    {
        public int count;
        public int[] sessionList;
        public GameEndMessage(int _id, Stack<int> result) : base(Type.GameEnd, _id)
        {
            this.id = _id;
            count = result.Count;
            sessionList = new int[count];
            for (int i = 0; i < count; ++i)
            {
                sessionList[i] = (int)result.Pop();
            }
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