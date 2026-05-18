using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class HighFallParkourImpact : MonoBehaviour
{
    [Header("Fall Detection")]
    public float minFallDistance = 20f;
    public float cooldown = 1.5f;

    [Header("References")]
    public Transform playerCamera;
    public Image blackOverlay;
    public GlassParkourGenerator parkourGenerator;

    [Header("Parkour Reset")]
    public bool regenerateParkourOnImpact = true;

    [Tooltip("Kapalý kalsýn. Ekran karardýktan sonra parkur yenilenir.")]
    public bool regenerateImmediately = false;

    [Header("Yere Düþme Kamera Hissi")]
    public float cameraDownAmount = 3f;
    public float fallLookDownAngle = 45f;
    public float fallRollAngle = 45f;

    [Header("Süreler")]
    public float impactInDuration = 0.12f;
    public float impactHoldDuration = 0.25f;
    public float getUpDuration = 3.2f;

    [Header("Yalpalama")]
    public float wobblePositionAmount = 0.035f;
    public float wobbleRotationAmount = 4.5f;
    public float wobbleFrequency = 10f;

    [Header("Göz Kararmasý")]
    [Range(0f, 1f)] public float impactBlackAlpha = 0.98f;
    [Range(0f, 1f)] public float darkFadeStartAlpha = 0.45f;
    [Range(0f, 1f)] public float blinkAlpha = 0.75f;

    public float firstBlinkTime = 0.55f;
    public float secondBlinkTime = 1.25f;
    public float blinkDuration = 0.28f;

    [Header("Optional Audio")]
    public AudioSource impactAudio;

    private CharacterController characterController;

    private bool wasGrounded = true;
    private bool trackingFall = false;
    private bool effectActive = false;
    private bool onCooldown = false;

    private float highestYDuringFall;
    private Coroutine routine;

    private Vector3 visualPositionOffset = Vector3.zero;
    private Quaternion visualRotationOffset = Quaternion.identity;

    private Vector3 lastAppliedPositionOffset = Vector3.zero;
    private bool hasLastPositionOffset = false;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            FPSController fps = GetComponent<FPSController>();

            if (fps != null && fps.playerCamera != null)
                playerCamera = fps.playerCamera;
            else if (Camera.main != null)
                playerCamera = Camera.main.transform;
        }

        if (blackOverlay != null)
        {
            SetOverlayAlpha(0f);
            blackOverlay.gameObject.SetActive(false);
        }

        wasGrounded = characterController.isGrounded;
    }

    void Update()
    {
        bool grounded = characterController.isGrounded;

        if (!grounded)
        {
            if (!trackingFall)
            {
                trackingFall = true;
                highestYDuringFall = transform.position.y;
            }

            if (transform.position.y > highestYDuringFall)
                highestYDuringFall = transform.position.y;
        }

        if (grounded && !wasGrounded && trackingFall)
        {
            float fallDistance = highestYDuringFall - transform.position.y;

            trackingFall = false;

            if (fallDistance >= minFallDistance)
                TriggerHighFallImpact();
        }

        wasGrounded = grounded;
    }

    void LateUpdate()
    {
        if (playerCamera == null)
            return;

        if (hasLastPositionOffset)
        {
            playerCamera.localPosition -= lastAppliedPositionOffset;
            lastAppliedPositionOffset = Vector3.zero;
            hasLastPositionOffset = false;
        }

        if (!effectActive)
            return;

        playerCamera.localPosition += visualPositionOffset;
        lastAppliedPositionOffset = visualPositionOffset;
        hasLastPositionOffset = true;

        playerCamera.localRotation = playerCamera.localRotation * visualRotationOffset;
    }

    void TriggerHighFallImpact()
    {
        if (onCooldown)
            return;

        if (playerCamera == null)
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(HighFallImpactRoutine());
    }

    IEnumerator HighFallImpactRoutine()
    {
        onCooldown = true;
        effectActive = true;

        visualPositionOffset = Vector3.zero;
        visualRotationOffset = Quaternion.identity;

        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            SetOverlayAlpha(0f);
        }

        if (impactAudio != null)
            impactAudio.Play();

        if (regenerateParkourOnImpact && regenerateImmediately && parkourGenerator != null)
            parkourGenerator.GenerateParkour();

        Vector3 fallenPosOffset = new Vector3(0f, -cameraDownAmount, 0f);
        Quaternion fallenRotOffset = Quaternion.Euler(fallLookDownAngle, 0f, fallRollAngle);

        float t = 0f;

        while (t < impactInDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / impactInDuration);

            visualPositionOffset = Vector3.Lerp(Vector3.zero, fallenPosOffset, k);
            visualRotationOffset = Quaternion.Slerp(Quaternion.identity, fallenRotOffset, k);

            SetOverlayAlpha(Mathf.Lerp(0f, impactBlackAlpha, k));

            yield return null;
        }

        visualPositionOffset = fallenPosOffset;
        visualRotationOffset = fallenRotOffset;
        SetOverlayAlpha(impactBlackAlpha);

        if (regenerateParkourOnImpact && !regenerateImmediately && parkourGenerator != null)
            parkourGenerator.GenerateParkour();

        yield return new WaitForSeconds(impactHoldDuration);

        t = 0f;

        while (t < getUpDuration)
        {
            t += Time.deltaTime;

            float k = Mathf.Clamp01(t / getUpDuration);
            float smoothK = Mathf.SmoothStep(0f, 1f, k);
            float fadeOut = 1f - k;

            Vector3 basePosOffset = Vector3.Lerp(fallenPosOffset, Vector3.zero, smoothK);

            Vector3 wobblePos = new Vector3(
                Mathf.Sin(t * wobbleFrequency) * wobblePositionAmount,
                Mathf.Sin(t * wobbleFrequency * 1.25f) * wobblePositionAmount * 0.5f,
                0f
            ) * fadeOut;

            visualPositionOffset = basePosOffset + wobblePos;

            Quaternion baseRotOffset = Quaternion.Slerp(fallenRotOffset, Quaternion.identity, smoothK);

            Quaternion wobbleRotOffset = Quaternion.Euler(
                Mathf.Sin(t * wobbleFrequency) * wobbleRotationAmount * fadeOut,
                Mathf.Sin(t * wobbleFrequency * 0.8f) * wobbleRotationAmount * 0.35f * fadeOut,
                Mathf.Sin(t * wobbleFrequency * 1.2f) * wobbleRotationAmount * 0.7f * fadeOut
            );

            visualRotationOffset = baseRotOffset * wobbleRotOffset;

            float baseDarkness = Mathf.Lerp(darkFadeStartAlpha, 0f, k);
            float blink = GetBlinkAlpha(t, firstBlinkTime) + GetBlinkAlpha(t, secondBlinkTime);

            SetOverlayAlpha(Mathf.Clamp01(Mathf.Max(baseDarkness, blink)));

            yield return null;
        }

        visualPositionOffset = Vector3.zero;
        visualRotationOffset = Quaternion.identity;
        effectActive = false;

        SetOverlayAlpha(0f);

        if (blackOverlay != null)
            blackOverlay.gameObject.SetActive(false);

        routine = null;

        yield return new WaitForSeconds(cooldown);

        onCooldown = false;
    }

    float GetBlinkAlpha(float currentTime, float blinkStartTime)
    {
        if (currentTime < blinkStartTime)
            return 0f;

        if (currentTime > blinkStartTime + blinkDuration)
            return 0f;

        float p = (currentTime - blinkStartTime) / blinkDuration;
        return Mathf.Sin(p * Mathf.PI) * blinkAlpha;
    }

    void SetOverlayAlpha(float alpha)
    {
        if (blackOverlay == null)
            return;

        Color c = blackOverlay.color;
        c.a = alpha;
        blackOverlay.color = c;
    }
}