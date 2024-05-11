using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
/* SpeedPostProcessingEffect.cs
 * 차량이 가속할 때 주어진 포스트 프로세싱 볼륨을 부드럽게 블렌딩하여 멋진 시각적 속도 효과를 연출
 */
public class SpeedPostProcessingEffect : MonoBehaviour
{

#region PrivateVariables
    [SerializeField] private PostProcessVolume postProcessVolume = null;
    private Player player;
#endregion

#region PrivateMethod
    private void Start()
    {
        player = GetComponentInParent<Player>();
        if (!player.isMe)
            gameObject.SetActive(false);
    }

    private void Update()
    {
        if (postProcessVolume != null && player != null && player.isMe)
        {
            // float speed = player.isMove ? player.NormalizedForwardSpeed : 0.0f;
            float speed = player.NormalizedForwardSpeed;

            // 차량의 정규화된 속도에 따라 볼륨의 무게를 조정
            postProcessVolume.weight = speed;
        }
    }
#endregion
}