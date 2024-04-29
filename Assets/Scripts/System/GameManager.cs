using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{

#region PrivateVariables
    private static bool isCreate = false;
    private static GameManager instance;
    private IEnumerator InGameUpdateCoroutine;
    private GameState gameState;
#endregion

#region PublicVariables
    public static event Action InGame = delegate { };
    public enum GameState { Login, MatchLobby, Ready, Start, InGame, Over, Result, Reconnect };
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
        ChangeState(GameState.InGame); // TODO: 일단 바로 시작. 나중에 바꿔야 함
        isCreate = true;
    }

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
            yield return new WaitForSeconds(0.01f); // 1초 단위
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
            case GameState.InGame:
                // 코루틴 시작
                StartCoroutine(InGameUpdateCoroutine);
                break;
            default:
                Debug.Log("알수없는 스테이트입니다. 확인해주세요.");
                break;
        }
    }

    public GameState GetGameState()
    {
        return gameState;
    }
#endregion
}
