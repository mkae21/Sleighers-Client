using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
#region PublicVariables
    public static InGameUI instance;

    public TextMeshProUGUI text_Timer;
    public TextMeshProUGUI text_CountDown;
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
