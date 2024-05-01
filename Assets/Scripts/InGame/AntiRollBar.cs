using UnityEngine;

public class AntiRollBar : MonoBehaviour
{
#region PrivateVariables
    private Rigidbody Car;

#endregion

#region PublicVariables
    public WheelCollider wheelL;
    public WheelCollider wheelR;
    public float antiRoll = 5000.0f;

#endregion
    void Start()
    {
        Car = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        //접촉 정보 저장
        WheelHit hit;

        float travelL = 1.0f;

        float travelR = 1.0f;

        bool groundedL = wheelL.GetGroundHit(out hit);

        if (groundedL)//지면에 닿았을 때
            travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
        //원래 이상적 거리는 0이지만 압축이 되었다면 음수 값이 나온다. 여기에 -를 붙여 얼마나 압축 되었는지 알 수 있다.

        bool groundedR = wheelR.GetGroundHit(out hit);

        if (groundedR)
            travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;

        float antiRollForce = (travelL - travelR) * antiRoll;

        if (groundedL)
            Car.AddForceAtPosition(wheelL.transform.up * antiRollForce, wheelL.transform.position);

        if (groundedR)
            Car.AddForceAtPosition(wheelR.transform.up * -antiRollForce, wheelR.transform.position);
    }
}
