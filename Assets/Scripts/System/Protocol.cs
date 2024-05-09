using System.Collections.Generic;
using UnityEngine;

namespace Protocol
{
    // 이벤트 타입
    public enum Type : byte
    {
        Login = 1,              // 로그인
        Logout,                 // 로그아웃
        Signin,                 // 회원가입
        StartMatchMaking = 10,  // 매치메이킹 시작
        EnterWaitingRoom,       // 대기실 입장
        LoadGameScene,          // 인게임 접속
        PlayerReady,            // 로딩 완료
        GameStartCountDown,     // 게임 시작 카운트 다운
        GameStart,              // 게임 시작
        Key = 20,               // 키 입력
        Item,                   // 아이템 사용
        PlayerReconnect,        // 플레이어 재접속
        PlayerDisconnect,       // 플레이어 접속 끊김
        PlayerGoal,             // 플레이어 골인
        GameEndCountDown,       // 1등 도착 후 카운트 다운 시작
        GameEnd,                // 게임 종료
        GameSync,               // 게임 싱크
        ResetServer = 100,      // 서버 리셋
    }

    public class Message
    {
        public Type type;
        public int from;
        public Message(Type _type, int _id)
        {
            this.type = _type;
            this.from = _id;
        }
    }

    public class KeyMessage : Message
    {
        public Vector2 acceleration;    // 가속도
        public Vector3 position;        // 위치
        public Vector3 velocity;        // 속도
        public float rotation;          // 회전 (y축)
        public long timeStamp;
        public KeyMessage(int _id, Vector2 _a, Vector3 _p, Vector3 _v, float _rY, long _timeStamp) : base(Type.Key, _id)
        {
            this.from = _id;
            this.acceleration = _a;
            this.position = _p;
            this.velocity = _v;
            this.rotation = _rY;
            this.timeStamp = _timeStamp;
        }
    }

    public class LoadGameSceneMessage : Message
    {
        public int count;
        public List<int> list;
        public LoadGameSceneMessage(int _id, int _count, List<int> _userList) : base(Type.LoadGameScene, _id)
        {
            this.from = _id;
            this.count = _count;
            this.list = new List<int>(_userList);
        }
    }

    public class GameCountDownMessage : Message
    {
        public int count;
        public GameCountDownMessage(int _id, int _count) : base(Type.GameStartCountDown, _id)
        {
            this.from = _id;
            this.count = _count;
        }
    }

    public class GameEndMessage : Message
    {
        public int count;
        public int[] sessionList;
        public GameEndMessage(int _id, Stack<int> result) : base(Type.GameEnd, _id)
        {
            this.from = _id;
            count = result.Count;
            sessionList = new int[count];
            for (int i = 0; i < count; ++i)
            {
                sessionList[i] = (int)result.Pop();
            }
        }
    }
}