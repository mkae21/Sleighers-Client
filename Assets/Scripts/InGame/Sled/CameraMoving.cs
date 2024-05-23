using UnityEngine;
using Cinemachine;

public class CameraMoving : MonoBehaviour
{
#region PublicVariables
    public CinemachineBlendListCamera blendListCamera;
    public CinemachineVirtualCamera vCam1;
#endregion

#region PrivateMethod
    private void Start()
    {
        blendListCamera.m_Instructions[0].m_VirtualCamera = vCam1;
        vCam1.m_Lens.NearClipPlane = 0.1f;
        vCam1.m_Lens.FarClipPlane = 300.0f;
    }
#endregion
}
