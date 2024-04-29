using System;

/* InfiniteLoopDetector.cs
 * 무한 루프 검사 및 방지(에디터 전용)
 * https://rito15.github.io/posts/unity-memo-prevent-infinite-loop/
 *
 * 사용법:
 * InfiniteLoopDetector.Run();
 * 을 무한 루프가 발생할 수 있는 지점에 넣어주면 됩니다.
 *
 * 예시:
    while( condition )
    {
        codes..

        InfiniteLoopDetector.Run();
    }
 */
public static class InfiniteLoopDetector
{
    private static string prevPoint = "";
    private static int detectionCount = 0;
    private const int DetectionThreshold = 100000;

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Run(
        [System.Runtime.CompilerServices.CallerMemberName] string mn = "",
        [System.Runtime.CompilerServices.CallerFilePath] string fp = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int ln = 0
    )
    {
        string currentPoint = $"{fp}:{ln}, {mn}()";

        if (prevPoint == currentPoint)
            detectionCount++;
        else
            detectionCount = 0;

        if (detectionCount > DetectionThreshold)
            throw new Exception($"Infinite Loop Detected: \n{currentPoint}\n\n");

        prevPoint = currentPoint;
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void Init()
    {
        UnityEditor.EditorApplication.update += () =>
        {
            detectionCount = 0;
        };
    }
#endif
}