using UnityEngine;
using Reader;

public class InputManager : MonoBehaviour    
{
#region PrivateVariables
#endregion

#region PublicVariables
    static public InputManager instance;
    public int playerIndex;
#endregion

#region PrivateMethod
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        GameManager.InGame += PlayerInput;
    }

    private void PlayerInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveVector = new Vector3(h, 0, v);
        moveVector = Vector3.Normalize(moveVector);

        byte[] data = new byte[1024];
        ByteWriter bw = new ByteWriter(data);
        bw.WriteInt((int)Protocol.Type.c_PlayerPosition);
        bw.WriteInt(playerIndex);
        bw.WriteVector3(moveVector);
        
        WorldManager.instance.GetMyPlayer().SetMoveVector(moveVector);
        ServerManager.Instance().SendMessage(data);
    }
#endregion

#region PublicMethod
#endregion
    
}
