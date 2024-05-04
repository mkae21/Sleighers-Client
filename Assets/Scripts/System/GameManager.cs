using System;
using System.Collections;
using UnityEngine;

/* GameManager.cs
 * - 게임 전체적인 상태를 관리
 * - 인게임 내에서 코루틴 업데이트 실행으로 플레이어 입력 처리
 */
public class GameManager : MonoBehaviour
{
    
#region PrivateVariables
    private static bool isCreate = false;
    private static GameManager instance;
    private IEnumerator ReadyUpdateCoroutine;
    private IEnumerator InGameUpdateCoroutine;
    private GameState gameState;
    #endregion

#region PublicVariables
    public static event Action Ready = delegate { }; // Ready 상태에서 실행되는 함수들
    public static event Action InGame = delegate { }; // InGame 상태에서 실행되는 함수들
    public enum GameState { Login, MatchLobby, Ready, Start, InGame, End, Result, Reconnect };
    public SoundManager soundManager = new SoundManager();
#endregion

#region PrivateMethod
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        // 60프레임 고정
        Application.targetFrameRate = 60;
        // 게임중 슬립모드 해제
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        ReadyUpdateCoroutine = ReadyUpdate();

        InGameUpdateCoroutine = InGameUpdate();

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        if (isCreate)
        {
            DestroyImmediate(gameObject, true);
            return;
        }
        soundManager.Init();
        ChangeState(GameState.Ready);
        isCreate = true;
    }
    // Ready 상태에서 실행되는 코루틴
    private IEnumerator ReadyUpdate()
    {
        while (true)
        {
            if (gameState != GameState.Ready)
            {
                StopCoroutine(ReadyUpdateCoroutine);
                yield return null;
            }
            Ready();
            yield return new WaitForSeconds(0.0333f);
        }
    }

    // 인게임에서 실행되는 코루틴
    private IEnumerator InGameUpdate()
    {
        while (true)
        {
            if (gameState != GameState.InGame)
            {
                StopCoroutine(InGameUpdateCoroutine);
                yield return null;
            }
            InGame();
            yield return new WaitForSeconds(0.0333f);
        }
    }
#endregion

#region PublicMethod
    public static GameManager Instance()
    {
        if (instance == null)
        {
            Debug.LogError("GameManager 인스턴스가 존재하지 않습니다.");
            return null;
        }
        return instance;
    }

    public void ChangeState(GameState state, Action<bool> func = null)
    {
        gameState = state;
        switch (gameState)
        {
            case GameState.Ready:
                StartCoroutine(ReadyUpdateCoroutine);
                break;

            case GameState.InGame:
                soundManager.Play("BGM/InGame", SoundType.BGM);
                StartCoroutine(InGameUpdateCoroutine);
                break;

            case GameState.End:
                soundManager.StopAll();
                break;
            default:
                Debug.Log("[GameManager] 알 수 없는 상태입니다.");
                break;
        }
    }

    public GameState GetGameState()
    {
        return gameState;
    }
#endregion
}
