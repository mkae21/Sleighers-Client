using UnityEngine;

/* [ FollowMyPlayer.cs ]
 * 내 플레이어를 따라가는 스크립트이다.
 */
public class FollowMyPlayer : MonoBehaviour
{
#region PrivateVariables
    private Transform sphere = null;
    private float smoothTime = 0.3f;
    private Vector3 currentVelocity;
#endregion

#region PrivateMethod
    private void Start()
    {
        TryGetMyPlayer();
    }
    private void Update()
    {
        if (sphere != null)
            transform.position = Vector3.SmoothDamp(transform.position, sphere.position, ref currentVelocity, smoothTime);
    }
    private void TryGetMyPlayer()
    {
        int myPlayerID = WorldManager.instance.MyPlayerId;
        if (myPlayerID == -1)
            Invoke("TryGetMyPlayer", 1f);
        else
            sphere = WorldManager.instance.GetMySphere().transform;
    }
#endregion
}