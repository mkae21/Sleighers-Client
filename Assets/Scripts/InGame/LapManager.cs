using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public struct LapInfo
{
    public int completed;   // 완료된 랩 수
}
public class LapManager : MonoBehaviour
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

    // 모든 차량의 랩 정보를 포함하는 딕셔너리
    private Dictionary<Player, LapInfo> lapInfoDictionary = new Dictionary<Player, LapInfo>();
#endregion

#region PublicVariables
    // 차량이 한 바퀴를 완료하면 액션이 호출
    public UnityAction<Player, LapInfo> OnLapComplete { get; set; }

#endregion

#region PrivateMethod
    private void Awake()
    {
        finish = FindObjectOfType<Finish>();

        if (finish != null)
            finish.OnPlayerEnter += OnPlayerEnter;
        else
            Debug.Log("[Lab Manager] 씬에 결승선이 없습니다.");
    }
    // 차량이 피니시 라인에 진입할 때(한 바퀴 완료 후) 호출되는 콜백
    private void OnPlayerEnter(Player _player)
    {   
        if (_player == null)
            return;
        // 딕셔너리 항목이 없는 경우 새 사전 항목 만들기
        if (!lapInfoDictionary.ContainsKey(_player))
            lapInfoDictionary.Add(_player, new LapInfo());

        // 완료한 랩 수 증가 및 업데이트
        var lapInfo = lapInfoDictionary[_player];

        lapInfo.completed = Mathf.Clamp(lapInfo.completed + 1, 0, laps);

        lapInfoDictionary[_player] = lapInfo;

        OnLapComplete?.Invoke(_player, lapInfo);
    }
#endregion

#region PublicMethod
    // 주어진 차량의 랩 정보를 가져오기.
    public LapInfo GetLapInfo(Player _player)
    {
        LapInfo labInfo = new LapInfo()
        {
            completed = 0
        };
        if (_player != null && lapInfoDictionary.ContainsKey(_player))
            labInfo = lapInfoDictionary[_player];

        return labInfo;
    }
#endregion
}
