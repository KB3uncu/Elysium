using UnityEngine;
using System.Collections;

public class GlovePickup : MonoBehaviour, IInteractable
{
    public PlayerGlove playerGlove;
    public GameObject pickupVisual;

    public bool destroyOnPickup = true;
    public float equipDelay = 0.5f;

    bool isPickingUp = false;
    Collider[] cols;
    Renderer[] rends;

    void Awake()
    {
        if (playerGlove == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerGlove = player.GetComponent<PlayerGlove>();
        }

        if (pickupVisual == null)
            pickupVisual = gameObject;

        cols = GetComponentsInChildren<Collider>(true);
        rends = pickupVisual.GetComponentsInChildren<Renderer>(true);
    }

    public void OnInteract()
    {
        if (isPickingUp) return;

        if (playerGlove == null)
        {
            Debug.LogWarning("GlovePickup: PlayerGlove referansý bulunamadý.");
            return;
        }

        if (playerGlove.hasGlove)
            return;

        isPickingUp = true;
        SetPickupEnabled(false);
        StartCoroutine(PickupRoutine());
    }

    IEnumerator PickupRoutine()
    {
        if (VFXManager.Instance != null)
            VFXManager.Instance.PlayPickupAnim();

        yield return new WaitForSecondsRealtime(equipDelay);

        playerGlove.EquipGlove();

        if (VFXManager.Instance != null)
            VFXManager.Instance.OnGloveEquipped();

        if (destroyOnPickup)
            Destroy(gameObject);
    }

    void SetPickupEnabled(bool enabled)
    {
        if (cols != null)
            for (int i = 0; i < cols.Length; i++)
                if (cols[i] != null) cols[i].enabled = enabled;

        if (rends != null)
            for (int i = 0; i < rends.Length; i++)
                if (rends[i] != null) rends[i].enabled = enabled;
    }
}