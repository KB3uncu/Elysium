using UnityEngine;

public class GlovePickup : MonoBehaviour, IInteractable
{
    public PlayerGlove playerGlove;
    public GameObject pickupVisual;

    
    public bool destroyOnPickup = true;

    void Awake()
    {
        if (playerGlove == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerGlove = player.GetComponent<PlayerGlove>();
        }
    }

    public void OnInteract()
    {
        if (playerGlove == null)
        {
            Debug.LogWarning("GlovePickup: PlayerGlove referansý bulunamadý.");
            return;
        }

        if (playerGlove.hasGlove)
        {
            Debug.Log("GlovePickup: Oyuncunun zaten eldiveni var.");
            return;
        }

        playerGlove.EquipGlove();
        VFXManager.Instance.OnGloveEquipped();

        if (pickupVisual != null)
            pickupVisual.SetActive(false);

        if (destroyOnPickup)
            Destroy(gameObject);
    }
}