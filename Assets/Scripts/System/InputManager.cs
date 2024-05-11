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
    private const string BRAKE = "Jump";
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
        GameManager.InGame += KeyInput;
    }
    private void ReadyInput()
    {
        if (Input.GetKey(KeyCode.R))
            WorldManager.instance.OnSend(Protocol.Type.ResetServer);
        if (Input.GetKey(KeyCode.T))
            WorldManager.instance.OnSend(Protocol.Type.GameStart);
    }

    private void KeyInput()
    {
        if (Input.GetKey(KeyCode.R))
            WorldManager.instance.OnSend(Protocol.Type.ResetServer);
        if (Input.GetKey(KeyCode.T))
            WorldManager.instance.OnSend(Protocol.Type.GameStart);
        if  (Input.GetKey(KeyCode.G))
            WorldManager.instance.OnSend(Protocol.Type.PlayerGoal);
        
        if (WorldManager.instance.isRaceFinish)
            return;
        
        float h = Input.GetAxis(HORIZONTAL);
        float v = Input.GetAxis(VERTICAL);
        bool drifting = Input.GetKey(KeyCode.LeftShift);

        if (h == 0 && v == 0)
            return;
        
        int id = WorldManager.instance.myPlayerId;
        Vector2 acceleration = new Vector2(h, v);
        acceleration = Vector3.Normalize(acceleration);

        WorldManager.instance.GetMyPlayer().SetDrift(drifting);
        WorldManager.instance.GetMyPlayer().SetMoveVector(acceleration);
        KeyMessage msg = new KeyMessage(id, acceleration);
        ServerManager.Instance().SendDataToInGame<KeyMessage>(msg);
    }
#endregion

#region PublicMethod
#endregion
    
}
