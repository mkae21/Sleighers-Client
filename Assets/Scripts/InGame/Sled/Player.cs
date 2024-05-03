using UnityEngine;
using TMPro;
using Cinemachine;
using System;

/* Player.cs
 * - 플레이어의 이동, 회전, 속도 조절
 * - 플레이어 스크립트는 여러 개 생성되기에 여기서 입력 처리를 하지 않는다.
 */
public class Player : MonoBehaviour
{
#region PrivateVariables
    private float currentSteerAngle;
    //private bool isDrifting;

    // expolation, slerp
    private Vector3 lastServerPosition;
    private Vector3 lastServerVelocity;
    private Vector3 lastServerAcceleration;
    private long lastServerTimeStamp;
    private float extrapolationLimit = 0.5f;

    private float maxSpeed = 20f;
    private float currentSpeed;
    private float motorForce = 2000f;
    private float brakeForce = 3000f;

    private float maxSteerAngle = 20f;
    private int playerId = 0;
    [SerializeField] private bool isMe = false;
    public bool IsMe
    {
        get { return isMe; }
        set { isMe = value; }
    }
    [SerializeField] private bool isBraking = false;
    public bool IsBraking
    {
        get { return isBraking; }
        set { isBraking = value; }
    }
    private string nickName = string.Empty;
    private GameObject playerModelObject;
    private Rigidbody rb;
#endregion

#region PublicVariables
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider backLeftWheelCollider;
    public WheelCollider backRightWheelCollider;

    [field: SerializeField] public Vector3 moveVector { get; private set; }
    [field: SerializeField] public bool isMove { get; private set; }
    public GameObject nameObject;
    public MiniMapComponent miniMapComponent;
#endregion


#region PrivateMethod
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();//Player의 Rigidbody를 가져옴
        nameObject = Resources.Load("Prefabs/PlayerName") as GameObject;
    }
    private void Start()
    {
        // 서버 인스턴스가 없으면 인게임 테스트용으로 초기화
        if (ServerManager.Instance() == null)
            Initialize(true, 0, "TestPlayer");
    }
    private void Update()
    {
        if (ServerManager.Instance() == null)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 tmp = new Vector3(h, 0, v);
            tmp = Vector3.Normalize(tmp);
            SetMoveVector(tmp);
        }
    }
    private Vector3 GetNameUIPos()
    {
        return this.transform.position + (Vector3.up * 2.0f);
    }

    private void FixedUpdate()
    {
        if (isMove)
            HandleMotor();
        
        if (IsBraking) // Space 누르고 있을 때
            ApplyBraking();
        else
            ApplyRestart();

        //CheckRotate();
        HandleSteering();
    }

    private void HandleMotor() // 엔진 속도 조절
    {
        currentSpeed = rb.velocity.magnitude;

        if (currentSpeed > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        frontLeftWheelCollider.motorTorque = moveVector.z * motorForce;
        frontRightWheelCollider.motorTorque = moveVector.z * motorForce;
        isMove = false;
    }

    private void ApplyBraking() // 브레이크
    {
        IsBraking = false;
        frontLeftWheelCollider.brakeTorque = brakeForce;
        frontRightWheelCollider.brakeTorque = brakeForce;
    }

    private void ApplyRestart()//브레이크가 풀렸을 때 엔진 다시 켜기
    {
        frontLeftWheelCollider.brakeTorque = 0;
        frontRightWheelCollider.brakeTorque = 0;
    }

    // private void CheckRotate()
    // {
    //     Debug.Log("회전 체크중");
    //     if (transform.rotation.z > 0.33f)
    //     {
    //         Debug.Log("힘주는 중");
    //     }
    //     if (transform.rotation.z < -0.33f)
    //     {
    //         Debug.Log("힘주는 중");
    //     }
    // }



    private void HandleSteering()//방향 조정은 전륜만 조정
    {
        currentSteerAngle = maxSteerAngle * moveVector.x;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Finish")
        {
            WorldManager.instance.OnSend(Protocol.Type.PlayerGoal);
            Debug.LogFormat("플레이어 {0} 도착", playerId);
        }
    }

    private void ExtrapolatePosition()
    {
        Quaternion lastServerRotation = Quaternion.LookRotation(lastServerAcceleration);
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float timeSinceLastUpdate = (currentTime - lastServerTimeStamp) / 1000f;
        float interpolationRatio = Mathf.Clamp01(timeSinceLastUpdate / extrapolationLimit);

        if (timeSinceLastUpdate < extrapolationLimit)
        {
            Vector3 extrapolatedPosition = lastServerPosition + (lastServerVelocity * timeSinceLastUpdate) + (0.5f * lastServerAcceleration * timeSinceLastUpdate);
            transform.position = Vector3.Lerp(transform.position, extrapolatedPosition, interpolationRatio);

        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, lastServerPosition, interpolationRatio);
        }
        //Quaternion extrapolatedRotation = Quaternion.Slerp(transform.rotation, lastServerRotation, interpolationRatio);
        //transform.rotation = extrapolatedRotation;
    }
    #endregion


#region PublicMethod
    // 내 플레이어와 다른 플레이어 객체 초기화
    public void Initialize(bool _isMe, int _playerId, string _nickName)
    {
        IsMe = _isMe;
        this.playerId = _playerId;
        this.nickName = _nickName;

        miniMapComponent.enabled = true;
        playerModelObject = this.gameObject;

        nameObject = Instantiate(nameObject, Vector3.zero, Quaternion.identity, playerModelObject.transform);
        nameObject.GetComponent<TMP_Text>().text = this.nickName;
        nameObject.transform.position = GetNameUIPos();

        if (IsMe)
            CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.Follow = this.transform;

        this.isMove = false;
        this.moveVector = new Vector3(0, 0, 0);
    }

    public void SetServerData(Vector3 _position, Vector3 _velocity, Vector3 _acceleration, long _timeStamp)
    {
        lastServerPosition = _position;
        lastServerVelocity = _velocity;
        lastServerAcceleration = _acceleration;
        lastServerTimeStamp = _timeStamp;
    }

    public void SetMoveVector(float move)
    {
        SetMoveVector(this.transform.forward * move);
    }
    public void SetMoveVector(Vector3 vector)
    {
        moveVector = vector;

        if (vector == Vector3.zero)
            isMove = false;
        else
            isMove = true;
        if(!IsMe)
            ExtrapolatePosition();

    }

    public void SetPosition(Vector3 pos)
    {
        gameObject.transform.position = pos;
    }

    // isStatic이 true이면 해당 위치로 바로 이동
    public void SetPosition(float x, float y, float z)
    {
        Vector3 pos = new Vector3(x, y, z);
        SetPosition(pos);
    }

    public Vector3 GetPosition()
    {
        return gameObject.transform.position;
    }

    public Vector3 GetRotation()
    {
        return gameObject.transform.rotation.eulerAngles;
    }
    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    public float GetSpeed()
    {   
        // km/h로 변환
        return rb.velocity.magnitude * 3.6f;
    }
    // 앞으로 나아가는 차량 속도의 양
    public float ForwardSpeed => Vector3.Dot(rb.velocity, transform.forward);

    // 차량의 최대 속도에 상대적인 전진 속도를 반환 
    // 반환되는 값은 [-1, 1] 범위
    public float NormalizedForwardSpeed
    {
        get => (Mathf.Abs(ForwardSpeed) > 0.1f) ? ForwardSpeed / maxSpeed : 0.0f;
    }
#endregion
}