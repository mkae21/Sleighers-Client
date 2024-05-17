using UnityEngine;

/* [ FollowMyPlayer.cs ]
 * 내리는 눈 파티클 시스템이 내 플레이어를 따라가는 스크립트이다.
 */
public class BackGroundSnowParticle : MonoBehaviour
{
#region PrivateVariables
    private Transform sled = null;
    private new ParticleSystem particleSystem;
    private Player player;
    private float currentYVelocity = 0;
    private float currentZVelocity = 0;
#endregion

#region PrivateMethod
    private void Awake()
    {
        particleSystem = gameObject.GetComponent<ParticleSystem>();
    }
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
            transform.rotation = Quaternion.LookRotation(-sled.forward);
        }
        if(particleSystem != null && player != null)
        {
            var velocityOverLifetime = particleSystem.velocityOverLifetime;

            float targetYVelocity = player.NormalizedForwardSpeed * -70f;
            float targetZVelocity = player.NormalizedForwardSpeed * 230f;

            // Use Lerp to gradually change the velocity values
            if(player.GetSpeed() > 60)
            {
                currentYVelocity = Mathf.Lerp(currentYVelocity, targetYVelocity, Time.deltaTime);
                currentZVelocity = Mathf.Lerp(currentZVelocity, targetZVelocity, Time.deltaTime);
            }
            else
            {
                currentYVelocity = Mathf.Lerp(currentYVelocity, 0, Time.deltaTime);
                currentZVelocity = Mathf.Lerp(currentZVelocity, 0, Time.deltaTime);
            }

            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(currentYVelocity); // Y-axis velocity
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(currentZVelocity); // Z-axis velocity
        }

    }
    private void TryGetMyPlayer()
    {
        string myPlayerID = WorldManager.instance.myPlayerNickname;
        if (myPlayerID == string.Empty)
            Invoke("TryGetMyPlayer", 1f);
        else
        {
            sled = WorldManager.instance.GetMySled().transform;
            player = WorldManager.instance.GetMyPlayer();
        }
    }
#endregion
}