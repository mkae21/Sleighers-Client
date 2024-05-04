using UnityEngine;
/* SnowEffect.cs
 * 플레이어가 특정 속도에 도달하면 눈 효과를 생성하는 스크립트
 */
public class SnowEffect : MonoBehaviour
{
#region PrivateVariables
    [Tooltip("눈 효과를 생성하기 위해 플레이어가 도달해야 하는 최대 속도의 최소 속도")]
    [SerializeField, Range(0, 1)] private float startVelocity = 0.2f;

    
    [Tooltip("파티클 시스템의 최대 방출 속도")]
    [SerializeField] private float maxEmissionRate = 100;

    
    [Tooltip("눈 효과를 만드는 파티클 시스템")]
    [SerializeField] private new ParticleSystem particleSystem = null;
    private Player player;
#endregion

#region PrivateMethod
    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        UpdateEmission();
    }

    // 차량의 속도에 따라 파티클 시스템의 방출 속도를 조정합니다.
    private void UpdateEmission()
    {
        // 차량이 이동 중이고 필요한 속도보다 빠른 경우에만 속도선 파티클을 표시
        if (player.NormalizedForwardSpeed >= startVelocity)
        {
            var emission = particleSystem.emission;

            // 파티클 시스템의 방출을 보간
            var t = startVelocity > 0 ? player.NormalizedForwardSpeed / startVelocity : 1.0f;

            emission.rateOverTime = Mathf.Lerp(0, maxEmissionRate, t);
        }
        // 그렇지 않으면 방출을 중지
        else
        {
            var emission = particleSystem.emission;
            emission.rateOverTime = 0;
        }
    }
#endregion
}