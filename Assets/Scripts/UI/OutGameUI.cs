using UnityEngine;
using UnityEngine.UI;

public class OutGameUI : MonoBehaviour
{
#region PublicVariables
    public static OutGameUI instance;
    
    [Space(10), Header("===== TopBar =====")]
    public Button lobbyBtn;
    public Button gargageBtn;
    public Button recordBtn;
    public Button friendBtn;
    public Button settingBtn;

    [Space(10), Header("===== Lobby =====")]
    public Button goWaitingBtn;

    [Space(10), Header("===== Setting =====")]
    public GameObject checkExitPanel;
    public GameObject settingPanel;
    public Button gameExitBtn;
    public Button realExitBtn;
    public Button settingBackBtn;
    public Toggle soundToggle;
    public Slider volumeSlider;

    [Space(10), Header("===== Store =====")]
    public GameObject storePanel;
    public GameObject[] sledList;
    public int sledListCnt;
    
    [Space(10), Header("===== Waiting Room =====")]
    public GameObject waitingRoomPanel;
    public Button waitingRoomStartBtn;

#endregion

#region PrivateMethod
    private void Start()
    {
        settingPanel.SetActive(false);
        checkExitPanel.SetActive(false);
        waitingRoomPanel.SetActive(false);
        storePanel.SetActive(false);
        instance = this;
    }
#endregion

#region PublicMethod
    public void ToggleSound()
    {
        if(soundToggle.isOn)
        {
            AudioListener.volume = 1;
            soundToggle.isOn = true;
        }
        else
        {
            AudioListener.volume = 0;
            soundToggle.isOn = false;
        }
    }

    public void VolumeSlider()
    {
        AudioListener.volume = volumeSlider.value;
    }
    public void RealExitGame()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }
#endregion
}