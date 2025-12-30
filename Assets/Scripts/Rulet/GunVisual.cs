using UnityEngine;

public class GunVisual : MonoBehaviour
{
    public GameObject tableGun; // masadaki
    public GameObject handGun;  // eldeki

    MeshRenderer[] tableRenderers;
    MeshRenderer[] handRenderers;

    void Awake()
    {
        if (tableGun) tableRenderers = tableGun.GetComponentsInChildren<MeshRenderer>(true);
        if (handGun) handRenderers = handGun.GetComponentsInChildren<MeshRenderer>(true);

        SetGunVisible(handRenderers, false); // eldeki baþta görünmesin
        SetGunVisible(tableRenderers, true); // masadaki görünsün
    }

    public void Pickup()
    {
        SetGunVisible(tableRenderers, false);
        SetGunVisible(handRenderers, true);
    }

    public void PutDown()
    {
        SetGunVisible(handRenderers, false);
        SetGunVisible(tableRenderers, true);
    }

    void SetGunVisible(MeshRenderer[] rends, bool visible)
    {
        if (rends == null) return;
        for (int i = 0; i < rends.Length; i++)
            rends[i].enabled = visible;
    }
}
