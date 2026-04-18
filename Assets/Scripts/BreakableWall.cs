using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class BreakableWall : MonoBehaviour, IInteractable
{
    [Header("Pieces")]
    public Rigidbody[] pieces;

    [Header("Player Break Force")]
    public float minForce = 3f;
    public float maxForce = 7f;
    public float minTorque = 1f;
    public float maxTorque = 4f;

    [Header("Optional Hit Source")]
    public Transform hitSource;

    [Header("Events")]
    public UnityEvent onBroken;

    bool broken = false;
    bool breaking = false;
    PlayerGlove playerGlove;

    public Vector3 lastHitPoint;
    public Vector3 lastHitNormal;
    public bool hasLastHit;

    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerGlove = player.GetComponent<PlayerGlove>();
            if (hitSource == null) hitSource = player.transform;
        }

        if (pieces == null || pieces.Length == 0)
            pieces = GetComponentsInChildren<Rigidbody>(true);
    }

    void Start()
    {
        foreach (var rb in pieces)
        {
            if (rb == null) continue;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public void SetLastHit(Vector3 point, Vector3 normal)
    {
        lastHitPoint = point;
        lastHitNormal = normal;
        hasLastHit = true;
    }

    public void OnInteract()
    {
        if (broken || breaking) return;
        if (playerGlove == null) return;
        if (!playerGlove.hasGlove) return;
        if (VFXManager.Instance == null) return;

        VFXManager.Instance.PunchWall(this, playerGlove);
    }

    public void FinishBreak(PlayerGlove glove)
    {
        if (broken || breaking) return;

        breaking = true;

        if (glove != null && glove.hasGlove)
            glove.ConsumeGlove();

        ShatterWithPlayerForce();
    }

    public void GetShatterHit(out Vector3 point, out Vector3 normal)
    {
        if (hasLastHit)
        {
            point = lastHitPoint;
            normal = lastHitNormal;
            return;
        }

        var col = GetComponent<Collider>();
        if (col == null)
        {
            point = transform.position;
            normal = transform.forward;
            return;
        }

        point = col.bounds.center;
        normal = transform.forward;
    }

    public void BreakFromWorld(Vector3 forceDirection, float forceMultiplier = 1f)
    {
        if (broken) return;

        Vector3 dir = forceDirection.sqrMagnitude > 0.001f
            ? forceDirection.normalized
            : transform.forward;

        ShatterCustom(
            dir,
            minForce * forceMultiplier,
            maxForce * forceMultiplier,
            minTorque * forceMultiplier,
            maxTorque * forceMultiplier
        );
    }

    void ShatterWithPlayerForce()
    {
        Vector3 forceDir = (Vector3.back + new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(0.2f, 0.8f),
            0f)).normalized;

        ShatterCustom(forceDir, minForce, maxForce, minTorque, maxTorque);
    }

    void ShatterCustom(Vector3 baseForceDir, float minF, float maxF, float minT, float maxT)
    {
        broken = true;
        breaking = false;

        foreach (var rb in pieces)
        {
            if (rb == null) continue;

            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 randomDir = (baseForceDir + new Vector3(
                Random.Range(-0.25f, 0.25f),
                Random.Range(0.1f, 0.7f),
                Random.Range(-0.25f, 0.25f)
            )).normalized;

            rb.AddForce(randomDir * Random.Range(minF, maxF), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(minT, maxT), ForceMode.Impulse);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        onBroken?.Invoke();
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(5f);

        Renderer[] rends = GetComponentsInChildren<Renderer>(true);
        float t = 0f;
        float duration = 1.5f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / duration);

            foreach (var r in rends)
            {
                if (r == null) continue;
                if (!r.material.HasProperty("_Color")) continue;

                Color c = r.material.color;
                c.a = alpha;
                r.material.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}