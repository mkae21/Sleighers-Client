using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
#region PublicVariables
    public static InGameUI instance;

    public TextMeshProUGUI text_timer;
    public TextMeshProUGUI text_countDown;
    public TextMeshProUGUI text_gameEndCountDown;
    public TextMeshProUGUI text_speedLabel;
    
    [Space(10), Header("랭킹 관련"), Tooltip("1/2 LAP")] 
    public TextMeshProUGUI text_lab;
    [Tooltip("1/5 (등수)")]
    public TextMeshProUGUI text_rank;
    [Tooltip("랭킹 프리팹을 가지고 있는 부모")]
    public Transform rankHolder;
    public RankManager rankManager;
    [Tooltip("랭킹 요소 프리팹")]
    public GameObject rankingElem;
#endregion

#region PrivateVariables
    private float countDownDuration = 3.0f;
    private float speed = 0.0f;
    private float timer = 0.0f;
    private Dictionary<int, TextMeshProUGUI> text_ranks;

    // Blink 코루틴 변수
    private float blinkDuration = 0.1f; // 블링크 지속 시간 (초)
    private Color originalColor = Color.white; // 원래 색상
    private Color blinkColor = Color.black; // 블링크 색상
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
        if (rankManager != null)
            rankManager.OnLapComplete += OnLapComplete;
        else
            Debug.Log("[InGameUI] LapManager가 없습니다.");
        UpdateLapText(1);
        text_ranks = new Dictionary<int, TextMeshProUGUI>();
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
        int lapsCompleted = rankManager.GetLapInfo(_player).lap;
        int currentLap = Mathf.Min(lapsCompleted + 1, rankManager.Laps);
        UpdateLapText(currentLap);
    }

    private void UpdateLapText(int _currentLap)
    {
        if (rankManager != null)
            text_lab.text = $"<size=160>{_currentLap}</size=160>/{rankManager.Laps} LAP";
    }
    // 랭킹 UI 깜박임 효과
    private IEnumerator BlinkCoroutine(Image _target)
    {
        // 점차 어두워짐
        for (float t = 0; t <= blinkDuration; t += Time.deltaTime)
        {
            Color color = Color.Lerp(originalColor, blinkColor, t / blinkDuration);
            color.a = 80/255f;
            _target.color = color;
            yield return null;
        }
        // 점차 밝아짐
        for (float t = 0; t <= blinkDuration; t += Time.deltaTime)
        {
            Color color = Color.Lerp(blinkColor, originalColor, t / blinkDuration);
            color.a = 80/255f;
            _target.color = color;
            yield return null;
        }
    }

#endregion

#region PublicMethod
    public void AddRankUI(int _id)
    {
        GameObject rankElem = Instantiate(rankingElem, rankHolder);
        TextMeshProUGUI text_rankElem = rankElem.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        text_rankElem.text = $"Player{_id} 0LAP 0CP";
        text_ranks.Add(_id, text_rankElem);
    }
    public void DeleteRankUI(int _id)
    {
        Destroy(text_ranks[_id].transform.parent.gameObject);
        text_ranks.Remove(_id);
    }
    public void UpdateRankUI(List<RankSort> _ranking)
    {
        for (int i = 0; i < _ranking.Count; i++)
        {
            int id = _ranking[i].id;
            int lap = _ranking[i].lap;
            int checkpoint = _ranking[i].checkpoint;

            if (id == WorldManager.instance.MyPlayerId)
            {
                text_rank.text = $"<size=160>{i + 1}</size>/{_ranking.Count}";
                StartCoroutine(BlinkCoroutine(text_ranks[id].transform.parent.GetComponent<Image>()));
            }
            text_ranks[id].text = $"{i + 1}. Player{id} {lap}LAP {checkpoint}CP";
            text_ranks[id].transform.parent.SetSiblingIndex(i);
        }
    }
    public void UpdateTimer()
    {
        if (WorldManager.instance.IsRaceFinish)
            return;

        timer += Time.deltaTime;
        int minutes = (int)(timer / 60 % 60);
        int seconds = (int)(timer % 60);
        int miliseconds = (int)(timer * 1000 % 1000);
        text_timer.text = string.Format("TIME  {0:D2} : {1:D2} : {2:D3}", minutes, seconds, miliseconds);
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
        if(!WorldManager.instance.IsRaceFinish)
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
        else
        {
            text_gameEndCountDown.gameObject.SetActive(true);
            text_gameEndCountDown.text = "완주!";
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
