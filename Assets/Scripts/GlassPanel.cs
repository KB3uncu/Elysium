using UnityEngine;

public class GlassPanel : MonoBehaviour
{
    [HideInInspector] public bool isCorrect;
    bool stepped = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || stepped)
            return;

        stepped = true;

        if (isCorrect)
        {
            Debug.Log("Doðru cam! Güvenli.");
        }
        else
        {
            Debug.Log("Yanlýþ cam! Kýrýlýyor...");
            gameObject.SetActive(false);
        }
    }

    public void ResetPanel()
    {
        stepped = false;
        gameObject.SetActive(true);
    }
}
