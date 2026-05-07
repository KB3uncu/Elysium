using System.Collections.Generic;
using UnityEngine;

public class RelicInventoryManager : MonoBehaviour
{
    public static RelicInventoryManager Instance { get; private set; }

    [Header("Camera Carry Anchor")]
    public Transform carryAnchor;

    [Tooltip("Anchor verilmezse sahnedeki Main Camera altýna otomatik anchor oluþturur.")]
    public bool createAnchorIfMissing = true;

    [Header("Carry Layout")]
    public Vector3 firstRelicLocalPosition = Vector3.zero;
    public Vector3 relicSpacing = new Vector3(-0.06f, 0.055f, -0.025f);
    public Vector3 baseRotationEuler = new Vector3(8f, -18f, -8f);

    [Header("Carry Idle Motion")]
    public bool enableCarryMotion = true;
    public float swaySpeed = 2.2f;
    public float swayAmount = 0.018f;
    public float rotationSwayAmount = 5f;

    private readonly List<CollectibleRelic> carriedRelics = new List<CollectibleRelic>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (carryAnchor == null && createAnchorIfMissing)
            CreateDefaultAnchor();
    }

    void Update()
    {
        if (!enableCarryMotion) return;

        for (int i = 0; i < carriedRelics.Count; i++)
        {
            CollectibleRelic relic = carriedRelics[i];
            if (relic == null) continue;

            ApplyCarryPose(relic, i);
        }
    }

    void CreateDefaultAnchor()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        GameObject anchorObj = new GameObject("LeftHandCarryAnchor");
        anchorObj.transform.SetParent(cam.transform);
        anchorObj.transform.localPosition = new Vector3(-0.34f, -0.28f, 0.62f);
        anchorObj.transform.localRotation = Quaternion.Euler(0f, -10f, 0f);
        anchorObj.transform.localScale = Vector3.one;

        carryAnchor = anchorObj.transform;
    }

    public Vector3 GetNextCarryWorldPosition()
    {
        if (carryAnchor == null)
            return transform.position;

        int index = carriedRelics.Count;

        Vector3 localPos = firstRelicLocalPosition + relicSpacing * index;
        return carryAnchor.TransformPoint(localPos);
    }

    public bool HasRelic(RelicType type)
    {
        for (int i = 0; i < carriedRelics.Count; i++)
        {
            if (carriedRelics[i] != null && carriedRelics[i].relicType == type)
                return true;
        }

        return false;
    }

    public void AddRelic(CollectibleRelic relic)
    {
        if (relic == null) return;
        if (carriedRelics.Contains(relic)) return;
        if (carryAnchor == null)
        {
            Debug.LogWarning("RelicInventoryManager: Carry Anchor yok.");
            return;
        }

        carriedRelics.Add(relic);

        relic.EnterCarryMode(carryAnchor);

        RearrangeCarriedRelics();
    }

    public CollectibleRelic TakeRelic(RelicType type)
    {
        for (int i = 0; i < carriedRelics.Count; i++)
        {
            CollectibleRelic relic = carriedRelics[i];

            if (relic == null) continue;

            if (relic.relicType == type)
            {
                carriedRelics.RemoveAt(i);
                RearrangeCarriedRelics();
                return relic;
            }
        }

        return null;
    }

    void RearrangeCarriedRelics()
    {
        for (int i = 0; i < carriedRelics.Count; i++)
        {
            CollectibleRelic relic = carriedRelics[i];
            if (relic == null) continue;

            ApplyCarryPose(relic, i, true);
        }
    }

    void ApplyCarryPose(CollectibleRelic relic, int index, bool instant = false)
    {
        if (carryAnchor == null || relic == null) return;

        Vector3 basePos = firstRelicLocalPosition + relicSpacing * index;
        Vector3 baseRot = baseRotationEuler + new Vector3(0f, index * 8f, index * -4f);

        float wave = Mathf.Sin(Time.time * swaySpeed + index * 0.8f);
        float wave2 = Mathf.Cos(Time.time * swaySpeed * 0.8f + index * 1.1f);

        Vector3 finalPos = basePos;

        if (enableCarryMotion && !instant)
        {
            finalPos += new Vector3(
                wave2 * swayAmount * 0.45f,
                wave * swayAmount,
                0f
            );
        }

        Vector3 finalRot = baseRot;

        if (enableCarryMotion && !instant)
        {
            finalRot += new Vector3(
                wave * rotationSwayAmount,
                wave2 * rotationSwayAmount,
                wave * rotationSwayAmount * 0.5f
            );
        }

        relic.SetCarryPose(finalPos, Quaternion.Euler(finalRot), instant);
    }
}