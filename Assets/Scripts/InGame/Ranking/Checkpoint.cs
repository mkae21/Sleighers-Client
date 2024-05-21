using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
#region PrivateVariables
    private new Collider collider;
#endregion

#region PublicVariables
    public UnityAction<Player, Checkpoint> OnPlayerEnterCheckpoint; // 플레이어가 체크포인트에 진입할 때 호출되는 콜백
    public int checkpointIndex = 0;           // 체크포인트 인덱스
#endregion

#region PrivateMethod
    private void Awake()
    {
        collider = GetComponent<Collider>();
        collider.isTrigger = true; // 콜라이더가 트리거인지 확인
    }

    private void OnTriggerEnter(Collider _other)
    {
        Player player = _other.GetComponentInParent<Player>();

        if (player != null)
            OnPlayerEnterCheckpoint?.Invoke(player, this);
    }
#endregion
}