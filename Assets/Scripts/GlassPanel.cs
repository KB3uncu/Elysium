using UnityEngine;

public class GlassPanel : MonoBehaviour
{
    [HideInInspector] public bool isCorrect;
    private bool stepped = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Oyuncu deðilse veya zaten tetiklendiyse çýk
        if (!collision.collider.CompareTag("Player") || stepped)
            return;

        stepped = true;

        if (isCorrect)
        {
            Debug.Log("Doðru cam! Güvenli.");
        }
        else
        {
            Debug.Log("Yanlýþ cam! Kýrýlýyor...");
            Destroy(gameObject);
        }
    }
}
