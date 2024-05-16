using System;
using System.Collections;
using System.Collections.Generic;
using Protocol;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/* GameManager.cs
 * - 게임 전체적인 상태를 관리
 */
public class GameManager : MonoBehaviour
{

#region PrivateVariables
    private static bool isCreate = false;
    private static GameManager instance;
    private IEnumerator InGameUpdateCoroutine;
    private GameState gameState;
#endregion

#region PublicVariables
/* Login - 로그인/회원가입
 * Lobby - 로비
 * MatchMaking - 매치메이킹 중
 * MatchResult - 매치메이킹 성사 시
 * Ready - 게임 시작 전 준비 (이때부터 인게임 씬)
 * InGame - 게임 중
 * End - 게임 종료 (피니시 라인 통과 시) ? 결과창 ?
 */
    public static event Action Login = delegate { };        // Login 상태에서 실행되는 함수들
    public static event Action Lobby = delegate { };        // Lobby 상태에서 실행되는 함수들
    public static event Action MatchMaking = delegate { };  // MatchMaking 상태에서 실행되는 함수들
    public static event Action MatchResult = delegate { };  // MatchResult 상태에서 실행되는 함수들
    public static event Action Ready = delegate { };        // Ready 상태에서 실행되는 함수들
    public static event Action InGame = delegate { };       // InGame 상태에서 실행되는 함수들
    public static UnityAction<List<PlayerResult>> End;   // 게임이 끝나고 결과창을 띄울 때 실행되는 함수

    public enum GameState { Login, Lobby, MatchMaking, MatchResult, Ready, InGame, End };
    public SoundManager soundManager = new SoundManager();
#endregion

#region PrivateMethod
    private void Awake()
    {
        if (!instance)
            instance = this;
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
        isCreate = true;
        soundManager.Init();
        ChangeState(GameState.Login);
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
            Debug.LogWarning("GameManager 인스턴스가 존재하지 않습니다.");
            return null;
        }
        return instance;
    }

    public void ChangeState(GameState state, Message msg = null)
    {
        gameState = state;

        switch (gameState)
        {
            case GameState.Login:
                soundManager.Play("BGM/Lobby", SoundType.BGM);
                Login();
                break;
            case GameState.Lobby:
                Lobby();
                break;
            case GameState.MatchMaking:
                MatchMaking();
                break;
            case GameState.MatchResult:
                MatchResult();
                break;
            case GameState.Ready:
                SceneManager.LoadScene("InGame");
                soundManager.Stop("BGM/Lobby", SoundType.BGM);
                soundManager.Play("BGM/Wind", SoundType.WIND);
                soundManager.Play("BGM/InGame", SoundType.BGM);
                Ready();
                break;
            case GameState.InGame:
                StartCoroutine(InGameUpdateCoroutine);
                break;
            case GameState.End:
                GameEndMessage gameResult = (GameEndMessage)msg;
                End?.Invoke(gameResult.resultList);
                soundManager.StopAll();
                break;
            default:
                Debug.Log("[GameManager] 알 수 없는 상태입니다.");
                break;
        }
    }
#endregion
}
