using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class FallpointManager : MonoBehaviour
{
#region PrivateVariables
    private List<Fallpoint> fallpoints;
    private int totalFallpoints;
#endregion

#region PrivateMethod

    private void Start()
    {
        totalFallpoints = transform.childCount;
        SetFallpoints();
        foreach (Fallpoint fallpoint in fallpoints)
            fallpoint.OnPlayerEnterFallpoint += OnPlayerEnterFallpoint;
    }
    private void SetFallpoints()
    {
        fallpoints = new List<Fallpoint>();
        for (int i = 0; i < totalFallpoints; i++)
            fallpoints.Add(transform.GetChild(i).GetComponent<Fallpoint>());
    }

    // 플레이어가 낙사 지점에 진입할 때 호출되는 콜백
    private void OnPlayerEnterFallpoint(Player _player, Fallpoint _fallpoint)
    {
        _player.Respawn();
    }
#endregion
}
