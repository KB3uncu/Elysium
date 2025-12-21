using UnityEngine;

public class GunVisual : MonoBehaviour
{
    public GameObject tableGun; // masadaki
    public GameObject handGun;  // eldeki

    void Start()
    {
        PutDown(); // oyun baþýnda masada olsun
    }

    public void Pickup()
    {
        tableGun.SetActive(false);
        handGun.SetActive(true);
    }

    public void PutDown()
    {
        tableGun.SetActive(true);
        handGun.SetActive(false);
    }

    public bool IsInHand => handGun.activeSelf;
}
