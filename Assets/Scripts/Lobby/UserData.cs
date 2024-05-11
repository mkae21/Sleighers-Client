using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData : MonoBehaviour
{
#region PublicVariables
    public static UserData instance = null;
    public string id;
    public string email;
    public string nickName;
    public string cart;
    public int curCostume;
    public int curCart;
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
