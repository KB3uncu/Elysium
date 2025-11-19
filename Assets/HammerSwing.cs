using UnityEngine;

public class HammerSwing : MonoBehaviour
{
    public Transform cameraTransform; 
    public Vector3 offsetFromCamera = new Vector3(0f, -1f, 1f); 

    public float minAngle = 0f;      
    public float maxAngle = 80f;     
    public float swingSpeed = 2f;   

    void Update()
    {

        if (cameraTransform != null)
        {
            transform.position = cameraTransform.position +
                                 cameraTransform.rotation * offsetFromCamera;
        }


        float t = (Mathf.Sin(Time.time * swingSpeed) + 1f) / 2f;


        float currentAngle = Mathf.Lerp(minAngle, maxAngle, t);


        transform.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
    }
}
