using UnityEngine;

public class HammerSwing : MonoBehaviour
{
    public float minAngle = 0f;      
    public float maxAngle = -80f;     
    public float swingSpeed = 2f;   
    public float start = 0f;

    private void Start()
    {
        
    }
    void Update()
    {


        float t = (Mathf.Sin(Time.time * swingSpeed) + 1f) / 2f;


        float currentAngle = Mathf.Lerp(minAngle, maxAngle, t);


        transform.localRotation = Quaternion.Euler(currentAngle, 90f, 0f);
    }
}
