using UnityEngine;


public class FareMovoment : MonoBehaviour
{
    public float moveSpeed = 5f;   
    //public float limitX = 3f; 

    void Update()
    {

        float horizontal = Input.GetAxis("Horizontal");


        Vector3 movement = new Vector3(0, 0, -horizontal) * moveSpeed * Time.deltaTime;


        transform.position += movement;

       // Vector3 pos = transform.position;
        //pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        //transform.position = pos;
    }
}