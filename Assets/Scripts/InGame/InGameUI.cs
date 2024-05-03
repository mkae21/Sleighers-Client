using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
#region PublicVariables
    public static InGameUI instance;
    public TextMeshProUGUI text_Lab;
    public TextMeshProUGUI text_Timer;
    public TextMeshProUGUI text_CountDown;
    public TextMeshProUGUI text_speedLabel;
    public RectTransform arrow;
    public LapManager lapManager;

    public float countDownDuration = 3.0f;
    public float maxSpeed = 0.0f;
    public float speed = 0.0f;
    public float minSpeedArrowAngle;
    public float maxSpeedArrowAngle;
#endregion

#region PrivateVariables
    private float timer = 0.0f;
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
    }

    // Go! 텍스트 숨기기
    private void HideCountDown()
    {
        if(text_CountDown != null)
            text_CountDown.gameObject.SetActive(false);
    }
    private void OnLapComplete(Player _player, LapInfo _lapInfo)
    {
        // 다른 플레이어가 랩을 완료하면 반환
        if (WorldManager.instance.GetMyPlayer() != _player)
            return;      
        // Lap 텍스트 업데이트
        else
        { 
            int lapsCompleted = lapManager.GetLapInfo(_player).completed;

            int currentLap = Mathf.Min(lapsCompleted + 1, lapManager.Laps);
            Debug.LogFormat("Player {0}가 {1}번째 랩 완주", _player.playerId, currentLap);

            UpdateLapText(currentLap);
        }
    }

    private void UpdateLapText(int currentLap)
    {
        if (lapManager != null)
            text_Lab.text = $"LAP {currentLap} / {lapManager.Laps}";
    }

#endregion

#region PublicMethod
    public void UpdateTimer()
    {
        timer += Time.deltaTime;
        int hours = (int)(timer / 3600);
        int minutes = (int)(timer / 60 % 60);
        int seconds = (int)(timer % 60);
        text_Timer.text = string.Format("Time: {0:D2} : {1:D2} : {2:D2}", hours, minutes, seconds);
    }

    // 카운트 다운 설정
    public void SetCountDown(int count)
    {
        countDownDuration = count;

        if(text_CountDown != null)
        {
            if(countDownDuration > 0)
            {
                text_CountDown.text = countDownDuration.ToString();
                text_CountDown.gameObject.SetActive(true);
            }
            else
            {
                text_CountDown.text = "GO!";
                Invoke("HideCountDown", 0.4f);
            }
        }
        else
        {
            text_CountDown.gameObject.SetActive(false);         
        }
    }

    public void UpdateSpeedometer()
    {
        speed = WorldManager.instance.GetMyPlayer().GetSpeed();
        if(text_speedLabel != null)
            text_speedLabel.text = string.Format("{0} km/h", (int)speed);
        
        if(arrow != null)
            arrow.localEulerAngles = new Vector3(0,0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, speed / maxSpeed));
    }

#endregion
}
