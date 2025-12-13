using UnityEngine;


public class FareMovoment : MonoBehaviour
{
    public float moveSpeed = 5f;   



    void Update()
    {

        float horizontal = Input.GetAxis("Horizontal");


        Vector3 movement = new Vector3(0, 0, -horizontal) * moveSpeed * Time.deltaTime;


        transform.position += movement;


    }
}