using UnityEngine;

/* [ FollowMyPlayer.cs ]
 * 내리는 눈 파티클 시스템이 내 플레이어를 따라가는 스크립트이다.
 */
public class FollowMyPlayer : MonoBehaviour
{
#region PrivateVariables
    private Transform sled = null;
#endregion

#region PrivateMethod
    private void Start()
    {
        TryGetMyPlayer();
    }
    private void Update()
    {
        if (sled != null)
        {
            transform.position = new Vector3(sled.position.x, sled.position.y + 20, sled.position.z);
            transform.position += sled.forward * 30f;
        }
    }
    private void TryGetMyPlayer()
    {
        int myPlayerID = WorldManager.instance.myPlayerId;
        if (myPlayerID == -1)
            Invoke("TryGetMyPlayer", 1f);
        else
            sled = WorldManager.instance.GetMySled().transform;
    }
#endregion
}