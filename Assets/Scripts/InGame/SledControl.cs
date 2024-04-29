using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SledControl : MonoBehaviour
{   
    //Input setting 변수들 저장
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    //private bool isDrifting;

    [SerializeField] private float motorForce;
    [SerializeField] private float brakeForce;
    [SerializeField] private float maxSteerAngle;
    [SerializeField] private bool isBraking;


    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider backLeftWheelCollider;
    [SerializeField] private WheelCollider backRightWheelCollider;


    /* 드리프트 하려면 후륜을 멈추게 한다.-> 관성 때문에 자동차가 미끄러진다.
     * stiffness를 조절한다.
     */
    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        CheckRotate();
        HandleSteering();

    }

    private void GetInput()//키 입력
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);
        //isDrifting = Input.GetKey(KeyCode.LeftShift);
        
        if(Input.GetButton("Jump"))
            isBraking = true;

        if(!Input.GetButton("Jump"))
            isBraking = false;

    }

    private void HandleMotor()//엔진 속도 조절
    {
        //추가 사항 : max 속도 제한, AddForce로 속도 조절
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;

        if (isBraking)//Space 누르고 있을 때
            ApplyBraking();
        else
            ApplyRestart();


        //if(isDrifting)
        //    Drift();

    }

    private void ApplyBraking()//브레이크
    {
        frontLeftWheelCollider.brakeTorque = brakeForce;
        frontRightWheelCollider.brakeTorque = brakeForce;
    }
    
    private void ApplyRestart()//브레이크가 풀렸을 때 엔진 다시 켜기
    {
        frontLeftWheelCollider.brakeTorque = 0;
        frontRightWheelCollider.brakeTorque = 0;
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce; 

    }

    private void CheckRotate()//차량이 절대값 19도 이상으로 기울지 않게
    {
        if (transform.rotation.z > 0.33f)
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0.33f);
        if (transform.rotation.z < -0.33f)
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -0.33f);
    }

    private void HandleSteering()//방향 조정은 전륜만 조정
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;

    }

    //private void Drift()
    //{
    //    Drifting();
    //}


    //private void Drifting()
    //{

    //}
}
