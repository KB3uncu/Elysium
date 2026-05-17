using System;
using System.Collections;
using UnityEngine;

public class CollectibleRelic : MonoBehaviour
{
    [Header("Relic")]
    public RelicType relicType;

    [Header("Inventory")]
    public RelicInventoryManager inventory;

    [Header("Collect Animation")]
    public float shakeDuration = 0.35f;
    public float shakeStrength = 0.045f;
    public float glowDuration = 0.25f;
    public float flyDuration = 0.8f;
    public AnimationCurve flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Scale")]
    public float carryScaleMultiplier = 0.12f;
    public float tabletScaleMultiplier = 0.22f;

    [Header("Glow")]
    public bool useEmissionGlow = true;
    public Color emissionColor = Color.white;
    public float maxEmissionPower = 2.5f;

    [Header("Tablet Fly Preview")]
    public Vector3 tabletRotationOffsetEuler = Vector3.zero;

    public bool IsCollected { get; private set; }
    public bool IsPlaced { get; private set; }
    public bool IsCollecting { get; private set; }

    private Vector3 originalLocalScale;
    private Collider[] colliders;
    private Rigidbody[] rigidbodies;
    private Renderer[] renderers;

    private Coroutine collectRoutine;
    private Coroutine tabletFlyRoutine;

    void Awake()
    {
        originalLocalScale = transform.localScale;

        colliders = GetComponentsInChildren<Collider>(true);
        rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        renderers = GetComponentsInChildren<Renderer>(true);

        if (inventory == null)
            inventory = RelicInventoryManager.Instance;
    }

    public void BeginCollect()
    {
        if (IsCollected || IsCollecting || IsPlaced) return;

        if (inventory == null)
            inventory = RelicInventoryManager.Instance;

        if (inventory == null)
        {
            Debug.LogWarning("CollectibleRelic: RelicInventoryManager bulunamadý.");
            return;
        }

        collectRoutine = StartCoroutine(CollectSequence());
    }

    IEnumerator CollectSequence()
    {
        IsCollecting = true;

        SetPhysicsEnabled(false);

        transform.SetParent(null, true);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startScale = transform.localScale;

        yield return StartCoroutine(ShakeRoutine(startPos));

        yield return StartCoroutine(GlowRoutine());

        float t = 0f;

        while (t < flyDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / flyDuration);
            float k = flyCurve.Evaluate(normalized);

            Vector3 targetPos = inventory.GetNextCarryWorldPosition();
            Vector3 targetScale = originalLocalScale * carryScaleMultiplier;

            transform.position = Vector3.Lerp(startPos, targetPos, k);
            transform.rotation = Quaternion.Slerp(startRot, inventory.carryAnchor.rotation, k);
            transform.localScale = Vector3.Lerp(startScale, targetScale, k);

            yield return null;
        }

        inventory.AddRelic(this);

        IsCollected = true;
        IsCollecting = false;
        collectRoutine = null;
    }

    IEnumerator ShakeRoutine(Vector3 basePos)
    {
        float t = 0f;

        while (t < shakeDuration)
        {
            t += Time.deltaTime;

            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * shakeStrength;
            randomOffset.y *= 0.4f;

            transform.position = basePos + randomOffset;

            float pulse = 1f + Mathf.Sin(t * 35f) * 0.025f;
            transform.localScale = originalLocalScale * pulse;

            yield return null;
        }

        transform.position = basePos;
        transform.localScale = originalLocalScale;
    }

    IEnumerator GlowRoutine()
    {
        if (!useEmissionGlow || renderers == null || renderers.Length == 0)
            yield break;

        float half = glowDuration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / half);
            SetEmission(k * maxEmissionPower);
            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / half);
            SetEmission(k * maxEmissionPower);
            yield return null;
        }

        SetEmission(0f);
    }

    void SetEmission(float power)
    {
        if (renderers == null) return;

        Color finalColor = emissionColor * power;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            Material[] mats = r.materials;

            for (int j = 0; j < mats.Length; j++)
            {
                Material mat = mats[j];
                if (mat == null) continue;

                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", finalColor);
                }
            }
        }
    }

    public void EnterCarryMode(Transform carryParent)
    {
        if (carryParent == null) return;

        transform.SetParent(carryParent, true);
        transform.localScale = originalLocalScale * carryScaleMultiplier;

        SetPhysicsEnabled(false);
    }

    public void SetCarryPose(Vector3 localPosition, Quaternion localRotation, bool instant)
    {
        if (IsPlaced) return;

        Vector3 targetScale = originalLocalScale * carryScaleMultiplier;

        if (instant)
        {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale = targetScale;
            return;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition, Time.deltaTime * 8f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, localRotation, Time.deltaTime * 8f);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 8f);
    }

    public void FlyToTabletAndHide(Transform placementPoint, float duration, AnimationCurve curve, Action onArrived)
    {
        if (placementPoint == null) return;

        if (tabletFlyRoutine != null)
            StopCoroutine(tabletFlyRoutine);

        tabletFlyRoutine = StartCoroutine(FlyToTabletAndHideRoutine(placementPoint, duration, curve, onArrived));
    }

    IEnumerator FlyToTabletAndHideRoutine(Transform placementPoint, float duration, AnimationCurve curve, Action onArrived)
    {
        IsPlaced = true;
        IsCollected = false;
        IsCollecting = false;

        SetPhysicsEnabled(false);

        transform.SetParent(null, true);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startScale = transform.localScale;

        Vector3 targetPos = placementPoint.position;
        Quaternion targetRot = placementPoint.rotation * Quaternion.Euler(tabletRotationOffsetEuler);
        Vector3 targetScale = originalLocalScale * tabletScaleMultiplier;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            float k = curve != null ? curve.Evaluate(normalized) : normalized;

            transform.position = Vector3.Lerp(startPos, targetPos, k);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, k);
            transform.localScale = Vector3.Lerp(startScale, targetScale, k);

            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        transform.localScale = targetScale;

        gameObject.SetActive(false);

        onArrived?.Invoke();

        tabletFlyRoutine = null;
    }

    void SetPhysicsEnabled(bool enabled)
    {
        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                    colliders[i].enabled = enabled;
            }
        }

        if (rigidbodies != null)
        {
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                Rigidbody rb = rigidbodies[i];
                if (rb == null) continue;

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = enabled;
                rb.isKinematic = !enabled;
            }
        }
    }
}