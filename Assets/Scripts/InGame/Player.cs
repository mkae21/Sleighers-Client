using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
#region PrivateVariables
    private int index = 0;
    private bool isMe = false;
    private float rotSpeed = 4.0f;
    private float moveSpeed = 4.0f;
    private string nickName = string.Empty;
    private GameObject playerModelObject;
    private Transform cameraTransform;
    private Animator anim;
    private float smoothRotationTime;   //target 각도로 회전하는데 걸리는 시간
    private float smoothMoveTime;   //target 속도로 바뀌는데 걸리는 시간
    private float rotationVelocity;
#endregion

#region PublicVariables
    [field: SerializeField] public Vector3 moveVector { get; private set; }
    [field: SerializeField] public bool isMove { get; private set; }
    [field: SerializeField] public bool isRotate { get; private set; }
    public GameObject nameObject;
    public GameObject joystick;
#endregion


#region PrivateMethod
    private void Awake()
    {
        anim = GetComponent<Animator>();
        cameraTransform = Camera.main.transform;
    }
    private void Start()
    {
        // 서버 인스턴스가 없으면 인게임 테스트용으로 초기화
        if (ServerManager.Instance() == null)
            Initialize(true, 0, "TestPlayer", 0);
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
        if (isMove)
        {
            Move();
            anim.SetBool("IsWalk", true);
        }
        else
        {
            anim.SetBool("IsWalk", false);
        }
        if (isRotate)
        {
            Rotate();
        }
    }
    private Vector3 GetNameUIPos()
    {
        return this.transform.position + (Vector3.up * 2.0f);
    }
#endregion


#region PublicMethod
    public void Initialize(bool isMe, int index, string nickName, float rot)
    {
        this.isMe = isMe;
        this.index = index;
        this.nickName = nickName;

        playerModelObject = this.gameObject;
        playerModelObject.transform.rotation = Quaternion.Euler(0, rot, 0);

        nameObject = Instantiate(nameObject, Vector3.zero, Quaternion.identity, playerModelObject.transform);
        nameObject.GetComponent<TMP_Text>().text = this.nickName;
        nameObject.transform.position = GetNameUIPos();

        if (this.isMe)
        {
            // Camera.main.GetComponentInParent<MainCamera>().target = this.transform;
        }

        this.isMove = false;
        this.moveVector = new Vector3(0, 0, 0);
        this.isRotate = false;
    }
#region Movement
    public void SetMoveVector(float move)
    {
        SetMoveVector(this.transform.forward * move);
    }
    public void SetMoveVector(Vector3 vector)
    {
        moveVector = vector;

        if (vector == Vector3.zero)
        {
            isMove = false;
        }
        else
        {
            isMove = true;
        }
    }

    public void Move()
    {
        Move(moveVector);
    }
    public void Move(Vector3 var)
    {
        // 회전
        if (var.Equals(Vector3.zero))
        {
            isRotate = false;
        }
        else
        {
            if (Quaternion.Angle(playerModelObject.transform.rotation, Quaternion.LookRotation(var)) > Quaternion.kEpsilon)
            {
                isRotate = true;
            }
            else
            {
                isRotate = false;
            }
        }

        // 움직임을 멈췄을 때 다시 처음 각도로 돌아가는 걸 막기 위함
        if (var != Vector3.zero)
        {
            float rotation = Mathf.Atan2(var.x, var.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, rotation, ref rotationVelocity, smoothRotationTime);
        }
        // 이동
        var pos = gameObject.transform.position + playerModelObject.transform.forward * moveSpeed * Time.deltaTime;
        SetPosition(pos);
    }

    public void Rotate()
    {
        if (moveVector.Equals(Vector3.zero))
        {
            isRotate = false;
            return;
        }
        if (Quaternion.Angle(playerModelObject.transform.rotation, Quaternion.LookRotation(moveVector)) < Quaternion.kEpsilon)
        {
            isRotate = false;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(moveVector, Vector3.up);
        
        playerModelObject.transform.rotation = Quaternion.Slerp(playerModelObject.transform.rotation, targetRotation, Time.deltaTime * rotSpeed);
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
#endregion

#endregion

}
