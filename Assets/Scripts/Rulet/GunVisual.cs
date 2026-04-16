using UnityEngine;

public class GunVisual : MonoBehaviour
{
    public GameObject tableGun; // masadaki
    public GameObject handGun;  // eldeki

    MeshRenderer[] tableRenderers;
    MeshRenderer[] handRenderers;

    Collider[] tableColliders;
    Collider[] handColliders;

    void Awake()
    {
        if (tableGun != null)
        {
            tableRenderers = tableGun.GetComponentsInChildren<MeshRenderer>(true);
            tableColliders = tableGun.GetComponentsInChildren<Collider>(true);
        }

        if (handGun != null)
        {
            handRenderers = handGun.GetComponentsInChildren<MeshRenderer>(true);
            handColliders = handGun.GetComponentsInChildren<Collider>(true);
        }

        // Baþlangýç
        SetGunVisible(handRenderers, false);
        SetGunColliders(handColliders, false);

        SetGunVisible(tableRenderers, true);
        SetGunColliders(tableColliders, true);
    }

    public void Pickup()
    {
        SetGunVisible(tableRenderers, false);
        SetGunColliders(tableColliders, false);

        SetGunVisible(handRenderers, true);
        SetGunColliders(handColliders, true);
    }

    public void PutDown()
    {
        SetGunVisible(handRenderers, false);
        SetGunColliders(handColliders, false);

        SetGunVisible(tableRenderers, true);
        SetGunColliders(tableColliders, true);
    }

    void SetGunVisible(MeshRenderer[] rends, bool visible)
    {
        if (rends == null) return;

        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i] != null)
                rends[i].enabled = visible;
        }
    }

    void SetGunColliders(Collider[] cols, bool enabledState)
    {
        if (cols == null) return;

        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] != null)
                cols[i].enabled = enabledState;
        }
    }
}