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
        Sync = 30,              // 게임 싱크
        ResetServer = 100,      // 서버 리셋
        Receiver = 255          // 수신 전용
    }

    public class Message
    {
        public Type type;
        public string from;
        public int roomID;
        public Message(Type _type, string _id, int _roomID)
        {
            this.type = _type;
            this.from = _id;
            this.roomID = _roomID;
        }
    }
    public class SyncMessage : Message
    {
        public Vector3 position;        // 위치
        public Vector3 velocity;        // 속도
        public float rotation;          // 회전 (y축)
        public long timeStamp;          // 타임스탬프
        public SyncMessage(string _id, int _roomID, Vector3 _p, Vector3 _v, float _rY, long _timeStamp) : base(Type.Sync, _id, _roomID)
        {
            this.from = _id;
            this.roomID = _roomID ;
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
        public LoadGameSceneMessage(string _id, int _roomID, int _count, List<int> _userList) : base(Type.Receiver, _id, _roomID)
        {
            this.from = _id;
            this.roomID = _roomID ;
            this.count = _count;
            this.list = new List<int>(_userList);
        }
    }

    public class GameCountDownMessage : Message
    {
        public int count;
        public GameCountDownMessage(string _id, int _roomID, int _count) : base(Type.Receiver, _id, _roomID)
        {
            this.from = _id;
            this.roomID = _roomID;
            this.count = _count;
        }
    }
    public struct PlayerResult
    {
        public int rank;
        public string nickname;
        public float time;
        public PlayerResult(string _nickname, int _rank, float _time)
        {
            this.rank = _rank;
            this.nickname = _nickname;
            this.time = _time;
        }
    }
    public class GameResultMessage : Message
    {
        public List<PlayerResult> resultList;
        public GameResultMessage(string _id, int _roomID, List<PlayerResult> _resultList) : base(Type.Receiver, _id, _roomID)
        {
            this.from = _id;
            this.roomID= _roomID;
            this.resultList = new List<PlayerResult>(_resultList);
        }
    }
}