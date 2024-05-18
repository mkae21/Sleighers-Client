using System.Collections.Generic;
using UnityEngine;

namespace Protocol
{
    // 이벤트 타입
    public enum Type : byte
    {
        PlayerReady = 1,        // 인게임 로딩 완료
        GameSetUp,              // 게임에 참가한 플레이어들 생성
        GameStartCountDown,     // 게임 시작 카운트 다운
        GameStart,              // 게임 시작
        PlayerDisconnect,       // 플레이어 접속 끊김
        PlayerGoal,             // 플레이어 골인
        GameEndCountDown,       // 1등 도착 후 카운트 다운 시작
        GameEnd,                // 게임 종료
        Sync = 30,              // 게임 싱크
        Receiver = 255          // 수신 전용
    }

    public class Message
    {
        public Type type;
        public int roomID;
        public string from;
        public Message(Type _type, int _roomID, string _id)
        {
            this.type = _type;
            this.roomID = _roomID;
            this.from = _id;
        }
    }
    public class SyncMessage : Message
    {
        public Vector3 position;        // 위치
        public Vector3 velocity;        // 속도
        public float rotation;          // 회전 (y축)
        public long timeStamp;          // 타임스탬프
        public SyncMessage(int _roomID, string _id, Vector3 _p, Vector3 _v, float _rY, long _timeStamp) : base(Type.Sync, _roomID, _id)
        {
            this.roomID = _roomID;
            this.from = _id;
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
        public LoadGameSceneMessage(int _roomID, string _id, int _count, List<int> _userList) : base(Type.Receiver, _roomID, _id)
        {
            this.roomID = _roomID ;
            this.from = _id;
            this.count = _count;
            this.list = new List<int>(_userList);
        }
    }

    public class GameCountDownMessage : Message
    {
        public int count;
        public GameCountDownMessage(int _roomID, string _id, int _count) : base(Type.Receiver, _roomID, _id)
        {
            this.roomID = _roomID;
            this.from = _id;
            this.count = _count;
        }
    }
    public struct PlayerResult
    {
        public int rank;
        public string nickname;
        public float goalTime;
    }

    public struct GameEndStruct
    {
        public Type type;
        public int roomID;
        public string from;
        public List<PlayerResult> resultList;
        public float endTime;
    }

    public class GameEndMessage : Message
    {
        public List<PlayerResult> resultList;
        public float endTime;
        public GameEndMessage(int _roomID, string _id, List<PlayerResult> _resultList, float _endTime) : base(Type.GameEnd, _roomID, _id)
        {
            this.roomID= _roomID;
            this.from = _id;
            this.resultList = new List<PlayerResult>(_resultList);
            this.endTime = _endTime;
        }
    }
}