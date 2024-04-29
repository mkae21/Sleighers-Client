using System;
using UnityEngine;

/* 상속 사용 예시
internal sealed class DerivedSingletonMono : SingletonMonoBase<DerivedSingletonMono>
{
    protected override void Awake()
    {
       base.Awake();
       // 필요시 부가적인 처리 ...
    }

    protected override void OnApplicationQuit()
    {
       base.OnApplicationQuit();
       // ...
    }

    protected override void OnDestroy()
    {
       base.OnDestroy();
       // ...
    }
}
*/

public abstract class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private static readonly Lazy<T> _lazy = new Lazy<T>(GetSingleton);

    public static T Instance => _lazy.Value;

    private static T GetSingleton()
    {
        T monobehaviourInstance = FindObjectOfType<T>();
        if (monobehaviourInstance == null)
        {
            GameObject gameObj = new GameObject(typeof(T).Name);
            monobehaviourInstance = gameObj.AddComponent<T>();
            DontDestroyOnLoad(gameObj);
        }
        return monobehaviourInstance;
    }
    
    // 필요시 Unity Message를 통한 부가적인 처리
    protected virtual void Awake() { }
    protected virtual void OnDestroy() { }
    protected virtual void OnApplicationQuit() { }
}



// public class SingletonBase<T> : MonoBehaviour where T : class, new()
// {
//     protected static T _instance = null;
//     public static T GetSingleton()
//     {
//         if (_instance == null)
//         {
//             Debug.LogError("Singleton Class is null: " + typeof(T));
//             return null;
//         }

//         return _instance;
//     }

//     void Awake()
//     {
//         if (_instance == null)
//         {
//             _instance = this as T;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }
// }