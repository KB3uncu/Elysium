using UnityEngine;
using System.Collections;

public class BreakableWall : MonoBehaviour, IInteractable
{
    [Header("Parça Rigidbody'leri")]
    [Tooltip("Boþ býrakýrsan child'lardaki tüm Rigidbody'leri otomatik bulur.")]
    public Rigidbody[] pieces;

    [Header("Patlama Gücü Ayarlarý")]
    public float minForce = 3f;
    public float maxForce = 7f;
    public float minTorque = 1f;
    public float maxTorque = 4f;

    private bool broken = false;
    private PlayerGlove playerGlove;

    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerGlove = player.GetComponent<PlayerGlove>();

        if (pieces == null || pieces.Length == 0)
        {
            pieces = GetComponentsInChildren<Rigidbody>();
        }
    }

    void Start()
    {
        foreach (var rb in pieces)
        {
            if (rb == null) continue;

            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void OnInteract()
    {
        if (broken) return;

        if (playerGlove == null)
        {
            Debug.LogWarning("BreakableWall: PlayerGlove bulunamadý.");
            return;
        }

        if (!playerGlove.hasGlove)
        {
            Debug.Log("BreakableWall: Bu duvarý kýrmak için eldivene ihtiyacýn var!");
            return;
        }

        playerGlove.ConsumeGlove();

        Shatter();
    }

    private void Shatter()
    {
        broken = true;

        foreach (var rb in pieces)
        {
            if (rb == null) continue;

            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1.5f),
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddForce(randomDir * Random.Range(minForce, maxForce), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(minTorque, maxTorque), ForceMode.Impulse);
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        StartCoroutine(FadeOut());

    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(5f);

        Renderer[] rends = GetComponentsInChildren<Renderer>();
        float t = 0f;
        float duration = 1.5f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / duration);

            foreach (var r in rends)
            {
                var c = r.material.color;
                c.a = alpha;
                r.material.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

}
