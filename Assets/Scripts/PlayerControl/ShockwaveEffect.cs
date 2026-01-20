using UnityEngine;
// VFX Graph kullanýyorsan bu kütüphaneyi eklemelisin, 
// ama þu an sadece Transform deðiþtireceðimiz için zorunlu deðil.

public class ShockwaveEffect : MonoBehaviour
{
    public float lifeTime = 1f;      // Ömür süresi

    void Start()
    {
        // Belirlenen süre sonunda objeyi sahneden sil
        Destroy(gameObject, lifeTime);
    }

}