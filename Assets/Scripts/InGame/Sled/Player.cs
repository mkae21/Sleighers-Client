using UnityEngine;
using TMPro;
using Cinemachine;
using System;
using Protocol;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;

/* Player.cs
 * - 플레이어의 이동, 회전, 속도 조절
 * - 플레이어 스크립트는 여러 개 생성되기에 여기서 입력 처리를 하지 않는다.
 */
public class Player : MonoBehaviour
{
#region PrivateVariables
    private bool isDrifting;
    // Polation
    private bool onRamp = false;
    private float timeToReachTarget = 0.5f;
    private float movementThreshold = 4f;
    private float squareMovementThreshold;
    private Vector3 previousPosition;
    private long previousTimeStamp;
    private Vector3 toPosition;
    private Vector3 toVelocity;
    private float toRotationY;
    private long toTimeStamp;

    // 최대속도 제한, 드리프트
    private float maxSpeed = 50f;
    private float speed;
    private float currentSpeed;
    private float rotate;

    private float currentRotate;
    private Animator animator;

#endregion

#region PublicVariables
    public bool isMe { get; private set; } = false;
    public bool isBraking { get; private set;} = false;
    public string nickname {get; private set;} = string.Empty;
    public Transform curCheckpoint;
    public Transform nextCheckpoint;
    public Rigidbody sphere;
    public Transform sledModel;
    public Transform sled;
    public Transform playerModel;

    [Header("Parameters")]
    public float acceleration = 50f;
    public float steering = 40f;
    public float gravity = 1f;
    public float amount;

    [field: SerializeField] public Vector3 moveVector { get; private set; }
    [field: SerializeField] public bool isMove { get; private set; }
    public GameObject nameObject;
    public MiniMapComponent miniMapComponent;
    public int myRank = 1;
#endregion


#region PrivateMethod
    private void Awake()
    {
        nameObject = Resources.Load("Prefabs/PlayerName") as GameObject;
        animator = playerModel.GetComponent<Animator>();
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
                
        if(isMe)
            BlurEffect();
    }

    private void FixedUpdate()
    {
        RaycastHit hitData = AdJustBottom();
        if (isMove && isMe)
        {
            ApplyPhysics(hitData);
        }
        CheckVelocity();
    }

