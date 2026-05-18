using UnityEngine;
using System.Collections;

public class GlovePickup : MonoBehaviour, IInteractable
{
    public PlayerGlove playerGlove;
    public GameObject pickupVisual;

    [Header("Pickup Ayarlarý")]
    public float equipDelay = 0.5f;

    bool isPickingUp = false;
    bool isTaken = false;

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

    public bool OnInteract()
    {
        if (isPickingUp) return false;
        if (isTaken) return false;

        if (playerGlove == null)
        {
            Debug.LogWarning("GlovePickup: PlayerGlove referansý bulunamadý.");
            return false;
        }

        if (playerGlove.hasGlove)
            return false;

        isPickingUp = true;
        SetPickupEnabled(false);
        StartCoroutine(PickupRoutine());

        return false;
    }

    IEnumerator PickupRoutine()
    {
        if (VFXManager.Instance != null)
            VFXManager.Instance.PlayPickupAnim();

        yield return new WaitForSecondsRealtime(equipDelay);

        playerGlove.EquipGlove();

        if (VFXManager.Instance != null)
            VFXManager.Instance.OnGloveEquipped();

        isPickingUp = false;
        isTaken = true;
    }

    public void ResetPickup()
    {
        StopAllCoroutines();

        isPickingUp = false;
        isTaken = false;

        SetPickupEnabled(true);
    }

    void SetPickupEnabled(bool enabled)
    {
        if (cols != null)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] != null)
                    cols[i].enabled = enabled;
            }
        }

        if (rends != null)
        {
            for (int i = 0; i < rends.Length; i++)
            {
                if (rends[i] != null)
                    rends[i].enabled = enabled;
            }
        }
    }
}