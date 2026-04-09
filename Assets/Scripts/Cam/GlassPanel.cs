using UnityEngine;
using System.Collections;

public class GlassPanel : MonoBehaviour
{
    [HideInInspector] public bool isCorrect;
    bool stepped = false;

    [Header("Saðlam cam child")]
    public GameObject saglamCam;

    [Header("Kýrýk cam prefab")]
    public GameObject kirikCamPrefab;
    private GameObject currentKirikCam;

    [Header("Glow Ayarlarý")]
    public Color glowColor = Color.cyan;
    public float glowIntensity = 3f;

    private void Start()
    {
        if (saglamCam != null)
            saglamCam.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || stepped)
            return;

        stepped = true;

        if (isCorrect)
        {
            Debug.Log("Doðru cam! Güvenli.");
            //burada da mini bir flash çaðýrabiliriz
           
        }
        else
        {
            Debug.Log("Yanlýþ cam! Kýrýlýyor...");

            if (saglamCam != null)
                saglamCam.SetActive(false);

            SpawnAndShatterBrokenGlass();
        }
    }

    void SpawnAndShatterBrokenGlass()
    {
        if (kirikCamPrefab == null) return;

        if (currentKirikCam == null)
        {
            currentKirikCam = Instantiate(
                kirikCamPrefab,
                saglamCam != null ? saglamCam.transform.position : transform.position,
                saglamCam != null ? saglamCam.transform.rotation : transform.rotation,
                transform 
            );
        }

        Rigidbody[] rbs = currentKirikCam.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1.5f),
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddForce(randomDir * Random.Range(2f, 5f), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(1f, 3f), ForceMode.Impulse);
        }
    }

    public void ResetPanel()
    {
        stepped = false;

        if (saglamCam != null)
            saglamCam.SetActive(true);

        if (currentKirikCam != null)
        {
            Destroy(currentKirikCam);
            currentKirikCam = null;
        }
    }

    public void Flash(float duration)
    {
        if (saglamCam == null) return;
        StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        var rend = saglamCam.GetComponent<MeshRenderer>();
        if (rend == null)
            rend = saglamCam.GetComponentInChildren<MeshRenderer>();
        if (rend == null) yield break;

        Material mat = rend.material;

        Color baseEmission = mat.IsKeywordEnabled("_EMISSION")
            ? mat.GetColor("_EmissionColor")
            : Color.black;

        mat.EnableKeyword("_EMISSION");

        Color targetEmission = glowColor * glowIntensity;

        float t = 0f;
        while (t < duration * 0.5f)
        {
            t += Time.deltaTime;
            float lerp = t / (duration * 0.5f);
            mat.SetColor("_EmissionColor", Color.Lerp(baseEmission, targetEmission, lerp));
            yield return null;
        }

        //parlak kalma süresi
        yield return new WaitForSeconds(duration * 0.1f);

        t = 0f;
        while (t < duration * 0.4f)
        {
            t += Time.deltaTime;
            float lerp = t / (duration * 0.4f);
            mat.SetColor("_EmissionColor", Color.Lerp(targetEmission, baseEmission, lerp));
            yield return null;
        }

        mat.SetColor("_EmissionColor", baseEmission);
    }
}
