using UnityEngine;

public class GunVisual : MonoBehaviour
{
    public GameObject tableGun;
    public GameObject handGun;

    Collider[] tableColliders;
    Collider[] handColliders;

    void Awake()
    {
        if (tableGun != null)
            tableColliders = tableGun.GetComponentsInChildren<Collider>(true);

        if (handGun != null)
            handColliders = handGun.GetComponentsInChildren<Collider>(true);

        PutDown();
    }

    public void Pickup()
    {
        if (tableGun != null)
            tableGun.SetActive(false);

        if (handGun != null)
            handGun.SetActive(true);

        SetGunColliders(tableColliders, false);
        SetGunColliders(handColliders, true);
    }

    public void PutDown()
    {
        if (handGun != null)
            handGun.SetActive(false);

        if (tableGun != null)
            tableGun.SetActive(true);

        SetGunColliders(handColliders, false);
        SetGunColliders(tableColliders, true);
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