using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Base Settings")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float acceleration = 10f;
    public float friction = 2f;

    [Header("Boost Mechanic")]
    public float boostAmount = 3f;
    public float maxBoostSpeed = 15f;
    private float _currentBoost;

    [Header("Jump")]
    public float jumpHeight = 1.5f;
    public float gravity = -25f;

    [Header("Slide Settings")]
    public float slideDuration = 1.0f;
    public float slideHeight = 0.5f;
    public float crouchHeight = 1.0f;

    private float _defaultHeight;
    private bool _isSliding;
    private bool _isCrouching;
    private float _slideTimer;
    private Vector3 _slideDir;
    private float _slideStartSpeed;

    [Header("Look Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float upDownRange = 80f;

    [Header("FOV Settings")]
    public float fovSmoothTime = 10f;
    public float maxFovIncrease = 15f;

    [Header("Visual Effects")]
    public GameObject shockwavePrefab;
    public float shockwaveSpawnDistance = 2f;

    private CharacterController _controller;
    private Vector3 _currentMoveVelocity;
    private Vector3 _moveDampVelocity;
    private float _verticalVelocity;
    private float _verticalRotation;
    private Camera _cam;
    private float _defaultFOV;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _defaultHeight = _controller.height;

        if (playerCamera != null) _cam = playerCamera.GetComponent<Camera>();
        if (_cam != null) _defaultFOV = _cam.fieldOfView;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleBoostInput();
        HandleMovement();
        HandleStance();
        HandleFOV();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        _verticalRotation -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -upDownRange, upDownRange);
        playerCamera.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
    }

    void HandleBoostInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !_isSliding && !_isCrouching && _controller.isGrounded)
        {
            _currentBoost += boostAmount;
            _currentBoost = Mathf.Clamp(_currentBoost, 0, maxBoostSpeed);

            if (shockwavePrefab != null && playerCamera != null)
            {
                SpawnShockwave();
            }
        }

        if (!_isSliding && _currentBoost > 0)
        {
            _currentBoost -= friction * Time.deltaTime;
            if (_currentBoost < 0) _currentBoost = 0;
        }
    }

    void HandleMovement()
    {
        bool isGrounded = _controller.isGrounded;
        if (isGrounded && _verticalVelocity < 0) _verticalVelocity = -2f;

        if (!_isSliding)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            float baseSpeed = _isCrouching ? crouchSpeed : walkSpeed;
            float totalTargetSpeed = baseSpeed + _currentBoost;

            Vector3 inputDir = (transform.right * x + transform.forward * z).normalized;
            Vector3 targetMoveVelocity = inputDir * totalTargetSpeed;

            _currentMoveVelocity = Vector3.SmoothDamp(_currentMoveVelocity, targetMoveVelocity, ref _moveDampVelocity, 1f / acceleration);
        }

        if (Input.GetButtonDown("Jump") && isGrounded && !_isCrouching)
        {
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (_isSliding) EndSlide();
        }

        _verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = _currentMoveVelocity * Time.deltaTime;
        finalMove.y = _verticalVelocity * Time.deltaTime;

        _controller.Move(finalMove);
    }

    void HandleStance()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && _controller.isGrounded)
        {
            if (_currentMoveVelocity.magnitude > walkSpeed + 1f)
            {
                StartSlide();
            }
            else
            {
                StartCrouch();
            }
        }

        if (_isSliding)
        {
            _slideTimer -= Time.deltaTime;

            float actualHorizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
            if (_slideTimer < (slideDuration - 0.2f) && actualHorizontalSpeed < 0.5f)
            {
                EndSlide();
                return;
            }

            if (_slideTimer <= 0)
            {
                EndSlide();
                return;
            }

            float decayFactor = (_slideTimer / slideDuration);
            _currentMoveVelocity = _slideDir * (_slideStartSpeed * decayFactor);

            if (_currentMoveVelocity.magnitude < crouchSpeed) EndSlide();
        }

        if (_isCrouching && Input.GetKeyUp(KeyCode.LeftControl))
        {
            StopCrouch();
        }
    }

    void HandleFOV()
    {
        if (_cam == null) return;

        float targetFOV = _defaultFOV;

        float horizontalSpeed = new Vector3(_currentMoveVelocity.x, 0f, _currentMoveVelocity.z).magnitude;

        if (_isSliding)
        {
            targetFOV += maxFovIncrease * 0.8f;
        }
        else if (_currentBoost > 0.1f && horizontalSpeed > walkSpeed)
        {
            float boostFactor = _currentBoost / maxBoostSpeed;
            targetFOV += boostFactor * maxFovIncrease;
        }

        // Yumuþak geçiþ
        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothTime);
    }

    void SpawnShockwave()
    {
        Vector3 spawnPos = playerCamera.position + (playerCamera.forward * shockwaveSpawnDistance);
        GameObject wave = Instantiate(shockwavePrefab, spawnPos, Quaternion.identity, playerCamera);

        wave.transform.LookAt(playerCamera);
        // wave.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }
    void StartCrouch()
    {
        _isCrouching = true;
        _controller.height = crouchHeight;
    }

    void StopCrouch()
    {
        _isCrouching = false;
        _controller.height = _defaultHeight;
    }

    void StartSlide()
    {
        _isSliding = true;
        _isCrouching = false;

        _slideStartSpeed = _currentMoveVelocity.magnitude;
        if (_slideStartSpeed < 10f) _slideStartSpeed = 10f;

        _controller.height = slideHeight;
        _slideTimer = slideDuration;
        _slideDir = _currentMoveVelocity.normalized;

        _currentBoost = 0;
    }

    void EndSlide()
    {
        _isSliding = false;
        _controller.height = _defaultHeight;
    }
}