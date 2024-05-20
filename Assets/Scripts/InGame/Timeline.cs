using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using Protocol;

public class Timeline : MonoBehaviour
{
#region PrivateVariables
    [SerializeField, Tooltip("레이스 시작 전에 재생되는 인트로 타임라인")]
    private PlayableDirector introTimeline = null;

    [SerializeField, Tooltip("플레이어가 레이스를 완료했을 때 재생되는 아웃트로 타임라인 (Non-Used)")]
    private PlayableDirector outroTimeline = null;
#endregion

#region PublicVariables
    public static Timeline instance = null;
#endregion

#region PrivateMethod
    private void Awake()
    {
        instance = this;
    }

    private IEnumerator StartTimelineCoroutine()
    {
        // Wait for intro
        if (introTimeline != null && introTimeline.isActiveAndEnabled)
        {
            yield return new WaitWhile(() => introTimeline.state == PlayState.Playing);
        }

        yield return null;
        WorldManager.instance.OnSendInGame(Type.PlayerReady);
    }
#endregion

#region PublicMethod
    public Coroutine StartTimeline()
    {
        return StartCoroutine(StartTimelineCoroutine());
    }
#endregion
}
