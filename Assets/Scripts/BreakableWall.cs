using UnityEngine;
using System.Collections;

public class BreakableWall : MonoBehaviour, IInteractable
{
    [Header("Parça Rigidbody'leri")]
    public Rigidbody[] pieces;

    [Header("Patlama Gücü Ayarları")]
    public float minForce = 3f;
    public float maxForce = 7f;
    public float minTorque = 1f;
    public float maxTorque = 4f;

    private bool broken = false;
    private bool breaking = false;
    private PlayerGlove playerGlove;

    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerGlove = player.GetComponent<PlayerGlove>();

        if (pieces == null || pieces.Length == 0)
            pieces = GetComponentsInChildren<Rigidbody>();
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

    public void OnInteract()
    {
        if (broken || breaking) return;

        if (playerGlove == null)
        {
            Debug.LogWarning("BreakableWall: PlayerGlove bulunamadı.");
            return;
        }

        if (!playerGlove.hasGlove)
        {
            Debug.Log("BreakableWall: Bu duvarı kırmak için eldivene ihtiyacın var!");
            return;
        }

        if (VFXManager.Instance == null)
        {
            Debug.LogWarning("BreakableWall: VFXManager.Instance yok!");
            return;
        }

        breaking = true;

        VFXManager.Instance.StartWallBreakSequence(this, playerGlove);
    }

    public void FinishBreak(PlayerGlove glove)
    {
        if (broken) return;

        if (glove != null && glove.hasGlove)
            glove.ConsumeGlove();

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

            Vector3 forceDir = (Vector3.back + new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(0.2f, 0.8f),
                0f)).normalized;

            rb.AddForce(forceDir * Random.Range(minForce, maxForce), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(minTorque, maxTorque), ForceMode.Impulse);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

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