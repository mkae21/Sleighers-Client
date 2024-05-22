using UnityEngine;
/* InputManager.cs
 * - 인게임에서 플레이어 입력 처리
 * - 플레이어 스크립트는 여러 개 생성되기에 여기서 플레이어의 입력을 받아서 플레이어 이동 처리
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
        GameManager.InGame += KeyInput;
    }

    private void KeyInput()
    {
        if (Input.GetKey(KeyCode.T))
        {
            WorldManager.instance.GetMyPlayer().transform.Find("Sphere").position = new Vector3(338, -184, 230);
        }
        if (Input.GetKey(KeyCode.G))
            WorldManager.instance.OnSendInGame(Protocol.Type.PlayerGoal);
        if (Input.GetKey(KeyCode.Escape))
            WorldManager.instance.GetMyPlayer().Respawn();
        
        if (WorldManager.instance.isRaceFinish)
            return;
        
        float h = Input.GetAxis(HORIZONTAL);
        float v = Input.GetAxis(VERTICAL);
        bool drifting = Input.GetKey(KeyCode.LeftShift);
        
        Vector2 acceleration = new Vector2(h, v);
        acceleration = Vector3.Normalize(acceleration);

        WorldManager.instance.GetMyPlayer().SetDrift(drifting);
        WorldManager.instance.GetMyPlayer().SetMoveVector(acceleration);
    }
#endregion  
}