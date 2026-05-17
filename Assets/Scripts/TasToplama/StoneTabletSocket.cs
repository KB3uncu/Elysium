using System.Collections;
using UnityEngine;

public class StoneTabletSocket : MonoBehaviour, IInteractable
{
    [Header("Tablet Requirement")]
    public RelicType requiredRelic;

    [Header("Placement")]
    public Transform placementPoint;

    [Tooltip("Tablette önceden düzgün oturtulmuþ, baþta kapalý duracak nesne.")]
    public GameObject placedVisual;

    [Header("References")]
    public RelicInventoryManager inventory;
    public RelicPuzzleManager puzzleManager;

    [Header("Relic Fly Animation")]
    public float relicFlyDuration = 0.55f;
    public AnimationCurve relicFlyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Reveal Feedback")]
    public GameObject placementVfxPrefab;
    public float placementVfxLifetime = 2f;

    [Tooltip("Titreyecek obje. Boþ kalýrsa tabletin kendisi titrer.")]
    public Transform shakeTarget;

    public float shakeDuration = 0.22f;
    public float shakeStrength = 0.035f;

    [Header("Glow Feedback")]
    public bool useGlow = true;
    public Color glowColor = Color.white;
    public float glowPower = 3f;
    public float glowDuration = 0.45f;

    [Header("State")]
    public bool isFilled = false;

    private bool isPlacing = false;
    private Renderer[] placedVisualRenderers;

    void Awake()
    {
        if (placementPoint == null)
            placementPoint = transform;

        if (shakeTarget == null)
            shakeTarget = transform;

        if (inventory == null)
            inventory = RelicInventoryManager.Instance;

        if (puzzleManager == null)
            puzzleManager = FindFirstObjectByType<RelicPuzzleManager>();

        if (placedVisual != null)
        {
            placedVisual.SetActive(false);
            placedVisualRenderers = placedVisual.GetComponentsInChildren<Renderer>(true);
        }
    }

    public void OnInteract()
    {
        if (isFilled) return;
        if (isPlacing) return;

        if (inventory == null)
            inventory = RelicInventoryManager.Instance;

        if (inventory == null)
        {
            Debug.LogWarning("StoneTabletSocket: RelicInventoryManager bulunamadý.");
            return;
        }

        if (!inventory.HasRelic(requiredRelic))
        {
            Debug.Log("Bu tablete uygun relic oyuncuda yok: " + requiredRelic);
            return;
        }

        CollectibleRelic relic = inventory.TakeRelic(requiredRelic);

        if (relic == null)
            return;

        StartCoroutine(PlaceRelicSequence(relic));
    }

    IEnumerator PlaceRelicSequence(CollectibleRelic relic)
    {
        isPlacing = true;
        isFilled = true;

        bool relicArrived = false;

        relic.FlyToTabletAndHide(
            placementPoint,
            relicFlyDuration,
            relicFlyCurve,
            () => relicArrived = true
        );

        while (!relicArrived)
            yield return null;

        RevealPlacedVisual();

        if (placementVfxPrefab != null)
        {
            GameObject vfx = Instantiate(placementVfxPrefab, placementPoint.position, placementPoint.rotation);
            Destroy(vfx, placementVfxLifetime);
        }

        if (useGlow)
            StartCoroutine(GlowRoutine());

        if (shakeTarget != null && shakeDuration > 0f && shakeStrength > 0f)
            StartCoroutine(ShakeRoutine());

        if (puzzleManager != null)
            puzzleManager.OnTabletFilled(this);

        isPlacing = false;
    }

    void RevealPlacedVisual()
    {
        if (placedVisual == null)
        {
            Debug.LogWarning("StoneTabletSocket: Placed Visual boþ. Tablette açýlacak hazýr nesne atanmamýþ.");
            return;
        }

        placedVisual.SetActive(true);
    }

    IEnumerator ShakeRoutine()
    {
        Vector3 startLocalPos = shakeTarget.localPosition;
        float t = 0f;

        while (t < shakeDuration)
        {
            t += Time.deltaTime;

            float fade = 1f - Mathf.Clamp01(t / shakeDuration);

            Vector3 randomOffset = Random.insideUnitSphere * shakeStrength * fade;
            randomOffset.y *= 0.4f;

            shakeTarget.localPosition = startLocalPos + randomOffset;

            yield return null;
        }

        shakeTarget.localPosition = startLocalPos;
    }

    IEnumerator GlowRoutine()
    {
        if (placedVisualRenderers == null || placedVisualRenderers.Length == 0)
            yield break;

        float half = glowDuration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / half);
            SetPlacedVisualEmission(k * glowPower);
            yield return null;
        }

        t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / half);
            SetPlacedVisualEmission(k * glowPower);
            yield return null;
        }

        SetPlacedVisualEmission(0f);
    }

    void SetPlacedVisualEmission(float power)
    {
        if (placedVisualRenderers == null) return;

        Color finalColor = glowColor * power;

        for (int i = 0; i < placedVisualRenderers.Length; i++)
        {
            Renderer r = placedVisualRenderers[i];
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
}