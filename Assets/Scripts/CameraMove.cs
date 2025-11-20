using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public float moveSpeed = 5f;


    void Update()
    {
        Vector3 dir = transform.forward;
        dir.y = 0f;              
        dir.Normalize(); 

        transform.position += dir * moveSpeed * Time.deltaTime;
    }
}
