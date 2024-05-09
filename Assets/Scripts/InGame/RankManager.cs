using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public struct RankInfo
{
    public int lap;         // 완료된 랩 수
    public int checkpoint;  // 완료된 체크포인트 수
}
public struct RankSort // 랭킹 정렬을 위한 임시 구조체. 추후 코드 리팩토링 필요
{
    public int id;          // 플레이어 ID
    public int lap;         // 완료된 랩 수
    public int checkpoint;  // 완료된 체크포인트 수
}
public class RankManager : MonoBehaviour
{
#region PrivateVariables
    [Tooltip("레이스를 완료하기 위해 통과해야 하는 랩 수")]
    [SerializeField] private int laps;
    public int Laps
    { 
        get { return this.laps; }
        set { this.laps = value; }
    }
    private Finish finish;

    // 모든 차량의 랭킹에 관련된 정보를 포함하는 딕셔너리
    private Dictionary<Player, RankInfo> rankInfoDictionary = new Dictionary<Player, RankInfo>();
#endregion

#region PublicVariables
    // 차량이 한 바퀴를 완료하면 액션이 호출
    public UnityAction<Player, RankInfo> OnLapComplete { get; set; }
    public static RankManager instance;

#endregion

#region PrivateMethod
    private void Awake()
    {
        instance = this;
        finish = FindObjectOfType<Finish>();

        if (finish != null)
            finish.OnPlayerEnter += OnPlayerEnterFinish;
        else
            Debug.LogWarning("[RankManager] 씬에 결승선이 없습니다.");
    }
    // 차량이 결승선에 진입할 때(한 바퀴 완료 후) 호출되는 콜백
    private void OnPlayerEnterFinish(Player _player)
    {   
        if (_player == null)
            return;
        // 딕셔너리 항목이 없는 경우 새 사전 항목 만들기
        if (!rankInfoDictionary.ContainsKey(_player))
            rankInfoDictionary.Add(_player, new RankInfo());

        // 완료한 랩 수 증가 및 업데이트
        RankInfo rankInfo = rankInfoDictionary[_player];

        rankInfo.lap = Mathf.Clamp(rankInfo.lap + 1, 0, laps);

        rankInfoDictionary[_player] = rankInfo;

        OnLapComplete?.Invoke(_player, rankInfo);
        InGameUI.instance.UpdateRankUI(GetRanking());
    }
    
#endregion

#region PublicMethod
    public void AddRankInfo(Player _player)
    {
        if (!rankInfoDictionary.ContainsKey(_player))
        {
            RankInfo lapInfo = new RankInfo()
            {
                lap = 0,
                checkpoint = 0
            };
            rankInfoDictionary.Add(_player, lapInfo);
            InGameUI.instance.AddRankUI(_player.playerId);
        }
    }
    public void DeleteRankInfo(Player _player)
    {
        if (rankInfoDictionary.ContainsKey(_player))
        {
            rankInfoDictionary.Remove(_player);
            InGameUI.instance.DeleteRankUI(_player.playerId);
        }
    }
    // 주어진 차량의 랩 정보를 가져오기.
    public RankInfo GetLapInfo(Player _player)
    {
        RankInfo labInfo = new RankInfo()
        {
            lap = 0,
            checkpoint = 0
        };
        if (_player != null && rankInfoDictionary.ContainsKey(_player))
            labInfo = rankInfoDictionary[_player];

        return labInfo;
    }
    public List<RankSort> GetRanking()
    {   
        List<RankSort> ranking = new List<RankSort>();
        var sortedRanking = from pair in rankInfoDictionary 
                        orderby pair.Value.lap descending, pair.Value.checkpoint descending 
                        select pair;

        foreach (var item in sortedRanking)
        {
            RankSort rankSort = new RankSort()
            {
                id = item.Key.playerId,
                lap = item.Value.lap,
                checkpoint = item.Value.checkpoint
            };
            ranking.Add(rankSort);
        }

        return ranking;
    }

    public void SetPlayerCheckpointCount(Player _player)
    {
        if (rankInfoDictionary.ContainsKey(_player))
        {
            RankInfo lapInfo = rankInfoDictionary[_player];
            lapInfo.checkpoint++;
            rankInfoDictionary[_player] = lapInfo;
        }
    }
#endregion
}
