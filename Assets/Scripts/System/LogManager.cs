using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
#region PublicVariables
    public static LogManager instance;
    public TMP_Text logText;
    public ScrollRect scrollRect;
#endregion

#region PrivateMethod
    private void Awake()
    {
        instance = this;
    }
#endregion

#region PublicMethod
    public void Log(string log)
    {
        if (scrollRect.gameObject.activeSelf)
        {
            logText.text += log + "\n";
            scrollRect.verticalNormalizedPosition = 0;
        }
    }
#endregion
}