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
    private bool isDrifting;

    // expolation, slerp
    private Vector3 lastServerPosition;
    private Vector3 lastServerVelocity;
    private Quaternion lastServerRotation;
    private long lastServerTimeStamp;
    private float extrapolationLimit = 0.5f;

    // 최대속도 제한, 드리프트
    private float maxSpeed = 75f;
    private float speed;
    private float currentSpeed;
    private float rotate;

    private float currentRotate;
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
#endregion

#region PublicVariables
    public int playerId { get; private set; } = -1;
    public Rigidbody sphere;
    public Transform sledModel;
    public Transform sled;

    [Header("Parameters")]
    public float acceleration = 50f;
    public float steering = 40f;
    public float gravity = 10f;
    public float amount;

    [field: SerializeField] public Vector3 moveVector { get; private set; }
    [field: SerializeField] public bool isMove { get; private set; }
    public GameObject nameObject;
    public MiniMapComponent miniMapComponent;
#endregion


#region PrivateMethod
    private void Awake()
    {
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

        SledPosition();
        SteerHandle();
        GetVerticalSpeed();
        CuerrentValue();
        CheckVelocity();
    }

    private void FixedUpdate()
    {
        RaycastHit hitData = AdJustBottom();
        if (isMove)
            ApplyPhysics(hitData);
    }

    private void GetVerticalSpeed()
    {
        if (moveVector.z > 0)
            speed = acceleration;
        else if (moveVector.z < 0)
            speed = -acceleration;
        else
            speed = 0;
    }

    private void SteerHandle()
    {
        if (moveVector.x != 0)
        {
            int dir = moveVector.x > 0 ? 1 : -1;
            
            if(!isDrifting){
                amount = Mathf.Abs(moveVector.x);
            }
            else
                amount = Math.Abs(moveVector.x) * 10f;//더 크게 회전
            
            Steer(dir, amount);
        }
    }
    private void CuerrentValue()
    {
        currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 10f);
        speed = 0f; // Reset for next frame
        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
        rotate = 0f; // Reset for next frame
    }

    private Vector3 GetNameUIPos()
    {
        return this.transform.position + (Vector3.up * 2.0f);
    }

    private void ApplyPhysics(RaycastHit hitNear)
    {

        if (hitNear.collider != null)// If the sled is on the ground
        {
            sphere.AddForce(sledModel.forward * currentSpeed, ForceMode.Acceleration);
            sled.eulerAngles = Vector3.Lerp(sled.eulerAngles, new Vector3(0, sled.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);
        }
        else
            sphere.AddForce(sledModel.forward * sphere.velocity.magnitude, ForceMode.Acceleration);


        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration); //Apply gravity
        isMove = false;
        isDrifting = false;
        if (!IsMe)
            ExtrapolatePosition();
    }
    public void Steer(int direction, float amount)
    {
        rotate = (steering * direction) * amount;
    }
    private void SledPosition()
    {
        sled.transform.position = sphere.transform.position - new Vector3(0, 1.2f, 0);
    }
    private RaycastHit AdJustBottom()
    {
        RaycastHit hitNear;

        Physics.Raycast(sled.position + (sled.up * .1f), Vector3.down, out hitNear, 2.0f);

        sledModel.up = Vector3.Lerp(sledModel.up, hitNear.normal, Time.deltaTime * 8.0f);
        sledModel.Rotate(0, sled.eulerAngles.y, 0);

        return hitNear;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Finish")
        {
            WorldManager.instance.OnSend(Protocol.Type.PlayerGoal);
            Debug.LogFormat("플레이어 {0} 도착", playerId);
        }
    }

    private void CheckVelocity()
    {
        if (sphere.velocity.magnitude > maxSpeed)
        {
            sphere.velocity = sphere.velocity.normalized * maxSpeed;
        }
    }
    private void ExtrapolatePosition()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        float timeSinceLastUpdate = (currentTime - lastServerTimeStamp) / 1000f;
        float interpolationRatio = Mathf.Clamp01(timeSinceLastUpdate / extrapolationLimit);

        if (timeSinceLastUpdate < extrapolationLimit)
        {
            Vector3 extrapolatedPosition = lastServerPosition + (lastServerVelocity * timeSinceLastUpdate);
            sphere.transform.position = Vector3.Lerp(sphere.transform.position, extrapolatedPosition, interpolationRatio);
        }
        else
        {
            sphere.transform.position = Vector3.Lerp(sphere.transform.position, lastServerPosition, interpolationRatio);
        }
        sled.transform.rotation = Quaternion.Slerp(sled.transform.rotation, lastServerRotation, interpolationRatio);
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

        nameObject = Instantiate(nameObject, Vector3.zero, Quaternion.identity, sledModel);
        nameObject.GetComponent<TMP_Text>().text = this.nickName;
        nameObject.transform.position = GetNameUIPos();

        RankManager.instance.AddRankInfo(GetComponent<Player>());
        InGameUI.instance.UpdateRankUI(RankManager.instance.GetRanking());

        if (IsMe)
            CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.Follow = sled.transform;

        this.isMove = false;
        this.moveVector = new Vector3(0, 0, 0);
    }

    public void SetServerData(Vector3 _position, Vector3 _velocity, Quaternion _rotation, long _timeStamp)
    {
        lastServerPosition = _position;
        lastServerVelocity = _velocity;
        lastServerRotation = _rotation;
        lastServerTimeStamp = _timeStamp;
    }

    public void SetMoveVector(Vector3 vector)
    {
        moveVector = vector;

        if (vector == Vector3.zero)
            isMove = false;
        else
            isMove = true;
    }

    public Vector3 GetPosition()
    {
        return sphere.transform.position;
    }
    public Vector3 GetVelocity()
    {
        return sphere.velocity;
    }
    public Vector3 GetRotation()
    {
        return sled.transform.rotation.eulerAngles;
    }

    public void SetDrift(bool isDrift)
    {
        isDrifting = isDrift;
    }
    public float GetSpeed()
    {   
        // km/h로 변환
        return sphere.velocity.magnitude * 3.6f;
    }

    // 앞으로 나아가는 차량 속도의 양
    public float ForwardSpeed => Vector3.Dot(sphere.velocity, transform.forward);

    // 차량의 최대 속도에 상대적인 전진 속도를 반환 
    // 반환되는 값은 [-1, 1] 범위
    public float NormalizedForwardSpeed
    {
        get => (Mathf.Abs(ForwardSpeed) > 0.1f) ? Mathf.Abs(ForwardSpeed) * 5 / maxSpeed : 0.0f;
    }
#endregion
}