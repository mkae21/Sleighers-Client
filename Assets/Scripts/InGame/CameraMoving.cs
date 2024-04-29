using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

public class CameraMoving : MonoBehaviour
{
    public CinemachineBlendListCamera blendListCamera;

    public CinemachineVirtualCamera vCam1;
    public CinemachineVirtualCamera vCam2;

    private bool isFirstPersonView = false;
    
    // Start is called before the first frame update
    void Start()
    {
        blendListCamera.m_Instructions[0].m_VirtualCamera = vCam1;
        blendListCamera.m_Instructions[1].m_VirtualCamera = vCam2;
        blendListCamera.m_Instructions[0].m_Hold = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //tab키로 가상 카메라 전환
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            isFirstPersonView = !isFirstPersonView;
        }

        blendListCamera.m_Instructions[0].m_VirtualCamera = isFirstPersonView ? vCam2 : vCam1;
        blendListCamera.m_Instructions[1].m_VirtualCamera = isFirstPersonView ? vCam1 : vCam2;
        blendListCamera.m_Instructions[0].m_Hold = isFirstPersonView ? 0.0f : 1.0f;
    }
}
