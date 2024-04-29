using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Moving : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    
    void Move()
    {
        // 전진 및 후진
        float moveInput = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        transform.Translate(0f, 0f, moveInput);

        // 좌우 회전
        float rotationInput = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, rotationInput, 0f);
        
    }
}
