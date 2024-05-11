using UnityEngine;
/* LookCamera.cs
 * - 닉네임 UI가 카메라를 바라보도록 설정
 */
public class LookCamera : MonoBehaviour
{
#region PrivateVariables
    private GameObject cam;
#endregion

#region PrivateMethod
    private void Start()
    {
        cam = GameObject.Find("Main Camera");
    }
    private void Update() 
    {
        transform.rotation = cam.transform.rotation;
    }
#endregion
}