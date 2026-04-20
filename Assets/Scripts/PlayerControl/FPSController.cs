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

    [Header("Balance Debug")]
    public bool drawBalanceDebug = true;

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

    // BALANCE
    private bool isInBalanceMode;
    private BalanceBeam currentBalanceBeam;
    private Vector3 activeBeamMoveForward;
    private float currentCameraRoll;
    private float targetCameraRoll;
    private float swayTimer;

    private int balanceStep;
    private bool lastBalanceInputConsumed;

    // NARROW PASSAGE
    private bool isInNarrowPassageMode;
    private NarrowPassageZone currentNarrowPassageZone;
    private Vector3 activePassageMoveForward;
    private float normalControllerRadius;

    // LADDER
    private bool isOnLadder;
    private LadderZone currentLadderZone;
    private float ladderBobTimer;


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

        normalControllerRadius = controller.radius;
    }

    void Update()
    {
        Look();
        Movement();
        Stance();
        BoostLogic();
        HandleHeadBob();
        HandleFOV();
        UpdateBalanceVisuals();

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
            playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, currentCameraRoll);
    }

    void Movement()
    {
        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        if (isOnLadder && currentLadderZone != null)
        {
            HandleLadderMovement();
        }
        else if (isInBalanceMode && currentBalanceBeam != null)
        {
            HandleBalanceMovement();
        }
        else if (isInNarrowPassageMode && currentNarrowPassageZone != null)
        {
            HandleNarrowPassageMovement();
        }
        else
        {
            HandleNormalMovement();
        }

        if (isOnLadder && Input.GetButtonDown("Jump"))
        {
            LadderReleaseJump();
        }
        else if (!isInBalanceMode && !isInNarrowPassageMode && !isOnLadder && Input.GetButtonDown("Jump") && grounded)
        {
            TryJump();
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 finalMove = moveVelocity;
        finalMove.y = velocity.y;

        controller.Move(finalMove * Time.deltaTime);
    }

    void LadderReleaseJump()
    {
        ForceExitLadderMode();

        moveVelocity = Vector3.zero;
        dampVelocity = Vector3.zero;

        velocity.y = Mathf.Sqrt((jumpHeight * 0.7f) * -2f * gravity);
    }


    void HandleLadderMovement()
    {
        if (currentLadderZone == null)
        {
            ForceExitLadderMode();
            return;
        }

        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 climb = currentLadderZone.GetLadderUp() * (verticalInput * currentLadderZone.climbSpeed);

        moveVelocity = Vector3.zero;
        velocity = Vector3.zero;

        controller.Move(climb * Time.deltaTime);
    }

    public void EnterLadderMode(LadderZone zone)
    {
        if (zone == null)
            return;

        if (isInBalanceMode)
            return;

        currentLadderZone = zone;
        isOnLadder = true;

        moveVelocity = Vector3.zero;
        velocity = Vector3.zero;
        boost = 0f;

        if (isSliding)
            EndSlide();

        if (isCrouching)
            StopCrouch();

        if (isInNarrowPassageMode)
            ForceExitNarrowPassageMode();

        ladderBobTimer = 0f;
    }

    public void ExitLadderMode(LadderZone zone)
    {
        if (zone != null && zone != currentLadderZone)
            return;

        ForceExitLadderMode();
    }

    void ForceExitLadderMode()
    {
        isOnLadder = false;
        currentLadderZone = null;
        ladderBobTimer = 0f;
    }

    void HandleNarrowPassageMovement()
{
    if (currentNarrowPassageZone == null)
    {
        ForceExitNarrowPassageMode();
        return;
    }

    float zInput = Input.GetAxisRaw("Vertical");

    if (!currentNarrowPassageZone.allowBackwardMovement && zInput < 0f)
        zInput = 0f;

    Vector3 target = activePassageMoveForward * (zInput * currentNarrowPassageZone.narrowMoveSpeed);
    moveVelocity = Vector3.SmoothDamp(moveVelocity, target, ref dampVelocity, 1f / acceleration);
}

    void HandleNormalMovement()
    {
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
    }

    void HandleBalanceMovement()
    {
        if (currentBalanceBeam == null)
        {
            ForceExitBalanceMode();
            return;
        }

        float zInput = Input.GetAxisRaw("Vertical");

        if (!currentBalanceBeam.allowBackwardMovement && zInput < 0f)
            zInput = 0f;

        Vector3 target = activeBeamMoveForward * (zInput * currentBalanceBeam.balanceWalkSpeed);
        moveVelocity = Vector3.SmoothDamp(moveVelocity, target, ref dampVelocity, 1f / acceleration);

        UpdateBalanceState();
    }

    void UpdateBalanceState()
    {
        if (!isInBalanceMode || currentBalanceBeam == null)
        {
            ForceExitBalanceMode();
            return;
        }

        swayTimer -= Time.deltaTime;
        if (swayTimer <= 0f)
        {
            ApplyRandomSway();
            ResetSwayTimer();
        }

        HandleBalanceInput();

        int maxStep = Mathf.Max(1, currentBalanceBeam.maxBalanceStep);

        if (balanceStep > maxStep || balanceStep < -maxStep)
        {
            FailBalance();
            return;
        }

        float normalized = (float)balanceStep / maxStep;
        targetCameraRoll = -normalized * currentBalanceBeam.maxCameraRoll;
    }

    void HandleBalanceInput()
    {
        bool pressLeft = Input.GetKeyDown(KeyCode.A);
        bool pressRight = Input.GetKeyDown(KeyCode.D);

        if (pressLeft && !pressRight)
        {
            balanceStep -= 1;
        }
        else if (pressRight && !pressLeft)
        {
            balanceStep += 1;
        }
    }

    void ApplyRandomSway()
    {
        if (currentBalanceBeam == null)
            return;

        bool sudden = Random.value < currentBalanceBeam.suddenSwayChance;

        int stepAmount;
        if (sudden)
        {
            stepAmount = Random.Range(
                currentBalanceBeam.suddenSwayMinStep,
                currentBalanceBeam.suddenSwayMaxStep + 1
            );
        }
        else
        {
            stepAmount = Random.Range(
                currentBalanceBeam.minSwayStep,
                currentBalanceBeam.maxSwayStep + 1
            );
        }

        int direction = Random.value < 0.5f ? -1 : 1;
        balanceStep += direction * stepAmount;
    }

    void ResetSwayTimer()
    {
        if (currentBalanceBeam == null)
            return;

        swayTimer = Random.Range(
            currentBalanceBeam.swayIntervalMin,
            currentBalanceBeam.swayIntervalMax
        );
    }

    void FailBalance()
    {
        if (!isInBalanceMode || currentBalanceBeam == null)
        {
            ForceExitBalanceMode();
            return;
        }

        Vector3 beamRight = currentBalanceBeam.GetBeamRightFromMoveDirection(activeBeamMoveForward);
        Vector3 sideDir = balanceStep > 0 ? beamRight : -beamRight;

        moveVelocity *= 0.2f;

        moveVelocity += sideDir * currentBalanceBeam.failPushForce;

        velocity.y = Mathf.Sqrt(currentBalanceBeam.failUpForce * -2f * gravity);

        ForceExitBalanceMode();
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

        if (isInNarrowPassageMode) return;
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

        if (isInBalanceMode || isInNarrowPassageMode || isOnLadder)
            return;

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
                    SpawnShockwave();
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
        if (isInBalanceMode || isInNarrowPassageMode || isOnLadder)
        {
            if (isSliding)
                EndSlide();

            if (isCrouching)
                StopCrouch();

            return;
        }

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
        if (isOnLadder && playerCamera != null && currentLadderZone != null)
        {
            float input = Mathf.Abs(Input.GetAxisRaw("Vertical"));

            if (input > 0.01f)
            {
                ladderBobTimer += Time.deltaTime * currentLadderZone.ladderCameraBobSpeed;
                float bob = Mathf.Sin(ladderBobTimer) * currentLadderZone.ladderCameraBobAmount;

                Vector3 target = defaultCamLocalPos + new Vector3(0f, bob, 0f);
                playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, target, Time.deltaTime * 10f);
            }
            else
            {
                playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, defaultCamLocalPos, Time.deltaTime * 8f);
            }

            return;
        }

        Vector3 horizontalVel = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        bool moving = horizontalVel.magnitude > 0.1f && controller.isGrounded;

        if (moving && !isSliding)
        {
            bobTimer += Time.deltaTime * bobSpeed * (isCrouching ? 0.7f : 1f);
            float currentBobAmount = isCrouching ? crouchBobAmount : bobAmount;

            if (isInBalanceMode)
                currentBobAmount *= 0.45f;

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
        else if (!isInBalanceMode && boost > 0.1f && horizontalSpeed > walkSpeed)
        {
            float boostFactor = boost / maxBoost;
            targetFOV += boostFactor * maxFovIncrease;
        }

        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothTime);
    }

    void UpdateBalanceVisuals()
    {
        if (isInBalanceMode && currentBalanceBeam != null)
        {

        }


        else if (isInNarrowPassageMode && currentNarrowPassageZone != null)
        {
            targetCameraRoll = currentNarrowPassageZone.cameraRoll;
        }
        else
        {
            targetCameraRoll = 0f;
        }

        currentCameraRoll = Mathf.Lerp(currentCameraRoll, targetCameraRoll, Time.deltaTime * 10f);
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
                staminaBars[i].color = emptyColor;
            }
        }
    }

    void SpawnShockwave()
    {
        Vector3 spawnPos = playerCamera.position + (playerCamera.forward * shockwaveSpawnDistance);
        GameObject wave = Instantiate(shockwavePrefab, spawnPos, Quaternion.identity, playerCamera);
        wave.transform.LookAt(playerCamera);
    }

    public void EnterNarrowPassageMode(NarrowPassageZone zone)
    {
        if (zone == null)
            return;

        if (isInBalanceMode)
            return;

        currentNarrowPassageZone = zone;
        isInNarrowPassageMode = true;

        Vector3 lookSource = playerCamera != null ? playerCamera.forward : transform.forward;
        activePassageMoveForward = zone.GetPassageForwardFromLook(lookSource);

        boost = 0f;
        slideJumpTimer = 0f;

        if (isSliding)
            EndSlide();

        if (isCrouching)
            StopCrouch();

        controller.radius = zone.narrowControllerRadius;
    }

    public void ExitNarrowPassageMode(NarrowPassageZone zone)
    {
        if (zone != null && zone != currentNarrowPassageZone)
            return;

        ForceExitNarrowPassageMode();
    }

    void ForceExitNarrowPassageMode()
    {
        isInNarrowPassageMode = false;
        currentNarrowPassageZone = null;
        controller.radius = normalControllerRadius;
    }

    public void EnterBalanceMode(BalanceBeam beam)
    {
        if (beam == null)
            return;

        currentBalanceBeam = beam;
        isInBalanceMode = true;

        Vector3 lookSource = playerCamera != null ? playerCamera.forward : transform.forward;
        activeBeamMoveForward = beam.GetBeamForwardFromLook(lookSource);

        boost = 0f;
        slideJumpTimer = 0f;

        if (isSliding)
            EndSlide();

        if (isCrouching)
            StopCrouch();

        if (isInNarrowPassageMode)
            ForceExitNarrowPassageMode();

        balanceStep = 0;
        currentCameraRoll = 0f;
        targetCameraRoll = 0f;
        lastBalanceInputConsumed = false;
        ResetSwayTimer();
    }

    public void ExitBalanceMode(BalanceBeam beam)
    {
        if (beam != null && beam != currentBalanceBeam)
            return;

        ForceExitBalanceMode();
    }

    void ForceExitBalanceMode()
    {
        isInBalanceMode = false;
        currentBalanceBeam = null;
        balanceStep = 0;
        targetCameraRoll = 0f;
        lastBalanceInputConsumed = false;
    }

    public bool IsInBalanceMode()
    {
        return isInBalanceMode;
    }

    public bool IsInNarrowPassageMode()
    {
        return isInNarrowPassageMode;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawBalanceDebug || !isInBalanceMode || currentBalanceBeam == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + activeBeamMoveForward * 2f);

        Vector3 beamRight = currentBalanceBeam.GetBeamRightFromMoveDirection(activeBeamMoveForward);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + beamRight * Mathf.Sign(balanceStep));
    }
}