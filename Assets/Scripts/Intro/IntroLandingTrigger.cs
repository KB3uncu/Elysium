using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class IntroLandingTrigger : MonoBehaviour
{
    [Header("Player Kontrol")]
    public string playerTag = "Player";
    public bool playOnlyOnce = true;
    public bool disableTriggerAfterPlay = true;

    [Header("Referanslar")]
    public Transform playerCamera;
    public Image blackOverlay;

    [Header("Yere Düþme Kamera Hissi")]
    public float cameraDownAmount = 0.55f;
    public float fallLookDownAngle = 55f;
    public float fallRollAngle = 8f;

    [Header("Süreler")]
    public float impactInDuration = 0.12f;
    public float impactHoldDuration = 0.35f;
    public float getUpDuration = 2.4f;

    [Header("Yalpalama")]
    public float wobblePositionAmount = 0.035f;
    public float wobbleRotationAmount = 5f;
    public float wobbleFrequency = 14f;

    [Header("Göz Kararmasý")]
    [Range(0f, 1f)] public float impactBlackAlpha = 0.95f;
    [Range(0f, 1f)] public float darkFadeStartAlpha = 0.5f;
    [Range(0f, 1f)] public float blinkAlpha = 0.75f;

    public float firstBlinkTime = 0.55f;
    public float secondBlinkTime = 1.25f;
    public float blinkDuration = 0.28f;

    private bool hasPlayed = false;
    private bool effectActive = false;

    private Collider triggerCollider;
    private Coroutine routine;

    private Vector3 visualPositionOffset = Vector3.zero;
    private Quaternion visualRotationOffset = Quaternion.identity;

    private Vector3 lastAppliedPositionOffset = Vector3.zero;
    private bool hasLastPositionOffset = false;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (blackOverlay != null)
        {
            SetOverlayAlpha(0f);
            blackOverlay.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasPlayed && playOnlyOnce)
            return;

        if (!IsPlayer(other))
            return;

        AutoFindCamera(other);

        if (playerCamera == null)
        {
            Debug.LogWarning("IntroLandingTrigger: Kamera bulunamadý.");
            return;
        }

        hasPlayed = true;

        if (disableTriggerAfterPlay && triggerCollider != null)
            triggerCollider.enabled = false;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(LandingVisualRoutine());
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

    bool IsPlayer(Collider other)
    {
        if (other.CompareTag(playerTag))
            return true;

        CharacterController cc = other.GetComponentInParent<CharacterController>();
        if (cc != null && cc.CompareTag(playerTag))
            return true;

        Transform root = other.transform.root;
        if (root != null && root.CompareTag(playerTag))
            return true;

        return false;
    }

    void AutoFindCamera(Collider other)
    {
        FPSController fps = other.GetComponentInParent<FPSController>();

        if (fps != null && fps.playerCamera != null)
        {
            playerCamera = fps.playerCamera;
            return;
        }

        if (Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    IEnumerator LandingVisualRoutine()
    {
        effectActive = true;

        visualPositionOffset = Vector3.zero;
        visualRotationOffset = Quaternion.identity;

        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            SetOverlayAlpha(0f);
        }

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