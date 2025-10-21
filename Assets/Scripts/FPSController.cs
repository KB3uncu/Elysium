using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Oyuncu kamerasý (FPS). Pitch bu objede döner).")]
    public Transform cameraTransform;

    private CharacterController controller;

    [Header("Movement")]
    [Tooltip("Yürüme hýzý (m/sn).")]
    public float moveSpeed = 4.5f;
    [Tooltip("Ývme (daha yumuþak baþla/dur).")]
    public float acceleration = 12f;
    [Tooltip("Havada yatay kontrol katsayýsý.")]
    public float airControl = 0.35f;

    [Header("Jump & Gravity")]
    [Tooltip("Zýplama yüksekliði (metre).")]
    public float jumpHeight = 1.2f;
    [Tooltip("Yerçekimi (negatif). Örn: -20 ~ -30 arasý iyidir.")]
    public float gravity = -25f;
    [Tooltip("Yere yapýþmayý kolaylaþtýrýr (küçük negatif deðer).")]
    public float groundedStickForce = -2f;

    [Header("Ground Check")]
    [Tooltip("Ayak hizasýnda yer kontrolü için nokta.")]
    public Transform groundCheck;
    [Tooltip("Yer kontrol yarýçapý.")]
    public float groundRadius = 0.25f;
    [Tooltip("Zemin Layer(lar)ý.")]
    public LayerMask groundMask;

    [Header("Mouse Look")]
    [Tooltip("Yatay (yaw) hassasiyet.")]
    public float mouseSensitivityX = 180f;
    [Tooltip("Dikey (pitch) hassasiyet.")]
    public float mouseSensitivityY = 180f;
    [Tooltip("Dikey bakýþ sýnýrý.")]
    public float minPitch = -85f, maxPitch = 85f;


    private Vector3 velocity;
    private Vector3 planarVelocity;
    private float targetSpeed;
    private float pitch;
    private bool isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }

        // Ýmleci kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        MoveCharacter();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

        // Yaw: oyuncu gövdesini Y ekseninde döndür
        transform.Rotate(Vector3.up * mouseX);

        // Pitch: sadece kamerayý dikey eksende döndür
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleGroundCheck()
    {
        if (groundCheck == null)
        {

            Vector3 feet = transform.position + Vector3.down * (controller.height * 0.5f - controller.radius + 0.05f);
            isGrounded = Physics.CheckSphere(feet, groundRadius, groundMask, QueryTriggerInteraction.Ignore);
        }
        else
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask, QueryTriggerInteraction.Ignore);
        }
    }

    void HandleMovement()
    {

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");


        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 wishDir = (forward * inputZ + right * inputX).normalized;


        targetSpeed = wishDir.magnitude * moveSpeed;


        float usedAccel = isGrounded ? acceleration : acceleration * airControl;
        Vector3 currentPlanar = new Vector3(planarVelocity.x, 0f, planarVelocity.z);
        Vector3 targetPlanar = wishDir * targetSpeed;


        currentPlanar = Vector3.MoveTowards(currentPlanar, targetPlanar, usedAccel * Time.deltaTime);
        planarVelocity = new Vector3(currentPlanar.x, planarVelocity.y, currentPlanar.z);
    }

    void HandleJump()
    {
        if (isGrounded && velocity.y < 0f)
        {

            velocity.y = groundedStickForce;
        }

        if (isGrounded && Input.GetButtonDown("Jump"))
        {

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
    }

    void MoveCharacter()
    {

        velocity.x = planarVelocity.x;
        velocity.z = planarVelocity.z;


        controller.Move(velocity * Time.deltaTime);


        if (isGrounded && new Vector2(planarVelocity.x, planarVelocity.z).sqrMagnitude < 0.0004f)
        {
            planarVelocity.x = 0f;
            planarVelocity.z = 0f;
        }
    }


    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
        else if (TryGetComponent(out CharacterController cc))
        {
            Vector3 feet = transform.position + Vector3.down * (cc.height * 0.5f - cc.radius + 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(feet, groundRadius);
        }
    }
}
