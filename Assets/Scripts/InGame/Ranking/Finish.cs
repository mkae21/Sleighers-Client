using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
public class Finish : MonoBehaviour
{
#region PrivateVariables
    private List<Checkpoint> checkpoints;
    private int totalCheckpoints;
#endregion

#region PublicVariables
    public GameObject checkpointHolder;
    public UnityAction<Player> OnPlayerEnter;
#endregion

#region PrivateMethod
    private void Start()
    {
        totalCheckpoints = checkpointHolder.transform.childCount;
        SetCheckpoints();
        foreach (Checkpoint checkpoint in checkpoints)
            checkpoint.OnPlayerEnterCheckpoint += OnPlayerEnterCheckpoint;
    }
    private void SetCheckpoints()
    {
        checkpoints = new List<Checkpoint>();
        for (int i = 0; i < totalCheckpoints; i++)
            checkpoints.Add(checkpointHolder.transform.GetChild(i).GetComponent<Checkpoint>());
    }
    // 플레이어가 체크포인트에 진입할 때 호출되는 콜백
    private void OnPlayerEnterCheckpoint(Player _player, Checkpoint _checkpoint)
    {
        CheckpointInfo checkpointInfo = GetOrAddCheckpointInfo(_player);

        // 플레이어가 아직 체크포인트를 하나도 통과하지 못하였다면
        if (checkpointInfo.lastCheckpoint == null)
        {
            // 첫번째 체크포인트만 통과해야 함.
            if (_checkpoint == checkpoints.First())
            {
                _player.respawnPosition = _checkpoint.transform.position;
                checkpointInfo.SetLastCheckpoint(_checkpoint);
                checkpointInfo.SetNextCheckpoint(GetNextCheckpoint(_checkpoint));
                RankManager.instance.SetPlayerCheckpointCount(_player);
                List<RankInfo> ranking = RankManager.instance.GetRanking();
                InGameUI.instance.UpdateRankUI(ranking);
            }
        }
        // 그렇지 않으면 통과한 체크포인트가 통과해야 하는 다음 체크포인트인 경우, 그 다음 체크포인트를 잠금 해제합니다.
        else if (checkpointInfo.nextCheckpoint == _checkpoint)
        {
            // 마지막 체크포인트를 통과한 경우 결승선을 활성화
            if (_checkpoint == checkpoints.Last())
                checkpointInfo.canPassFinish = true;

            _player.respawnPosition = _checkpoint.transform.position;
            checkpointInfo.SetLastCheckpoint(_checkpoint);
            checkpointInfo.SetNextCheckpoint(GetNextCheckpoint(_checkpoint));
            RankManager.instance.SetPlayerCheckpointCount(_player);
            List<RankInfo> ranking = RankManager.instance.GetRanking();
            InGameUI.instance.UpdateRankUI(ranking);
        } 
    }
    // 주어진 체크포인트의 다음 체크포인트를 반환
    private Checkpoint GetNextCheckpoint(Checkpoint _checkpoint)
    {
        int index = checkpoints.IndexOf(_checkpoint);

        return checkpoints[(index + 1) % checkpoints.Count];
    }

    // 지정된 차량의 체크포인트 정보를 가져오거나 없는 경우 추가합니다.
    private CheckpointInfo GetOrAddCheckpointInfo(Player _player)
    {
        CheckpointInfo result = _player.GetComponent<CheckpointInfo>();

        if(result == null)
            result = _player.gameObject.AddComponent<CheckpointInfo>();

        return result;
    }

    private void OnTriggerEnter(Collider _other)
    {
        var player = _other.GetComponentInParent<Player>();
        if(_other.attachedRigidbody != null && player != null)
            OnPlayerEnterFinish(player);
    }

    // 플레이어가 결승선에 진입할 때 호출되는 콜백
    private void OnPlayerEnterFinish(Player _player)
    {
        CheckpointInfo checkpointInfo = GetOrAddCheckpointInfo(_player);

        // 결승선을 통과하면 마지막으로 통과한 체크포인트를 초기화
        // 이렇게 하면 플레이어가 뒤로 운전하여 마지막 체크포인트를 통과한 후 다시 결승선을 통과하는 등의 부정행위를 방지할 수 있다
        checkpointInfo.SetLastCheckpoint(null);

        if (checkpointInfo.canPassFinish)
        {
            checkpointInfo.canPassFinish = false;
            OnPlayerEnter?.Invoke(_player);
        }
    }
#endregion
}