using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FareMovoment : MonoBehaviour
{
    [Header("Sað/Sol")]
    public float moveSpeed = 5f;
    public float smooth = 12f;
    public float halfWidth = 2.8f;

    [Header("Ýleri Akýþ")]
    public float forwardSpeed = 0.6f;

    private Rigidbody rb;
    private float input;

    private float startZ;
    private float targetZ;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints |= RigidbodyConstraints.FreezeRotation;

        startZ = rb.position.z;
        targetZ = startZ;
    }

    void Update()
    {
        input = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        targetZ += (-input) * moveSpeed * Time.fixedDeltaTime;
        targetZ = Mathf.Clamp(targetZ, startZ - halfWidth, startZ + halfWidth);

        Vector3 pos = rb.position;
        float newZ = Mathf.Lerp(pos.z, targetZ, 1f - Mathf.Exp(-smooth * Time.fixedDeltaTime));

        float newX = pos.x + forwardSpeed * Time.fixedDeltaTime;

        rb.MovePosition(new Vector3(newX, pos.y, newZ));
    }
}
