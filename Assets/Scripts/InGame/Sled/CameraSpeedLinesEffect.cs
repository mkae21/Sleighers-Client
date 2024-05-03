using UnityEngine;
/* CameraSpeedLinesEffect.cs
 * 차량이 특정 속도에 도달하면 스피드 라인 효과를 만들 수 있습니다.
 * 1인칭 또는 탑뷰를 사용하는 경우 이펙트가 잘 보이지 않는다는 점에 유의하세요.
 */
public class CameraSpeedLinesEffect : MonoBehaviour
{
#region PrivateVariables
    [Tooltip("The min rate of it's max velociyt the vehicle needs to reach, in order to produce speed lines")]
    [SerializeField, Range(0, 1)] private float startVelocity = 0.5f;

    
    [Tooltip("The max emission rate of the particle system")]
    [SerializeField] private float maxEmissionRate = 50;

    
    [Tooltip("The particle system creating the speed lines")]
    [SerializeField] private new ParticleSystem particleSystem = null;

    private Camera mainCamera;
    private Player player;
#endregion

#region PrivateMethod
    private void Awake()
    {
        mainCamera = Camera.main;
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        UpdateEmission();
        UpdateRotation();
    }

    // 차량의 속도에 따라 파티클 시스템의 방출 속도를 조정합니다.
    private void UpdateEmission()
    {
        // 차량이 이동 중이고 필요한 속도보다 빠른 경우에만 속도선 파티클을 표시
        // if (player.NormalizedForwardSpeed >= startVelocity && player.isMove)
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

    // 속도선이 카메라의 시야를 가리키도록 회전을 조정
    private void UpdateRotation()
    {
        var targetPos = mainCamera.transform.position;
        targetPos.y = transform.position.y;

        transform.LookAt(targetPos);
        transform.forward = -transform.forward;
    }
#endregion
}