using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
#region PublicVariables
    public static InGameUI instance;

    public TextMeshProUGUI text_timer;
    public TextMeshProUGUI text_countDown;
    public TextMeshProUGUI text_gameEndCountDown;
    public TextMeshProUGUI text_lab;
    public TextMeshProUGUI text_speedLabel;
    public Transform rankHolder; // 랭킹 프리팹을 가지고 있는 부모
    public RankManager lapManager;

#endregion

#region PrivateVariables
    private float countDownDuration = 3.0f;
    private float speed = 0.0f;
    private float timer = 0.0f;
    private TextMeshProUGUI[] text_ranks;
#endregion

#region PrivateMethod
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        GameManager.InGame += UpdateTimer;
        GameManager.InGame += UpdateSpeedometer;
        if (lapManager != null)
            lapManager.OnLapComplete += OnLapComplete;
        else
            Debug.Log("[InGameUI] LapManager가 없습니다.");
        UpdateLapText(1);
        InitRankUI();
    }

    private void InitRankUI()
    {
        int totalRankCount = rankHolder.childCount;
        text_ranks = new TextMeshProUGUI[totalRankCount];
        for (int i = 0; i < totalRankCount; i++)
        {
            text_ranks[i] = rankHolder.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>();
            text_ranks[i].text = string.Empty;
        }
    }

    // Go! 텍스트 숨기기
    private void HideCountDown()
    {
        if(text_countDown.text == "GO!")
            text_countDown.gameObject.SetActive(false);
        if(text_gameEndCountDown.text == "Game End")
            text_gameEndCountDown.gameObject.SetActive(false);
    }
    // 플레이어가 랩을 통과하면 호출
    private void OnLapComplete(Player _player, RankInfo _lapInfo)
    {
        // 다른 플레이어가 랩을 완료하면 반환
        if (WorldManager.instance.GetMyPlayer() != _player)
            return;      
        // Lap 텍스트 업데이트
        else
        { 
            int lapsCompleted = lapManager.GetLapInfo(_player).lap;

            int currentLap = Mathf.Min(lapsCompleted + 1, lapManager.Laps);

            UpdateLapText(currentLap);
        }
    }

    private void UpdateLapText(int _currentLap)
    {

        if (lapManager != null)
            text_lab.text = $"<size=180>{_currentLap}</size=180>/{lapManager.Laps} LAP";
    }

#endregion

#region PublicMethod
    public void UpdateRankUI(List<string> _ranking)
    {
        for (int i = 0; i < _ranking.Count; i++)
            text_ranks[i].text = _ranking[i];
    }
    public void UpdateTimer()
    {
        if (WorldManager.instance.IsRaceFinish)
            return;

        timer += Time.deltaTime;
        int minutes = (int)(timer / 60 % 60);
        int seconds = (int)(timer % 60);
        int miliseconds = (int)(timer * 1000 % 1000);
        text_timer.text = string.Format("{0:D2} : {1:D2} : {2:D3}", minutes, seconds, miliseconds);
    }

    // 카운트 다운 설정
    public void SetCountDown(int _count)
    {
        GameManager.Instance().soundManager.Play("Effect/StartCount", SoundType.EFFECT);
        countDownDuration = _count;

        if(text_countDown != null)
        {
            if(countDownDuration > 0)
            {
                text_countDown.text = countDownDuration.ToString();
                text_countDown.gameObject.SetActive(true);
            }
            else
            {
                text_countDown.text = "GO!";
                Invoke("HideCountDown", 0.4f);
            }
        }
        else
        {
            text_countDown.gameObject.SetActive(false);         
        }
    }

    public void SetGameEndCountDown(int _count)
    {
        GameManager.Instance().soundManager.Play("Effect/EndCount", SoundType.EFFECT);

        if(text_gameEndCountDown != null)
        {
            if(_count > 0)
            {
                text_gameEndCountDown.text = _count.ToString();
                text_gameEndCountDown.gameObject.SetActive(true);
            }
            else
            {
                text_gameEndCountDown.text = "Game End";
                Invoke("HideCountDown", 1f);
            }
        }
    }
    public void UpdateSpeedometer()
    {
        speed = WorldManager.instance.GetMyPlayer().GetSpeed();
        if(text_speedLabel != null)
            text_speedLabel.text = string.Format("{0}", (int)speed);
    }

#endregion
}
