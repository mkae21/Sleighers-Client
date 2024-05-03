using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
#region PublicVariables
    public static InGameUI instance;

    public TextMeshProUGUI text_timer;
    public TextMeshProUGUI text_countDown;
    public TextMeshProUGUI text_speedLabel;
    public RectTransform arrow;

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
    }

    // Go! 텍스트 숨기기
    private void HideCountDown()
    {
        if(text_countDown != null)
            text_countDown.gameObject.SetActive(false);
    }

#endregion

#region PublicMethod
    public void UpdateTimer()
    {
        timer += Time.deltaTime;
        int minutes = (int)(timer / 60 % 60);
        int seconds = (int)(timer % 60);
        int miliseconds = (int)(timer * 1000 % 1000);
        text_timer.text = string.Format("Time : {0:D2} : {1:D2} : {2:D3}", minutes, seconds, miliseconds);
    }

    // 카운트 다운 설정
    public void SetCountDown(int count)
    {
        countDownDuration = count;

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

    public void UpdateSpeedometer()
    {
        speed = WorldManager.instance.GetMyPlayer().GetSpeed();
        if(text_speedLabel != null)
            text_speedLabel.text = string.Format("{0}", (int)speed);
    }

#endregion
}
