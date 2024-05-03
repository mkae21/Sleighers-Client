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
        SendCountDown,          // 게임 시작/끝 카운트 다운
        GameStart,              // 게임 시작
        Key = 20,               // 키 입력
        Item,                   // 아이템 사용
        PlayerReconnect,        // 플레이어 재접속
        PlayerDisconnect,       // 플레이어 접속 끊김
        PlayerGoal,             // 플레이어 골인
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
        public Vector3 position;        // 위치
        public Vector3 velocity;        // 속도
        public Vector3 acceleration;    // 가속도
        public long timeStamp;
        public KeyMessage(int _id, Vector3 _p, Vector3 _v, Vector3 _a, long _timestamp) : base(Type.Key, _id)
        {
            this.from = _id;
            this.position = _p;
            this.velocity = _v;
            this.acceleration = _a;
            this.timeStamp = _timestamp;
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

    public class GameStartCountDownMessage : Message
    {
        public int count;
        public GameStartCountDownMessage(int _id, int _count) : base(Type.SendCountDown, _id)
        {
            this.from = _id;
            this.count = _count;
        }
    }

    public class GameStartMessage : Message
    {
        public GameStartMessage(int _id) : base(Type.GameStart, _id) 
        { 
            this.from = _id;
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