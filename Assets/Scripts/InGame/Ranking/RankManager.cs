using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public struct RankInfo
{
    public string nickname; // 플레이어 닉네임
    public int lap;         // 완료된 랩 수
    public int checkpoint;  // 완료된 체크포인트 수
    public float distanceToNextCheckpoint; // 다음 체크포인트까지의 거리
}

public class RankManager : MonoBehaviour
{
#region PrivateVariables
    private Finish finish;

    // 모든 플레이어의의 랭킹에 관련된 정보를 포함하는 딕셔너리
    private Dictionary<string, RankInfo> rankInfoDictionary;
#endregion

#region PublicVariables
    public int previousRank = 1;    // 갱신된 랭킹에 비해 나의 이전 랭킹
    // 차량이 한 바퀴를 완료하면 액션이 호출
    public UnityAction<Player, RankInfo> OnLapComplete { get; set; }
    public static RankManager instance;
    public int laps { get; set; } = 1;
#endregion

#region PrivateMethod
    private void Awake()
    {
        instance = this;
        finish = FindObjectOfType<Finish>();
        rankInfoDictionary = new Dictionary<string, RankInfo>();

        if (finish != null)
            finish.OnPlayerEnter += OnPlayerEnterFinish;
        else
            Debug.LogWarning("[RankManager] 씬에 결승선이 없습니다.");
    }
    
    private void Update()
    {
        UpdatePlayersDistanceAtSameCheckpoint();
    }

    // 차량이 결승선에 진입할 때(한 바퀴 완료 후) 호출되는 콜백
    private void OnPlayerEnterFinish(Player _player)
    {   
        if (_player == null)
            return;
        string nickname = _player.nickname;
        // 딕셔너리 항목이 없는 경우 새 사전 항목 만들기
        if (!rankInfoDictionary.ContainsKey(nickname))
            rankInfoDictionary.Add(nickname, new RankInfo());

        // 완료한 랩 수 증가 및 업데이트
        RankInfo rankInfo = rankInfoDictionary[nickname];
        rankInfo.lap = Mathf.Clamp(rankInfo.lap + 1, 0, laps);
        rankInfoDictionary[nickname] = rankInfo;

        OnLapComplete?.Invoke(_player, rankInfo);
        List<RankInfo> ranking = GetRanking();
        InGameUI.instance.UpdateRankUI(ranking);
    }
    private void UpdatePlayersDistanceAtSameCheckpoint()
    {
        // 체크포인트별로 플레이어들을 그룹화
        var playersAtSameCheckpoint = new Dictionary<int, List<RankInfo>>();

        foreach(var info in rankInfoDictionary.Values)
        {   
            // 현재 체크포인트에 해당하는 플레이어들을 그룹화
            if (!playersAtSameCheckpoint.ContainsKey(info.checkpoint))
                playersAtSameCheckpoint[info.checkpoint] = new List<RankInfo>();

            // 그룹에 플레이어 추가
            playersAtSameCheckpoint[info.checkpoint].Add(info);
        }

        bool rankingUpdated = false;

        // 각 그룹을 순회하며 그룹 내 플레이어 수가 2명 이상인 경우에만 거리 업데이트
        foreach(var group in playersAtSameCheckpoint.Values)
        {
            if (group.Count < 2)
                continue;

            foreach(var player in group)
            {
                Player playerNickName = WorldManager.instance.GetPlayerFromNickName(player.nickname);
                if (playerNickName == null)
                    return;
                SetDistanceToNextCheckpoint(playerNickName);
                rankingUpdated = true;
            }
        }
        
        if(rankingUpdated)
        {
            List<RankInfo> ranking = GetRanking();
            InGameUI.instance.UpdateRankUI(ranking);
        }
    }

#endregion

#region PublicMethod
    // 플레이어의 랭킹 정보를 가져오거나 추가
    public RankInfo AddOrGetRankInfo(Player _player)
    {
        string nickname = _player.nickname;
        if (!rankInfoDictionary.ContainsKey(nickname))
        {
            RankInfo newRankInfo = new RankInfo()
            {
                nickname = _player.nickname,
                lap = 0,
                checkpoint = 0
            };
            rankInfoDictionary.Add(nickname, newRankInfo);
            InGameUI.instance.CreateRankUI(_player.nickname);
            return newRankInfo;
        }
        return rankInfoDictionary[nickname];
    }
    public void DeleteRankInfo(Player _player)
    {
        string nickname = _player.nickname;
        if (rankInfoDictionary.ContainsKey(nickname))
        {
            rankInfoDictionary.Remove(nickname);
            InGameUI.instance.DeleteRankUI(nickname);
        }
    }
    public List<RankInfo> GetRanking()
    {
        // 업데이트된 정보를 기반으로 플레이어들을 정렬하고 랭킹 매김
        List<RankInfo> sortedRanking = rankInfoDictionary.Values
            .OrderByDescending(info => info.lap)
            .ThenByDescending(info => info.checkpoint)
            .ThenBy(info => info.distanceToNextCheckpoint)
            .ToList();

        for (int i = 0; i < sortedRanking.Count; i++)
        {
            if (sortedRanking[i].nickname == WorldManager.instance.GetMyPlayer().nickname)
            {
                WorldManager.instance.GetMyPlayer().myRank = i + 1;
                break;
            }
        }
        return sortedRanking;
    }

    public void SetPlayerCheckpointCount(Player _player)
    {
        string nickname = _player.nickname;
        if (rankInfoDictionary.ContainsKey(nickname))
        {
            RankInfo lapInfo = rankInfoDictionary[nickname];
            lapInfo.checkpoint++;
            rankInfoDictionary[nickname] = lapInfo;
        }
    }

    public void SetDistanceToNextCheckpoint(Player _player)
    {
        string nickname = _player.nickname;
        if (rankInfoDictionary.ContainsKey(nickname))
        {
            RankInfo rankInfo = rankInfoDictionary[_player.nickname];
            rankInfo.distanceToNextCheckpoint = _player.UpdateDistanceToNextCheckpoint();
            rankInfoDictionary[_player.nickname] = rankInfo;
        }
    }
#endregion
}
