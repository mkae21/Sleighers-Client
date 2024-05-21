using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Finish : MonoBehaviour
{
#region PrivateVariables
    private List<Checkpoint> checkpoints;
#endregion

#region PublicVariables
    public GameObject checkpointHolder;
    public UnityAction<Player> OnPlayerEnter;
#endregion

#region PrivateMethod
    private void Start()
    {
        SetCheckpoints();
    }
    // 모든 체크포인트를 가져와서 리스트에 저장 및 초기화
    private void SetCheckpoints()
    {
        int totalCheckpoints = checkpointHolder.transform.childCount;
        checkpoints = new List<Checkpoint>();
        for (int i = 0; i < totalCheckpoints; i++)
        {
            checkpoints.Add(checkpointHolder.transform.GetChild(i).GetComponent<Checkpoint>()); 
            checkpoints[i].checkpointIndex = i;
            checkpoints[i].OnPlayerEnterCheckpoint += OnPlayerEnterCheckpoint;
        }
    }
    // 플레이어가 체크포인트에 진입할 때 호출되는 콜백
    private void OnPlayerEnterCheckpoint(Player _player, Checkpoint _checkpoint)
    {
        Checkpoint nextCheckpoint = GetNextCheckpoint(_checkpoint);

        _player.nextCheckpoint = nextCheckpoint.transform;
        _player.curCheckpoint = _checkpoint.transform;

        int checkpointIndex = _checkpoint.checkpointIndex;

        RankManager.instance.SetPlayerCheckpointCount(_player, checkpointIndex);

        // 마지막 체크포인트는 랭킹을 업데이트하지 않음
        if (checkpointIndex == checkpoints.Count - 1)
            return;

        List<RankInfo> ranking = RankManager.instance.GetRanking();
        InGameUI.instance.UpdateRankUI(ranking);
    }

    // 주어진 체크포인트의 다음 체크포인트를 반환
    private Checkpoint GetNextCheckpoint(Checkpoint _checkpoint)
    {
        int index = _checkpoint.checkpointIndex;

        return checkpoints[(index + 1) % checkpoints.Count];
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
        OnPlayerEnter?.Invoke(_player);
    }
#endregion
}