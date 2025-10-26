using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider frontLeftWheel, frontRightWheel;
    public WheelCollider rearLeftWheel, rearRightWheel;
    public Transform frontLeftMesh, frontRightMesh, rearLeftMesh, rearRightMesh;

    public float maxMotorTorque = 400f;
    public float maxSteerAngle = 30f;
    public float brakeForce = 1500f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0.3f);
    }

    void FixedUpdate()
    {
        float currentSpeed = rb.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(1 - (currentSpeed / 60f)); // 60 m/s = ~216 km/h
        float motor = Input.GetAxis("Vertical") * maxMotorTorque * speedFactor;
        float steering = Input.GetAxis("Horizontal") * maxSteerAngle;

        // Direksiyon
        frontLeftWheel.steerAngle = steering;
        frontRightWheel.steerAngle = steering;

        // Çekiþ (örnek: arkadan itiþli)
        rearLeftWheel.motorTorque = motor;
        rearRightWheel.motorTorque = motor;

        // Fren
        if (Input.GetKey(KeyCode.Space))
        {
            rearLeftWheel.brakeTorque = brakeForce;
            rearRightWheel.brakeTorque = brakeForce;
        }
        else
        {
            rearLeftWheel.brakeTorque = 0;
            rearRightWheel.brakeTorque = 0;
        }

        UpdateWheelPoses();
    }

    void UpdateWheelPoses()
    {
        UpdateWheelPose(frontLeftWheel, frontLeftMesh);
        UpdateWheelPose(frontRightWheel, frontRightMesh);
        UpdateWheelPose(rearLeftWheel, rearLeftMesh);
        UpdateWheelPose(rearRightWheel, rearRightMesh);
    }

    void UpdateWheelPose(WheelCollider col, Transform trans)
    {
        Vector3 pos;
        Quaternion quat;
        col.GetWorldPose(out pos, out quat);
        trans.position = pos;
        trans.rotation = quat;
    }
}
