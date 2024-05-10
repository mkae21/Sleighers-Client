using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OutGameUI : MonoBehaviour
{
#region PublicVariables
    public static OutGameUI instance;
    
    [Space(10), Header("===== Panels =====")]
    public GameObject[] panels;
    [Space(10), Header("===== Setting =====")]
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
    public void ClosePanels()
    {
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
    }
    public void ToggleObject(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }
    public void LoadInGame()
    {
        GameManager.Instance().ChangeState(GameManager.GameState.Ready);
        SceneManager.LoadScene("InGame");
    }
#endregion
}