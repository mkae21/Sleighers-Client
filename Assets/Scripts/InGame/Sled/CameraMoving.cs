using UnityEngine;
using Cinemachine;

public class CameraMoving : MonoBehaviour
{
#region PublicVariables
    public CinemachineBlendListCamera blendListCamera;
    public CinemachineVirtualCamera vCam1;
    public CinemachineVirtualCamera vCam2;
#endregion

#region PrivateVariables
    private bool isFirstPersonView = false;
#endregion

#region PrivateMethod
    private void Start()
    {
        blendListCamera.m_Instructions[0].m_VirtualCamera = vCam1;
        blendListCamera.m_Instructions[1].m_VirtualCamera = vCam2;
        blendListCamera.m_Instructions[0].m_Hold = 1.0f;
        vCam1.m_Lens.NearClipPlane = 0.1f;
        vCam2.m_Lens.NearClipPlane = 0.1f;
        vCam1.m_Lens.FarClipPlane = 300.0f;
        vCam2.m_Lens.FarClipPlane = 300.0f;
    }
    private void Update()
    {
        //tab키로 가상 카메라 전환
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            isFirstPersonView = !isFirstPersonView;
            
            blendListCamera.m_Instructions[0].m_VirtualCamera = isFirstPersonView ? vCam2 : vCam1;
            blendListCamera.m_Instructions[1].m_VirtualCamera = isFirstPersonView ? vCam1 : vCam2;
            blendListCamera.m_Instructions[0].m_Hold = isFirstPersonView ? 0.0f : 1.0f;
        }
    }
#endregion
}
