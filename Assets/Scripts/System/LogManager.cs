using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    public static LogManager instance;
    public TMP_Text logText;
    public ScrollRect scrollRect;
    private void Awake()
    {
        instance = this;
    }
    public void Log(string log)
    {
        logText.text += log + "\n";
        scrollRect.verticalNormalizedPosition = 0;
    }
}
