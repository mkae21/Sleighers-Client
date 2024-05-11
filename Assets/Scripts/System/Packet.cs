using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Packet
{
    public string email;
}

public class UserInfo : Packet
{
    public string name;
    public string cart;
    public int curCostume;
    public int curCart;
}