using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
#region Pulbic Variables 

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
    }

    private void Update()
    {
        
    }
#endregion

#region PublicMethod
    
    public void TogglePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }

    // 설정 패널 On/Off
    public void ToggleSettingPanel()
    {
        TogglePanel(settingPanel);
    }

    // 게임 종료 체크 패널 On/Off
    public void ToggleCheckExitPanel()
    {
        TogglePanel(checkExitPanel);
    }
    // 상점 패널 On/Off
    public void ToggleStorePanel()
    {
        TogglePanel(storePanel);
    }

    // 대기실 패널 On/Off
    public void ToggleWaitingRoom()
    {
        TogglePanel(waitingRoomPanel);
    }

    // 사운드 On/Off 함수
    public void ToggleSound()
    {
        if(soundToggle.isOn)
        {
            AudioListener.volume = 1;   //TODO: 아직 노래를 안 넣어서 추후에 수정해야 함 
            soundToggle.isOn = true;
        }
        else
        {
            AudioListener.volume = 0;
            soundToggle.isOn = false;
        }
    }

    // 볼륨 조절 함수
    public void VolumeSlider()
    {
        AudioListener.volume = volumeSlider.value;  //TODO: 아직 노래를 안 넣어서 추후에 수정해야 함
    }

    // 게임 종료
    public void RealExitGame()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }

#endregion

}
