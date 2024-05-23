using UnityEngine;
using TMPro;
using Cinemachine;
using System;
using System.Collections;
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
    // 현재 프레임에서 플레이어가 지면에 닿아 있는지 여부
    private bool isGround;
    // 이전 프레임에서 플레이어가 지면에 닿아 있었는지 여부
    private bool wasGround;

    // Polation
    private bool onRamp = false;
    private float timeToReachTarget = 1f;
    private float squareMovementThreshold = 3.5f;
    private float extrapolationWeight = 2f;
    private Vector3 previousPosition;
    private long previousTimeStamp;
    private Vector3 toPosition;
    private Vector3 toVelocity;
    private Quaternion toRotation;
    private long toTimeStamp;

    // 최대속도 제한, 드리프트
    private float maxSpeed = 50f;
    private float speed;
    private float currentSpeed;
    private float rotate;

    private float currentRotate;
    private Animator animator;
    public List<string> playerList = new List<string>();
    [SerializeField]
    private Material[] opaqueMaterials;
    [SerializeField]
    private Material[] fadeMaterials;
    private IEnumerator respawnCoroutine;
    private bool change = false;
    private int alphaCount = 0;
    [SerializeField]
    private ParticleSystem landingEffect;

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
    public GameObject playerRightArm;
    public GameObject playerLeftArm;
    public GameObject playerHead;

    [Header("Parameters")]
    public float acceleration = 50f;
    public float steering = 40f;
    public float gravity = 1f;
    public float amount;

    [field: SerializeField] public Vector3 moveVector { get; private set; }
    [field: SerializeField] public bool isMove { get; private set; }
    public MiniMapComponent miniMapComponent;
    public int myRank = 1;
    public int playerIndex;
#endregion


