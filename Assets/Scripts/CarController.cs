using UnityEngine;

public class CarController : MonoBehaviour
{
    public WheelCollider frontLeftWheel, frontRightWheel;
    public WheelCollider rearLeftWheel, rearRightWheel;
    public ParticleSystem smokeLeft, smokeRight;
    public TrailRenderer trailLeft, trailRight;
    public Transform frontLeftMesh, frontRightMesh, rearLeftMesh, rearRightMesh;

    public float maxMotorTorque = 400f;
    public float maxSteerAngle = 30f;
    public float brakeForce = 1500f;

    public float driftSlipLimit = 0.4f;
    public float brakeSlipBoost = 0.3f; 

    private bool isBraking = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.6f, 0.3f);


        if (trailLeft != null) trailLeft.emitting = false;
        if (trailRight != null) trailRight.emitting = false;
    }

    void FixedUpdate()
    {
        float currentSpeed = rb.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(1 - (currentSpeed / 60f));
        float motor = Input.GetAxis("Vertical") * maxMotorTorque * speedFactor;
        float steering = Input.GetAxis("Horizontal") * maxSteerAngle;


        frontLeftWheel.steerAngle = steering;
        frontRightWheel.steerAngle = steering;


        rearLeftWheel.motorTorque = motor;
        rearRightWheel.motorTorque = motor;
        frontLeftWheel.motorTorque = motor * 0.3f;
        frontRightWheel.motorTorque = motor * 0.3f;

        float horizontal = Input.GetAxis("Horizontal");
        float speed = rb.velocity.magnitude;

        WheelFrictionCurve sideFriction = rearLeftWheel.sidewaysFriction;
        float driftFactor = Mathf.Lerp(1.2f, 0.8f, Mathf.Abs(horizontal) * (speed / 40f));
        sideFriction.stiffness = driftFactor;
        rearLeftWheel.sidewaysFriction = sideFriction;
        rearRightWheel.sidewaysFriction = sideFriction;


        CheckDrift(rearLeftWheel, smokeLeft, trailLeft);
        CheckDrift(rearRightWheel, smokeRight, trailRight);

        if (Input.GetKey(KeyCode.Space))
        {
            isBraking = true;
            rearLeftWheel.brakeTorque = brakeForce;
            rearRightWheel.brakeTorque = brakeForce;
        }
        else
        {
            isBraking = false;
            rearLeftWheel.brakeTorque = 0;
            rearRightWheel.brakeTorque = 0;
        }

        UpdateWheelPoses();
    }

    void CheckDrift(WheelCollider wheel, ParticleSystem smoke, TrailRenderer trail)
    {
        WheelHit hit;
        if (wheel.GetGroundHit(out hit))
        {
            float slip = Mathf.Abs(hit.sidewaysSlip);
            bool shouldEmit = slip > driftSlipLimit || isBraking;


            if (shouldEmit)
            {
                if (!smoke.isPlaying) smoke.Play();
            }
            else
            {
                if (smoke.isPlaying) smoke.Stop();
            }


            if (trail != null)
            {
                if (shouldEmit && !trail.emitting)
                    trail.emitting = true;
                else if (!shouldEmit && trail.emitting)
                    trail.emitting = false;
            }
        }
        else
        {
            if (smoke.isPlaying) smoke.Stop();
            if (trail != null && trail.emitting) trail.emitting = false;
        }
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
