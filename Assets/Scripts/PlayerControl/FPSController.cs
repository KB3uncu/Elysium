using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float acceleration = 10f;
    public float gravity = -25f;
    public float jumpHeight = 1.5f;

    [Header("Boost")]
    public float boostAmount = 3f;
    public float maxBoost = 15f;
    public float friction = 2f;

    [Header("Slide")]
    public float slideDuration = 1f;
    public float slideHeight = 0.5f;
    public float crouchHeight = 1f;
    public float minSlideSpeed = 10f;

    [Header("Jump Combo")]
    public float speedJumpBonusHeight = 1.5f;
    public float speedJumpMinSpeed = 8f;

    [Header("Slide Jump")]
    public float slideJumpForwardForce = 10f;
    public float slideJumpHeightMultiplier = 1f;
    public float slideJumpWindow = 0.35f;

    [Header("Camera")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float upDownRange = 80f;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public float bobSpeed = 10f;
    public float bobAmount = 0.05f;
    public float crouchBobAmount = 0.03f;

    [Header("Ceiling Check")]
    public LayerMask ceilingMask = ~0;

    [Header("FOV Settings")]
    public float fovSmoothTime = 10f;
    public float maxFovIncrease = 15f;
    private Camera _cam;
    private float _defaultFOV;

    [Header("Stamina (Boost) System")]
    public int maxStaminaSegments = 3;
    public float staminaRegenTime = 2f;
    private int currentStaminaSegments;
    private float staminaRegenTimer;

    [Header("Stamina UI")]
    public UnityEngine.UI.Image[] staminaBars;
    public Color fullColor = Color.white;
    public Color emptyColor = new Color(1, 1, 1, 0.2f);

    [Header("Visual Effects")]
    public GameObject shockwavePrefab;
    public float shockwaveSpawnDistance = 2f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 moveVelocity;
    private Vector3 dampVelocity;

    private float boost;
    private float verticalRotation;
    private float defaultHeight;
    private Vector3 defaultCamLocalPos;
    private float bobTimer;

    private bool isSliding;
    private bool isCrouching;

    private float slideTimer;
    private Vector3 slideDir;
    private float slideSpeed;
    private float slideJumpTimer;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        defaultHeight = controller.height;

        if (playerCamera != null)
            defaultCamLocalPos = playerCamera.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null) _cam = playerCamera.GetComponent<Camera>();
        if (_cam != null) _defaultFOV = _cam.fieldOfView;

        currentStaminaSegments = maxStaminaSegments;
    }

    void Update()
    {
        Look();
        Movement();
        Stance();
        BoostLogic();
        HandleHeadBob();
        HandleFOV();

        if (slideJumpTimer > 0f)
            slideJumpTimer -= Time.deltaTime;
    }

    void Look()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);

        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void Movement()
    {
        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        if (!isSliding)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            float baseSpeed = isCrouching ? crouchSpeed : walkSpeed;
            float totalSpeed = baseSpeed + boost;

            Vector3 dir = (transform.right * x + transform.forward * z).normalized;
            Vector3 target = dir * totalSpeed;

            moveVelocity = Vector3.SmoothDamp(moveVelocity, target, ref dampVelocity, 1f / acceleration);
        }

        if (Input.GetButtonDown("Jump") && grounded)
            TryJump();

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = moveVelocity;
        finalMove.y = velocity.y;

        controller.Move(finalMove * Time.deltaTime);
    }

    void TryJump()
    {
        if (isCrouching && !isSliding) return;

        float currentJumpHeight = jumpHeight;
        float horizontalSpeed = new Vector3(moveVelocity.x, 0f, moveVelocity.z).magnitude;

        if (slideJumpTimer > 0f)
        {
            if (CanStandUp()) SetStand(); else SetCrouch();

            velocity.y = Mathf.Sqrt((jumpHeight * slideJumpHeightMultiplier) * -2f * gravity);
            moveVelocity += transform.forward * slideJumpForwardForce;
            slideJumpTimer = 0f;
            return;
        }

        if (horizontalSpeed >= speedJumpMinSpeed)
            currentJumpHeight += speedJumpBonusHeight;

        velocity.y = Mathf.Sqrt(currentJumpHeight * -2f * gravity);

        if (isSliding) EndSlide();
    }

    void BoostLogic()
    {
        if (currentStaminaSegments < maxStaminaSegments)
        {
            staminaRegenTimer += Time.deltaTime;
            if (staminaRegenTimer >= staminaRegenTime)
            {
                currentStaminaSegments++;
                UpdateStaminaUI();
                staminaRegenTimer = 0f;
                Debug.Log("Stamina Doldu: " + currentStaminaSegments);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && controller.isGrounded && !isSliding && !isCrouching)
        {
            if (currentStaminaSegments > 0)
            {
                boost += boostAmount;
                boost = Mathf.Clamp(boost, 0f, maxBoost);

                currentStaminaSegments--;
                UpdateStaminaUI();
                staminaRegenTimer = 0f;
                Debug.Log("Boost Kullanýldý! Kalan Stamina: " + currentStaminaSegments);
                if (shockwavePrefab != null && playerCamera != null)
                {
                    SpawnShockwave();
                }
            }
            else
            {
                Debug.Log("Stamina Yetersiz");
            }
        }

        if (boost > 0f && !isSliding)
        {
            boost -= friction * Time.deltaTime;
            if (boost < 0f) boost = 0f;
        }
    }

    void Stance()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && controller.isGrounded && !isSliding)
        {
            float speed = new Vector3(moveVelocity.x, 0f, moveVelocity.z).magnitude;
            if (speed > walkSpeed + 1f) StartSlide();
            else StartCrouch();
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            float decay = slideTimer / slideDuration;
            moveVelocity = slideDir * (slideSpeed * decay);

            if (slideTimer <= 0f || moveVelocity.magnitude < crouchSpeed)
                EndSlide();

            return;
        }

        if (isCrouching && !Input.GetKey(KeyCode.LeftControl))
        {
            if (CanStandUp()) StopCrouch();
        }
    }
    void StartCrouch()
    {
        isCrouching = true;
        isSliding = false;
        controller.height = crouchHeight;
    }

    void StopCrouch()
    {
        isCrouching = false;
        controller.height = defaultHeight;
    }

    void StartSlide()
    {
        isSliding = true;
        isCrouching = false;
        slideSpeed = Mathf.Max(moveVelocity.magnitude, minSlideSpeed);
        slideDir = moveVelocity.sqrMagnitude > 0.01f ? moveVelocity.normalized : transform.forward;
        slideTimer = slideDuration;
        controller.height = slideHeight;
        boost = 0f;
    }

    void EndSlide()
    {
        isSliding = false;
        slideJumpTimer = slideJumpWindow;

        if (CanStandUp()) SetStand();
        else SetCrouch();
    }

    void SetStand() { isCrouching = false; isSliding = false; controller.height = defaultHeight; }
    void SetCrouch() { isCrouching = true; isSliding = false; controller.height = crouchHeight; }

    bool CanStandUp()
    {
        float radius = controller.radius * 0.8f;
        Vector3 start = transform.position + Vector3.up * (controller.height / 2f);
        float distance = defaultHeight - (controller.height / 2f);

        return !Physics.SphereCast(start, radius, Vector3.up, out _, distance, ceilingMask);
    }
    
    void HandleHeadBob()
    {
        if (!enableHeadBob || playerCamera == null) return;

        Vector3 horizontalVel = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        bool moving = horizontalVel.magnitude > 0.1f && controller.isGrounded;

        if (moving && !isSliding)
        {
            bobTimer += Time.deltaTime * bobSpeed * (isCrouching ? 0.7f : 1f);
            float currentBobAmount = isCrouching ? crouchBobAmount : bobAmount;
            float bobOffsetY = Mathf.Sin(bobTimer) * currentBobAmount;

            Vector3 targetPos = defaultCamLocalPos + new Vector3(0f, bobOffsetY, 0f);
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetPos, Time.deltaTime * 10f);
        }
        else
        {
            bobTimer = 0f;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, defaultCamLocalPos, Time.deltaTime * 8f);
        }
    }

    void HandleFOV()
    {
        if (_cam == null) return;

        float targetFOV = _defaultFOV;
        float horizontalSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

        if (isSliding)
        {
            targetFOV += maxFovIncrease * 0.8f;
        }
        else if (boost > 0.1f && horizontalSpeed > walkSpeed)
        {
            float boostFactor = boost / maxBoost;
            targetFOV += boostFactor * maxFovIncrease;
        }

        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothTime);
    }

    void UpdateStaminaUI()
    {
        for (int i = 0; i < staminaBars.Length; i++)
        {
            if (i < currentStaminaSegments)
            {
                staminaBars[i].color = fullColor;
                staminaBars[i].enabled = true;
            }
            else
            {
                staminaBars[i].color = emptyColor;   // staminaBars[i].enabled = false;

            }
        }
    }

    void SpawnShockwave()
    {
        Vector3 spawnPos = playerCamera.position + (playerCamera.forward * shockwaveSpawnDistance);
        GameObject wave = Instantiate(shockwavePrefab, spawnPos, Quaternion.identity, playerCamera);

        wave.transform.LookAt(playerCamera);
    }
}