#region PrivateMethod
    private void Awake()
    {
        animator = playerModel.GetComponent<Animator>();
        respawnCoroutine = RespawnEffect();
    }
    private void Update()
    {
        SledPosition();
        SteerHandle();
        GetVerticalSpeed();
        CuerrentValue();
                
        if (isMe)
            BlurEffect();
        if (WorldManager.instance.isGameStart && !isMe)
            Polation();

        if (alphaCount >= 3) {
            StopCoroutine(respawnCoroutine);
            alphaCount = 0;
            ChangeMaterial("Opaque");
        }

        CheckLanding();
    }

    private void FixedUpdate()
    {
        RaycastHit hitData = AdJustBottom();

        if (isMove && isMe)
            ApplyPhysics(hitData);

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

    private void ApplyPhysics(RaycastHit hitNear)
    {

        if (hitNear.collider != null)// If the sled is on the ground
        {
            float weight = (myRank - 1) * 3f; // 등수에 따른 속도 가중치
            sphere.AddForce(sledModel.forward * (currentSpeed + weight), ForceMode.Acceleration);
            sled.eulerAngles = Vector3.Lerp(sled.eulerAngles, new Vector3(0, sled.eulerAngles.y + currentRotate, 0), Time.fixedDeltaTime * 3f);

            sphere.drag = 1;
            if (hitNear.collider.gameObject.tag == "Ramp")
            {
                onRamp = true;
                sphere.AddForce(sledModel.forward * currentSpeed, ForceMode.Acceleration);
            }
        }
        else{// If the sled is in the air
            if(onRamp)
            {
                // Ramp를 벗어날 때의 방향과 속도를 기반으로 힘을 가함
                Vector3 launchDirection = sledModel.forward * 20f + sledModel.up * 26f;
                
                sphere.AddForce(sledModel.forward * sphere.velocity.magnitude * 20f, ForceMode.Acceleration);
                sphere.AddForce(launchDirection.normalized * sphere.velocity.magnitude * 45f, ForceMode.Impulse);
                sphere.drag = 0;
                StartCoroutine(MaintainVelocityAfterJump());
                onRamp = false;
            }
        }
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);//중력 작용

        isMove = false;
        isDrifting = false;
        animator.SetBool("isMove", false);
    }

    private IEnumerator MaintainVelocityAfterJump()
    {
        float duration = 0.6f; // 속도를 유지할 시간
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            sphere.AddForce(sledModel.forward * sphere.velocity.magnitude * 0.1f, ForceMode.Acceleration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
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
        
        isGround = hitNear.collider != null;

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

    private void CheckLanding()
    {
        // 착지 이펙트 재생
        if (!wasGround && isGround)
        {
            landingEffect.Play();
        }
        wasGround = isGround;
    }

    private void SetCamera()
    {
        CinemachineCore.Instance.GetVirtualCamera(0).Follow = sled.transform;
    }

    private bool IsMoving(Vector3 fromPosition, Vector3 toPosition)
    {   
        // 일정 거리 이상 움직였는지 여부를 판단한다.
        float movementThreshold = 1f; // 움직임 여부를 판단하는 임계값
        return Vector3.Distance(fromPosition, toPosition) > movementThreshold;
    }

    // ExtraPolation, InterPolation
    private void Polation()
    {   
        float latency = (toTimeStamp - previousTimeStamp) / 1000f;
        float lerpAmount = Mathf.Clamp01(latency / timeToReachTarget);
        Vector3 fromPosition = sphere.transform.position;
        float squareMagnitude = (toPosition - previousPosition).sqrMagnitude;

        //플레이어가 움직였는지 확인
        isMove = IsMoving(fromPosition, toPosition);

        if (isMove)
        {     
            animator.SetBool("isMove", true);
            animator.speed = 0.5f + Mathf.Clamp01(squareMagnitude / squareMovementThreshold);
        }
        else
            animator.SetBool("isMove", false);

        if (toPosition != fromPosition)
        {
            Vector3 extrapolatedPosition = toPosition + (toVelocity * Mathf.Clamp01(latency) * extrapolationWeight);
            sphere.transform.position = Vector3.Slerp(fromPosition, extrapolatedPosition, lerpAmount);
        }

        // Interpolation Rotation
        sled.transform.rotation = Quaternion.Slerp(sled.transform.rotation, toRotation, lerpAmount);
    }

    private void ChangeMaterial(string materialName)
    {
        string myPlayerNickname = ServerManager.instance.myNickname;
        if (materialName == "Opaque")
        {
            playerModel.transform.Find("Character_Male").GetComponent<Renderer>().material = opaqueMaterials[playerIndex];
            sledModel.GetComponent<Renderer>().material = opaqueMaterials[playerIndex];
            playerLeftArm.GetComponent<Renderer>().material = opaqueMaterials[playerIndex];
            playerRightArm.GetComponent<Renderer>().material = opaqueMaterials[playerIndex];
            playerHead.GetComponent<Renderer>().material = opaqueMaterials[playerIndex];
        }
        else if (materialName == "Fade")
        {
            playerModel.transform.Find("Character_Male").GetComponent<Renderer>().material = fadeMaterials[playerIndex];
            sledModel.GetComponent<Renderer>().material = fadeMaterials[playerIndex];
            playerLeftArm.GetComponent<Renderer>().material = fadeMaterials[playerIndex];
            playerRightArm.GetComponent<Renderer>().material = fadeMaterials[playerIndex];
            playerHead.GetComponent<Renderer>().material = fadeMaterials[playerIndex];
        }
    }
    private IEnumerator RespawnEffect()
    {
        GameObject myModel = gameObject.transform.Find("Sled").Find("Character").Find("Character_Male").gameObject;
        GameObject mySled = sledModel.gameObject;
        
        while(true)
        {
            if (!change)
            {
                change = true;
                myModel.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.65f);
                mySled.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.65f);
                playerLeftArm.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.65f);
                playerRightArm.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.65f);
                playerHead.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.65f);
            }
            else
            {
                change = false;
                myModel.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                mySled.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                playerLeftArm.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                playerRightArm.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                playerHead.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                alphaCount++;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
#endregion


#region PublicMethod
    // 내 플레이어와 다른 플레이어 객체 초기화
    public void Initialize(bool _isMe, string _nickName, Vector3 position, float rotation)
    {
        isMe = _isMe;
        this.nickname = _nickName;

        miniMapComponent.Init(sled.gameObject);

        sled.transform.position = position;
        sphere.transform.position = position;
        sled.transform.rotation = Quaternion.Euler(0, rotation, 0);

        curCheckpoint = this.transform;
        nextCheckpoint = this.transform;

        previousPosition = GetPosition();
        previousTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        toPosition = GetPosition();
        toVelocity = GetVelocity();
        toRotation = GetRotation();
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

    public void SetSyncData(Vector3 _position, Vector3 _velocity, Quaternion _rotation, long _timeStamp)
    {
        previousPosition = toPosition;
        previousTimeStamp = toTimeStamp;

        toPosition = _position;
        toVelocity = _velocity;
        toRotation = _rotation;
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
    public Quaternion GetRotation()
    {
        return sled.transform.rotation;
    }
    public SyncMessage GetSyncData()
    {
        return new SyncMessage(ServerManager.instance.roomData.roomID, nickname, GetPosition(), GetVelocity(), GetRotation(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
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
    
    public float GetDistanceToNextCheckpoint()
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
        
        if(checkSpeed > 80)
        {
            if(RadialBlur.instance.blurStrength > RadialBlur.instance.maxBlurStrength)
                RadialBlur.instance.blurStrength = RadialBlur.instance.maxBlurStrength;
            
            RadialBlur.instance.blurStrength = Mathf.Lerp(RadialBlur.instance.blurStrength, 1.1f * NormalizedForwardSpeed,Time.fixedDeltaTime);
        }
        else
            RadialBlur.instance.blurStrength = Mathf.Lerp(RadialBlur.instance.blurStrength,0f,Time.fixedDeltaTime);
    }
    
    public void Respawn()
    {
        if (isMe)
            GameManager.Instance().soundManager.Play("Effect/Reset", SoundType.EFFECT, 1.0f, 0.4f);
        sphere.velocity = Vector3.zero;
        sphere.angularVelocity = Vector3.zero;
        sphere.transform.position = curCheckpoint.position;

        // 바라볼 방향 구하기
        Vector3 direction = nextCheckpoint.localPosition - curCheckpoint.localPosition;
        Quaternion rot = Quaternion.LookRotation(direction.normalized);
        rot.x = 0;
        rot.z = 0;
        sled.transform.rotation = rot;

        ChangeMaterial("Fade");
        StartCoroutine(respawnCoroutine);
    }
#endregion
}