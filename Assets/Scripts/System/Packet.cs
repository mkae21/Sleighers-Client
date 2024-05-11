using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class packet
{
    public string id;
}

public class userInfo:packet
{
    public string name;
    public string cart;
    public int curCostume;
    public int curCart;
}