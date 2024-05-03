using UnityEngine;

/* CheckpointInfo.cs
 * 이 컴포넌트는 차량에 부착되어 차량이 통과한 체크포인트에 대한 정보를 저장한다.
 * 이 정보를 통해 차량이 한 바퀴를 성공적으로 통과했는지 여부를 Finish 컴포넌트에서 확인할 수 있다.
 * 이를 통해 결승선을 통과한 후 뒤로 주행했다가 다시 통과하는 등의 부정 행위를 방지할 수 있다.
 */
public class CheckpointInfo : MonoBehaviour
{
#region PublicVariables
        // 차량이 현재 랩을 완료하기 위해 결승선을 통과할 수 있는지 아니면 먼저 체크포인트를 통과해야 하는지 여부
        public bool canPassFinish { get; set; }

        // 마지막 체크포인트 통과
        public Checkpoint lastCheckpoint { get; private set; } = null;

        // 통과해야 하는 다음 체크포인트
        public Checkpoint nextCheckpoint { get; private set; } = null;

        public void SetLastCheckpoint(Checkpoint _checkpoint)
        {
            lastCheckpoint = _checkpoint;
        }

        public void SetNextCheckpoint(Checkpoint _checkpoint)
        {
            nextCheckpoint = _checkpoint;
        }
#endregion
}
