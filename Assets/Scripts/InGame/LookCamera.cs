using UnityEngine;

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