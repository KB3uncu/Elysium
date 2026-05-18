using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepBreakGlass : MonoBehaviour
{
    [Header("Player Detection")]
    public string playerTag = "Player";

    public Vector3 detectionBoxSize = new Vector3(3f, 1f, 3f);

    public float detectionHeightOffset = 0.6f;

    public LayerMask detectionMask = ~0;
    public float checkInterval = 0.1f;

    [Header("Break Delay")]
    public float minBreakDelay = 3f;
    public float maxBreakDelay = 5f;

    [Header("Main Support Collider")]
    [Tooltip("Oyuncunun üstünde durduðu ana collider. Root objeye Box Collider ekleyip buraya verebilirsin.")]
    public Collider supportCollider;

    [Tooltip("Cam kýrýlýnca ana taþýyýcý collider kapansýn mý?")]
    public bool disableSupportColliderOnBreak = true;

    [Header("Glass Pieces")]
    [Tooltip("Boþ býrakýrsan child objelerdeki bütün Rigidbody'leri otomatik bulur.")]
    public Rigidbody[] pieces;

    [Tooltip("Kýrýlana kadar parça colliderlarý kapalý kalsýn. Root Box Collider oyuncuyu taþýr.")]
    public bool disablePieceCollidersUntilBreak = true;

    [Tooltip("Kýrýlýnca parçalarý root objeden ayýrýr.")]
    public bool detachPiecesOnBreak = true;

    [Header("Break Force")]
    public float outwardForce = 1.5f;
    public float upwardForce = 0.4f;
    public float randomForce = 1.2f;
    public float torqueForce = 4f;

    [Header("Effects")]
    public GameObject breakVfxPrefab;
    public AudioSource breakAudio;

    [Header("Respawn")]
    [Tooltip("Cam kýrýldýktan kaç saniye sonra ayný konumda tekrar toparlansýn.")]
    public float destroyPiecesAfter = 5f;

    [Tooltip("Respawn olduktan sonra hemen tekrar tetiklenmesin diye kýsa güvenlik süresi.")]
    public float respawnProtectionTime = 0.4f;

    [Header("Debug")]
    public bool drawDetectionGizmo = true;

    bool triggered = false;
    bool broken = false;
    float checkTimer;
    float lastRespawnTime = -999f;

    Collider[] pieceColliders;

    Transform[] originalParents;
    Vector3[] originalLocalPositions;
    Quaternion[] originalLocalRotations;
    Vector3[] originalLocalScales;

    Coroutine breakRoutine;
    Coroutine respawnRoutine;

    void Awake()
    {
        if (supportCollider == null)
            supportCollider = GetComponent<Collider>();

        if (pieces == null || pieces.Length == 0)
            pieces = GetComponentsInChildren<Rigidbody>(true);

        CacheOriginalPieceTransforms();
        CollectPieceColliders();
        PreparePieces();

        if (supportCollider != null)
            supportCollider.enabled = true;
    }

    void Update()
    {
        if (triggered || broken)
            return;

        if (Time.time < lastRespawnTime + respawnProtectionTime)
            return;

        checkTimer -= Time.deltaTime;

        if (checkTimer > 0f)
            return;

        checkTimer = checkInterval;

        if (IsPlayerOnGlass())
        {
            triggered = true;

            if (breakRoutine != null)
                StopCoroutine(breakRoutine);

            breakRoutine = StartCoroutine(BreakRoutine());
        }
    }

    void CacheOriginalPieceTransforms()
    {
        if (pieces == null)
            return;

        originalParents = new Transform[pieces.Length];
        originalLocalPositions = new Vector3[pieces.Length];
        originalLocalRotations = new Quaternion[pieces.Length];
        originalLocalScales = new Vector3[pieces.Length];

        for (int i = 0; i < pieces.Length; i++)
        {
            Rigidbody rb = pieces[i];

            if (rb == null)
                continue;

            Transform tr = rb.transform;

            originalParents[i] = tr.parent;
            originalLocalPositions[i] = tr.localPosition;
            originalLocalRotations[i] = tr.localRotation;
            originalLocalScales[i] = tr.localScale;
        }
    }

    void CollectPieceColliders()
    {
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);
        List<Collider> list = new List<Collider>();

        for (int i = 0; i < allColliders.Length; i++)
        {
            Collider col = allColliders[i];

            if (col == null)
                continue;

            if (col == supportCollider)
                continue;

            list.Add(col);
        }

        pieceColliders = list.ToArray();
    }

    void PreparePieces()
    {
        if (pieces != null)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                Rigidbody rb = pieces[i];

                if (rb == null)
                    continue;

                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                rb.useGravity = false;
                rb.isKinematic = true;
                rb.Sleep();
            }
        }

        SetPieceCollidersEnabled(!disablePieceCollidersUntilBreak);
    }

    void SetPieceCollidersEnabled(bool enabled)
    {
        if (pieceColliders == null)
            return;

        for (int i = 0; i < pieceColliders.Length; i++)
        {
            if (pieceColliders[i] != null)
                pieceColliders[i].enabled = enabled;
        }
    }

    bool IsPlayerOnGlass()
    {
        Vector3 center = transform.position + transform.up * detectionHeightOffset;
        Vector3 halfExtents = detectionBoxSize * 0.5f;

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            transform.rotation,
            detectionMask,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hit = hits[i];

            if (hit == null)
                continue;

            if (hit.CompareTag(playerTag))
                return true;

            Transform root = hit.transform.root;

            if (root != null && root.CompareTag(playerTag))
                return true;
        }

        return false;
    }

    IEnumerator BreakRoutine()
    {
        float delay = Random.Range(minBreakDelay, maxBreakDelay);
        yield return new WaitForSeconds(delay);

        BreakGlass();
    }

    public void BreakGlass()
    {
        if (broken)
            return;

        broken = true;

        if (breakVfxPrefab != null)
            Instantiate(breakVfxPrefab, transform.position, transform.rotation);

        if (breakAudio != null)
            breakAudio.Play();

        if (disableSupportColliderOnBreak && supportCollider != null)
            supportCollider.enabled = false;

        SetPieceCollidersEnabled(true);

        if (pieces != null)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                Rigidbody rb = pieces[i];

                if (rb == null)
                    continue;

                if (detachPiecesOnBreak)
                    rb.transform.SetParent(null, true);

                rb.isKinematic = false;
                rb.useGravity = true;
                rb.WakeUp();

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                Vector3 outwardDirection = rb.transform.position - transform.position;
                outwardDirection.y = 0f;

                if (outwardDirection.sqrMagnitude < 0.01f)
                    outwardDirection = Random.insideUnitSphere;

                outwardDirection.y = 0f;
                outwardDirection.Normalize();

                Vector3 force =
                    outwardDirection * outwardForce +
                    Vector3.up * upwardForce +
                    Random.insideUnitSphere * randomForce;

                rb.AddForce(force, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);
            }
        }

        if (respawnRoutine != null)
            StopCoroutine(respawnRoutine);

        respawnRoutine = StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(destroyPiecesAfter);

        RespawnGlass();
    }

    void RespawnGlass()
    {
        if (pieces != null)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                Rigidbody rb = pieces[i];

                if (rb == null)
                    continue;

                Transform tr = rb.transform;

                if (originalParents != null && i < originalParents.Length)
                    tr.SetParent(originalParents[i], false);

                if (originalLocalPositions != null && i < originalLocalPositions.Length)
                    tr.localPosition = originalLocalPositions[i];

                if (originalLocalRotations != null && i < originalLocalRotations.Length)
                    tr.localRotation = originalLocalRotations[i];

                if (originalLocalScales != null && i < originalLocalScales.Length)
                    tr.localScale = originalLocalScales[i];

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.useGravity = false;
                rb.isKinematic = true;
                rb.Sleep();
            }
        }

        SetPieceCollidersEnabled(!disablePieceCollidersUntilBreak);

        if (supportCollider != null)
            supportCollider.enabled = true;

        triggered = false;
        broken = false;
        lastRespawnTime = Time.time;

        breakRoutine = null;
        respawnRoutine = null;
    }

    void OnDestroy()
    {
        if (!Application.isPlaying)
            return;

        if (pieces == null)
            return;

        for (int i = 0; i < pieces.Length; i++)
        {
            Rigidbody rb = pieces[i];

            if (rb == null)
                continue;

            if (rb.transform.parent == null)
                Destroy(rb.gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawDetectionGizmo)
            return;

        Gizmos.color = Color.red;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            transform.position + transform.up * detectionHeightOffset,
            transform.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, detectionBoxSize);

        Gizmos.matrix = oldMatrix;
    }
}