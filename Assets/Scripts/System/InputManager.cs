using UnityEngine;
using Protocol;
/* InputManager.cs
 * - 인게임에서 플레이어 입력 처리
 * - 플레이어 스크립트는 여러 개 생성되기에 여기서 플레이어의 입력을 받아서 서버로 전송 및 플레이어 이동 처리
 */
public class InputManager : MonoBehaviour    
{
#region PrivateVariables
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
#endregion

#region PublicVariables
    static public InputManager instance;
#endregion

#region PrivateMethod
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        GameManager.Ready += ReadyInput;
        GameManager.InGame += PlayerInput;
        GameManager.InGame += BreakInput;
    }
    private void ReadyInput()
    {
        if (ServerManager.Instance().IsHost() == false)
            return;
        if (Input.GetKey(KeyCode.R))
            WorldManager.instance.OnSend(Protocol.Type.ResetServer);
        if (Input.GetKey(KeyCode.S))
            WorldManager.instance.OnSend(Protocol.Type.GameStart);
    }

    private void PlayerInput()
    {
        if (Input.anyKey == false)
            return;
        if (ServerManager.Instance().IsHost() && Input.GetKey(KeyCode.R))
            WorldManager.instance.OnSend(Protocol.Type.ResetServer);
        float h = Input.GetAxis(HORIZONTAL);
        float v = Input.GetAxis(VERTICAL);

        int keyCode = 0;
        keyCode |= KeyEventCode.MOVE;
        if (keyCode <= 0)
            return;

        Vector3 moveVector = new Vector3(h, 0, v);
        moveVector = Vector3.Normalize(moveVector);

        KeyMessage msg = new KeyMessage(WorldManager.instance.MyPlayerId, keyCode, moveVector);
        if (ServerManager.Instance().IsHost())
            ServerManager.Instance().AddMsgToLocalQueue(msg);
        else
            ServerManager.Instance().SendDataToInGame<KeyMessage>(msg);
    }
    private void BreakInput()
    {
        if (!Input.GetButton("Jump"))
            return;
        int keyCode = 0;
        keyCode |= KeyEventCode.BREAK;
        if (keyCode <= 0)
            return;
        
        KeyMessage msg = new KeyMessage(WorldManager.instance.MyPlayerId, keyCode, Vector3.zero);
        if (ServerManager.Instance().IsHost())
            ServerManager.Instance().AddMsgToLocalQueue(msg);
        else
            ServerManager.Instance().SendDataToInGame<KeyMessage>(msg);
    }

#endregion

#region PublicMethod
#endregion
    
}
