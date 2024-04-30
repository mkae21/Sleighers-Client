using UnityEngine;

namespace Protocol
{
    public enum Type : byte
    {
        PlayerMove = 30, // 플레이어 이동
        PlayerReconnect, // 플레이어 재접속
        PlayerDisconnect, // 플레이어 접속 끊김

        OtherPlayerConnect, // 다른 플레이어 접속
        OtherPlayerReconnect, // 다른 플레이어 재접속
        SyncPosition, // 위치 동기화

        LoadGameScene = 50, // 인게임 씬 로드
        GameStartCountDown, // 게임 시작 전 카운트 다운
        GameStart, // 게임 시작
        PlayerGoal, // 플레이어 골인
        GameEndCountDown, // 1등 도착 후 카운트 다운 시작
        GameEnd, // 게임 종료
        ResetServer = 100, // 서버 리셋
    }
    public class Message
    {
        public Type type;
        public Message(Type type)
        {
            this.type = type;
        }
    }
    public class PlayerSyncMessage : Message
    {
        public int playerId;
        public Vector3 position;
        public Vector3 rotation;
        public PlayerSyncMessage(Protocol.Type _type, int _playerId, Vector3 _position, Vector3 _rotation) : base(_type)
        {
            this.type = _type;
            this.playerId = _playerId;
            this.position = _position;
            this.rotation = _rotation;
        }
    }
}