using UnityEngine;

namespace Protocol
{
    public enum Type : byte
    {
        PlayerMove = 30,
        PlayerReconnect,
        PlayerDisconnect,

        OtherPlayerConnect,
        OtherPlayerReconnect,
        SyncPosition,

        LoadGameScene = 50,
        GameStartCountDown, // 게임 시작 전 카운트 다운
        GameStart, // 게임 시작
        GameEndCountDown, // 1등 도착 후 카운트 다운 시작
        GameEnd, // 게임 종료
    }
    public class Message
    {
        public Type type;
        public Message(Type type)
        {
            this.type = type;
        }
    }
    public class PlayerMoveMessage : Message
    {
        public int playerId;
        public Vector3 position;
        public PlayerMoveMessage(int _playerId, Vector3 _position) : base(Type.PlayerMove)
        {
            this.playerId = _playerId;
            this.position = _position;
        }
        public PlayerMoveMessage(Protocol.Type _type, int _playerId, Vector3 _position) : base(_type)
        {
            this.type = _type;
            this.playerId = _playerId;
            this.position = _position;
        }
    }
}