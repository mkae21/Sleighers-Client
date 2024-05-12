using UnityEngine;
using TMPro;
using Cinemachine;
using System;
using Protocol;

/* Player.cs
 * - 플레이어의 이동, 회전, 속도 조절
 * - 플레이어 스크립트는 여러 개 생성되기에 여기서 입력 처리를 하지 않는다.
 */
public class Player : MonoBehaviour
{
#region PrivateVariables
    private bool isDrifting;

    // Polation
    private float timeToReachTarget = 0.05f;
    private float movementThreshold = 1f;
    private float squareMovementThreshold;
    private Vector3 previousPosition;
    private long previousTimeStamp;
    private Vector3 toPosition;
    private Vector3 toVelocity;
    private float toRotationY;
    private long toTimeStamp;

    // 최대속도 제한, 드리프트
    private float maxSpeed = 75f;
    private float speed;
    private float currentSpeed;
    private float rotate;

    private float currentRotate;
    private string nickName = string.Empty;
#endregion

#region PublicVariables
    public bool isMe { get; private set; } = false;
    public bool isBraking { get; private set;} = false;
    public int playerId { get; private set; } = -1;
    public Vector3 respawnPosition = Vector3.zero;
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
        squareMovementThreshold = movementThreshold * movementThreshold;
    }
    private void Update()
    {
        if (!isMe && WorldManager.instance.isGameStart)
        {
            Polation();
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
        if (isMove && isMe)
        {
            ApplyPhysics(hitData);
        }
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

        Physics.Raycast(sled.position + (sled.up * .5f), Vector3.down, out hitNear, 2.0f);
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
    private void SetCamera()
    {
        CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.Follow = sled.transform;
    }
    // ExtraPolation, InterPolation
    private void Polation()
    {
        float latency = (toTimeStamp - previousTimeStamp) / 1000f;
        float lerpAmount = Mathf.Clamp01(latency / timeToReachTarget);
        Vector3 fromPosition = sphere.transform.position;

        // Interpolation Position
        if ((toPosition - previousPosition).sqrMagnitude < squareMovementThreshold)
        {
            if (toPosition != fromPosition)
            {
                // Debug.Log("Interpolation");
                sphere.transform.position = Vector3.Lerp(fromPosition, toPosition, lerpAmount);            
            }
        }
        // Extrapolation Position
        else
        {
            // Debug.Log("Extrapolation");
            Vector3 extrapolatedPosition = toPosition + (toVelocity * latency);
            sphere.transform.position = Vector3.Lerp(fromPosition, extrapolatedPosition, lerpAmount);
        }

        // Interpolation Rotation
        float quaternionY = Mathf.Lerp(sled.transform.rotation.eulerAngles.y, toRotationY, 0.75f);
        sled.transform.rotation = Quaternion.Euler(0f, quaternionY, 0f);
    }
#endregion


#region PublicMethod
    // 내 플레이어와 다른 플레이어 객체 초기화
    public void Initialize(bool _isMe, int _playerId, string _nickName, Vector3 position, float rotation)
    {
        isMe = _isMe;
        this.playerId = _playerId;
        this.nickName = _nickName;

        miniMapComponent.enabled = true;

        nameObject = Instantiate(nameObject, Vector3.zero, Quaternion.identity, sledModel);
        nameObject.GetComponent<TMP_Text>().text = this.nickName;
        nameObject.transform.position = GetNameUIPos();

        sled.transform.position = position;
        sphere.transform.position = position;
        sled.transform.rotation = Quaternion.Euler(0, rotation, 0);

        previousPosition = GetPosition();
        previousTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        toPosition = GetPosition();
        toVelocity = GetVelocity();
        toRotationY = GetRotationY();
        toTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        RankManager.instance.AddRankInfo(GetComponent<Player>());
        InGameUI.instance.UpdateRankUI(RankManager.instance.GetRanking());

        if (isMe)
            Invoke("SetCamera", 0.5f);

        this.isMove = false;
        this.moveVector = new Vector3(0, 0, 0);
    }

    public void SetSyncData(Vector3 _position, Vector3 _velocity, float _rotationY, long _timeStamp)
    {
        previousPosition = toPosition;
        previousTimeStamp = toTimeStamp;

        toPosition = _position;
        toVelocity = _velocity;
        toRotationY = _rotationY;
        toTimeStamp = _timeStamp;
    }

    public void SetMoveVector(Vector2 vector)
    {
        moveVector = new Vector3(vector.x, 0, vector.y);

        if (vector == Vector2.zero)
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
    public float GetRotationY()
    {
        return sled.transform.rotation.eulerAngles.y;
    }
    public SyncMessage GetSyncData()
    {
        return new SyncMessage(playerId, GetPosition(), GetVelocity(), GetRotationY(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
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
    public void Respawn()
    {
        sphere.velocity = Vector3.zero;
        sphere.angularVelocity = Vector3.zero;
        sphere.transform.position = respawnPosition;
        sphere.transform.rotation = Quaternion.identity;
        // TODO: 서버로 리스폰 패킷 전송
    }
#endregion
}