using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class OutGameUI : MonoBehaviour
{
#region PublicVariables
    public static OutGameUI instance;
    
    [Space(10), Header("===== Panels =====")]
    public GameObject[] panels;
    [Space(10), Header("===== Setting =====")]
    public Toggle volumeToggle;
    public Slider volumeSlider;
    public TMP_InputField setNicknameField;
    public Button setNicknameBtn;
    public Toggle backgroundPostProcessingToggle;
    public Toggle speedPostProcessingToggle;
    [Space(10), Header("===== Matching Room =====")]
    public Button matchMakingBtn;
    public GameObject loadingObject;
    public TMP_Text matchMakingBtnText;
    public GameObject PlayerMatchList;

    [Space(10), Header("===== Login =====")]
    public TMP_InputField loginID;
    public TMP_InputField loginPW;
    public Button defaultLoginBtn;
    public Button defaultSignupBtn;
    public Button googleLoginBtn;

    [Space(10), Header("===== SignUp =====")]
    public TMP_InputField signupID;
    public TMP_InputField signupPW;
    public TMP_InputField signupNickname;
    [Space(10), Header("===== Bar =====")]
    public GameObject topBar;
    [Space(10), Header("===== Store =====")]
    public GameObject[] sledList;
    public int sledListCnt;
#endregion

#region PrivateVariables
    private GameObject PlayerMatchElemPrefabs;
#endregion

#region PrivateMethod
    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
    }

    private void Start()
    {
        defaultLoginBtn.onClick.AddListener(() => 
        {
            PlayerInfo playerInfo = new PlayerInfo()
            {
                email = loginID.text,
                password = loginPW.text
            };
            ServerManager.instance.OnSendOutGame(API.Type.login, playerInfo);
        });
        defaultSignupBtn.onClick.AddListener(() => 
        {
            PlayerInfo playerInfo = new PlayerInfo()
            {
                email = signupID.text,
                password = signupPW.text,
                nickname = signupNickname.text
            };
            ServerManager.instance.OnSendOutGame(API.Type.signup, playerInfo);
        });
        setNicknameBtn.onClick.AddListener(() => 
        {
            PlayerInfo playerInfo = new PlayerInfo()
            {
                email = ServerManager.instance.myEmail,
                nickname = setNicknameField.text
            };
            ServerManager.instance.myNickname = setNicknameField.text;
            ServerManager.instance.OnSendOutGame(API.Type.setName, playerInfo);
            setNicknameField.text = "";
        });
        googleLoginBtn.onClick.AddListener(() => GameManager.Instance().ChangeState(GameManager.GameState.Lobby));
        matchMakingBtn.onClick.AddListener(() => GameManager.Instance().ChangeState(GameManager.GameState.MatchMaking));

        volumeToggle.onValueChanged.AddListener((value) => SettingManager.VolumeToggle(value));
        volumeToggle.onValueChanged.AddListener((value) => volumeSlider.interactable = value);
        volumeSlider.onValueChanged.AddListener((value) => SettingManager.VolumeSlide(value)); 
        volumeSlider.value = 0.5f;

        backgroundPostProcessingToggle.isOn = SettingManager.backgroundPostProcessing;
        speedPostProcessingToggle.isOn = SettingManager.speedPostProcessing;
        backgroundPostProcessingToggle.onValueChanged.AddListener((value) => SettingManager.ToggleBackgroundPostProcessing());
        speedPostProcessingToggle.onValueChanged.AddListener((value) => SettingManager.ToggleSpeedPostProcessing());
        
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        foreach(Button button in buttons)
        {
            button.onClick.AddListener(()=> GameManager.Instance().soundManager.Play("Effect/Click", SoundType.EFFECT));
        }
        Toggle[] toggles = Resources.FindObjectsOfTypeAll<Toggle>();
        foreach(Toggle toggle in toggles)
        {
            toggle.onValueChanged.AddListener((value) => GameManager.Instance().soundManager.Play("Effect/Click", SoundType.EFFECT));
        }

        PlayerMatchElemPrefabs = Resources.Load<GameObject>("UI/PlayerMatchElem");
        
        if (ServerManager.Instance().myNickname != string.Empty)
            OnLobbyPanel();
    }
#endregion

#region PublicMethod
    public void OnLobbyPanel()
    {
        panels[0].SetActive(false);  // auth panel
        panels[1].SetActive(true);   // lobby panel
        topBar.SetActive(true);
    }

    public void MatchMakingUI()
    {
        matchMakingBtnText.text = "매칭중";
        loadingObject.SetActive(true);
    }
    public void ReturnMatchMakingUI()
    {
        matchMakingBtnText.text = "매치메이킹";
        loadingObject.SetActive(false);
    }

    public void DrawMatchPlayer(string name)
    {
        GameObject playerObject = Instantiate(PlayerMatchElemPrefabs, PlayerMatchList.transform);
        playerObject.GetComponentInChildren<TMP_Text>().text = name;
    }

    public void DestroyMatchPlayer()
    {
        foreach(Transform child in PlayerMatchList.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void PopupMatchMakingPanel()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == 6)
                panels[i].SetActive(true);
            else
            {
                panels[i].SetActive(false);
                topBar.SetActive(false);
            }       
        }
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
#endregion
}