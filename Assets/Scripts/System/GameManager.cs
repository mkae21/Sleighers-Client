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
/* Login - 로그인 전 상태
 * Lobby - 로비
 * Garage - 차고
 * Record - 전적 리스트
 * Friend - 친구 리스트
 * MatchResult - 매치메이킹 성사
 * Ready - 게임 시작 전 준비 (이때부터 인게임 씬)
 * InGame - 게임 중
 * End - 게임 종료 (피니시 라인 통과 시)
 * Result - 게임 결과
 */
    public static event Action Login = delegate { };        // Login 상태에서 실행되는 함수들
    public static event Action Lobby = delegate { };        // Lobby 상태에서 실행되는 함수들
    public static event Action Garage = delegate { };       // Garage 상태에서 실행되는 함수들
    public static event Action Record = delegate { };       // Record 상태에서 실행되는 함수들
    public static event Action Friend = delegate { };       // Friend 상태에서 실행되는 함수들
    public static event Action MatchMaking = delegate { };  // MatchMaking 상태에서 실행되는 함수들
    public static event Action MatchReady = delegate { };   // MatchReady 상태에서 실행되는 함수들
    public static event Action MatchResult = delegate { };  // MatchResult 상태에서 실행되는 함수들
    public static event Action Ready = delegate { };        // Ready 상태에서 실행되는 함수들
    public static event Action InGame = delegate { };       // InGame 상태에서 실행되는 함수들
    public static event Action End = delegate { };          // End 상태에서 실행되는 함수들
    public static event Action Result = delegate { };       // Result 상태에서 실행되는 함수들
    public enum GameState { Login, Lobby, Garage, Record, Friend, MatchMaking, MatchReady, MatchResult, Ready, InGame, End, Result };
    public SoundManager soundManager = new SoundManager();
#endregion

#region PrivateMethod
    private void Awake()
    {
        if (!instance)
            instance = this;
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
        isCreate = true;
        soundManager.Init();
        ChangeState(GameState.Login);
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
            case GameState.Login:
                Login();
                break;
            case GameState.Lobby:
                Lobby();
                break;
            case GameState.Garage:
                Garage();
                break;
            case GameState.Record:
                Record();
                break;
            case GameState.Friend:
                Friend();
                break;
            case GameState.MatchMaking:
                MatchMaking();
                break;
            case GameState.MatchReady:
                MatchReady();
                break;
            case GameState.MatchResult:
                MatchResult();
                break;
            case GameState.Ready:
                soundManager.Play("BGM/Wind", SoundType.WIND);
                StartCoroutine(ReadyUpdateCoroutine);
                break;
            case GameState.InGame:
                soundManager.Play("BGM/InGame", SoundType.BGM);
                StartCoroutine(InGameUpdateCoroutine);
                break;
            case GameState.End:
                End();
                soundManager.StopAll();
                break;
            case GameState.Result:
                Result();
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
