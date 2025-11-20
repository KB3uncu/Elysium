using System;
using UnityEngine;


public class EngelMove : MonoBehaviour
{
    public float moveSpeed = 5f;          
    public Vector3 moveDirection = Vector3.forward;

    void Start()
    {
        Destroy(gameObject, 2.5f);
    }
    void Update()
    {
        
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }

    
}
