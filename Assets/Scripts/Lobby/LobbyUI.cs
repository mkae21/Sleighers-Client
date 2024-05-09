using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
#region Pulbic Variables 
    public static LobbyUI instance;
    
    [Space(10), Header("===== LobbyUI =====")]
    [Header("TopBar")]
    public Button lobbyBtn;
    public Button gargageBtn;
    public Button storeBtn;
    public Button recordBtn;
    public Button friendBtn;
    public Button settingBtn;

    [Header("Main")]
    public Button goWaitingBtn;

    [Header("Setting")]
    public GameObject checkExitPanel;
    public GameObject settingPanel;
    public Button panelCloseBtn;
    public Button gameExitBtn;
    public Button realExitBtn;
    public Button settingBackBtn;
    public Toggle soundToggle;
    public Slider volumeSlider;

    [Header("Store")]
    public GameObject storePanel;
    public GameObject[] sledList;
    public int sledListCnt;
    
    [Space(10), Header("===== WaitingRoomUI =====")]
    public GameObject waitingRoomPanel;
    public Button waitingRoomBackBtn;
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

    public void TogglePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }

    // ?��?�� ?��?�� On/Off
    public void ToggleSettingPanel()
    {
        TogglePanel(settingPanel);
    }

    // 게임 종료 체크 ?��?�� On/Off
    public void ToggleCheckExitPanel()
    {
        TogglePanel(checkExitPanel);
    }
    // ?��?�� ?��?�� On/Off
    public void ToggleStorePanel()
    {
        TogglePanel(storePanel);
    }

    // ???기실 ?��?�� On/Off
    public void ToggleWaitingRoom()
    {
        TogglePanel(waitingRoomPanel);
    }

    // ?��?��?�� On/Off ?��?��
    public void ToggleSound()
    {
        if(soundToggle.isOn)
        {
            AudioListener.volume = 1;   //TODO: ?���? ?��?���? ?�� ?��?��?�� 추후?�� ?��?��?��?�� ?��
            soundToggle.isOn = true;
        }
        else
        {
            AudioListener.volume = 0;
            soundToggle.isOn = false;
        }
    }

    // 볼륨 조절 ?��?��
    public void VolumeSlider()
    {
        AudioListener.volume = volumeSlider.value;  //TODO: ?���? ?��?���? ?�� ?��?��?�� 추후?�� ?��?��?��?�� ?��
    }

    // 게임 종료
    public void RealExitGame()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }


#endregion

}
