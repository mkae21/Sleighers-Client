using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{
    public static User instance = null;
    public string id;
    public string email;
    public string nickName;
    public string cart;
    public int curCostume;
    public int curCart;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
    }
}