    private void GetVerticalSpeed()
    {
        if (moveVector.z > 0)
            speed = acceleration;
        else if (moveVector.z < 0)
            speed = -acceleration / 2;
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
                amount = Math.Abs(moveVector.x) * 5f;//더 크게 회전
            
            Steer(dir, amount);
        }
    }
    private void CuerrentValue()
    {
        currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 8f);
        speed = 0f; // Reset for next frame
        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 2f);
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
            float weight = (myRank - 1) * 3f; // 등수에 따른 속도 가중치
            sphere.AddForce(sledModel.forward * (currentSpeed + weight), ForceMode.Acceleration);
            sled.eulerAngles = Vector3.Lerp(sled.eulerAngles, new Vector3(0, sled.eulerAngles.y + currentRotate, 0), Time.fixedDeltaTime * 3f);
            
            if(hitNear.collider.gameObject.tag == "Ramp")
            {
                onRamp = true;
                sphere.AddForce(sledModel.forward * currentSpeed, ForceMode.Acceleration);
            }
        }
        else{// If the sled is in the air
            if(onRamp)
            {
                // Ramp를 벗어날 때의 방향과 속도를 기반으로 힘을 가함
                Vector3 launchDirection = sledModel.forward * 15f + sledModel.up * 25f;
                sphere.AddForce(launchDirection.normalized * sphere.velocity.magnitude * 55f, ForceMode.Impulse);
                onRamp = false;
            }
        }
        
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);//중력 작용

        isMove = false;
        isDrifting = false;
        animator.SetBool("isMove", false);
    }
    public void Steer(int direction, float amount)
    {
        rotate = (steering * direction) * amount;
    }
    private void SledPosition()
    {
        sled.transform.position = sphere.transform.position - new Vector3(0, 1.2f, 0);
        playerModel.transform.position = sphere.transform.position - new Vector3(0, 0.5f, 0);
    }
    private RaycastHit AdJustBottom()
    {
        RaycastHit hitNear;

        Physics.Raycast(sled.position + (sled.up * .5f) + (sled.forward * 0.5f), Vector3.down, out hitNear, 2.0f);
        sledModel.up = Vector3.Lerp(sledModel.up, hitNear.normal, Time.deltaTime * 8.0f);
        sledModel.Rotate(0, sled.eulerAngles.y, 0);
        
        playerModel.up = Vector3.Lerp(playerModel.up, hitNear.normal, Time.deltaTime * 8.0f);
        playerModel.Rotate(0, sled.eulerAngles.y, 0);
        
        return hitNear;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Finish")
        {
            WorldManager.instance.OnSendInGame(Protocol.Type.PlayerGoal);
            Debug.LogFormat("플레이어 {0} 도착", nickname);
        }
    }

    private void CheckVelocity()
    {
        float maxWeight = (myRank - 1) * 5f;
        maxSpeed = 50f + maxWeight;//등수에 따른 최대 속도 증가
        if (sphere.velocity.magnitude > maxSpeed)
        {
            sphere.velocity = sphere.velocity.normalized * maxSpeed;
        }
    }
    private void SetCamera()
    {
        CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.Follow = sled.transform;
    }

    private bool IsMoving(Vector3 fromPosition, Vector3 toPosition)
    {   
        //일정 거리 이상 움직였는지 여부를 판단한다.
        float movementThreshold = 0.1f; //움직임 여부를 판단하는 임계값
        return Vector3.Distance(fromPosition, toPosition) > movementThreshold;
    }

    // ExtraPolation, InterPolation
    private void Polation()
    {   
        float latency = (toTimeStamp - previousTimeStamp) / 1000f;
        float lerpAmount = Mathf.Clamp01(latency / timeToReachTarget);
        Vector3 fromPosition = sphere.transform.position;

        //플레이어가 움직였는지 확인
        isMove = IsMoving(fromPosition, toPosition);

        if(isMove)
        {     
            animator.SetBool("isMove", true);
        }
        else
        {
            animator.SetBool("isMove", false);
        }

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
    public void Initialize(bool _isMe, string _nickName, Vector3 position, float rotation)
    {
        isMe = _isMe;
        this.nickname = _nickName;

        miniMapComponent.Init(sled.gameObject);

        nameObject = Instantiate(nameObject, Vector3.zero, Quaternion.identity, sledModel);
        nameObject.GetComponent<TMP_Text>().text = this.nickname;
        nameObject.transform.position = GetNameUIPos();

        sled.transform.position = position;
        sphere.transform.position = position;
        sled.transform.rotation = Quaternion.Euler(0, rotation, 0);

        curCheckpoint = this.transform;
        nextCheckpoint = this.transform;

        previousPosition = GetPosition();
        previousTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        toPosition = GetPosition();
        toVelocity = GetVelocity();
        toRotationY = GetRotationY();
        toTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if(isMe && SettingManager.backgroundPostProcessing == false)
        {
            GameObject.FindWithTag("MainPostProcessing").SetActive(false);
        }

        if(isMe && SettingManager.speedPostProcessing == false)
        {
            gameObject.GetComponentInChildren<PostProcessVolume>().enabled = false;
        }
            
        RankManager.instance.AddOrGetRankInfo(GetComponent<Player>());
        List<RankInfo> ranking = RankManager.instance.GetRanking();
        InGameUI.instance.UpdateRankUI(ranking);

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
        {
            isMove = true;
            animator.SetBool("isMove",true);
            animator.speed = 0.5f + NormalizedForwardSpeed;
        }
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
        return new SyncMessage(ServerManager.instance.roomData.roomID, nickname, GetPosition(), GetVelocity(), GetRotationY(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
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
    
    public float UpdateDistanceToNextCheckpoint()
    {
        return Vector3.Distance(sphere.transform.position, nextCheckpoint.position);
    }
    // 앞으로 나아가는 차량 속도의 양
    public float ForwardSpeed => Vector3.Dot(sphere.velocity, sled.forward);

    // 차량의 최대 속도에 상대적인 전진 속도를 반환 
    // 반환되는 값은 [-1, 1] 범위
    public float NormalizedForwardSpeed
    {
        get
        {
            float speed = Mathf.Abs(ForwardSpeed * 3.6f);
            float normalizedSpeed = (speed > 30f) ? Mathf.Clamp01(speed / (maxSpeed * 3.6f)) : 0.0f;
            return normalizedSpeed * 2;
        }
    }
    public void BlurEffect()
    {
        float checkSpeed = GetSpeed();
        
        if(checkSpeed > 60)
            RadialBlur.instance.blurStrength = Mathf.Lerp(RadialBlur.instance.blurStrength,2f * NormalizedForwardSpeed,Time.fixedDeltaTime);
        else
            RadialBlur.instance.blurStrength = Mathf.Lerp(RadialBlur.instance.blurStrength,0f,Time.fixedDeltaTime);
    }
    public void Respawn()
    {
        sphere.velocity = Vector3.zero;
        sphere.angularVelocity = Vector3.zero;
        sphere.transform.position = curCheckpoint.position;

        // 바라볼 방향 구하기
        Vector3 direction = nextCheckpoint.localPosition - curCheckpoint.localPosition;
        Quaternion rot = Quaternion.LookRotation(direction.normalized);
        rot.x = 0;
        rot.z = 0;
        sled.transform.rotation = rot;
    }
#endregion
}