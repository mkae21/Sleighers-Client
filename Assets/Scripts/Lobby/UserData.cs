using UnityEngine;

public class UserData : MonoBehaviour
{
#region PublicVariables
    public static UserData instance = null;
    public string email;
    public string nickName;
#endregion

#region PrivateMethod
    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
    }
#endregion
}
