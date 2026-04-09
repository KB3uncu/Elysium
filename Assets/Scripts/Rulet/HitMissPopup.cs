using System.Collections;
using UnityEngine;

public class HitMissPopup : MonoBehaviour
{
    [Header("Scene Objects (prefabsiz)")]
    public GameObject hitObj;
    public GameObject missObj;

    [Header("Timing")]
    public float showTime = 0.8f;

    Coroutine routine;

    void Awake()
    {
        // baþlangýçta ikisi de kapalý olsun
        if (hitObj) hitObj.SetActive(false);
        if (missObj) missObj.SetActive(false);
    }

    public void Show(bool isHit)
    {
        ShowInternal(isHit ? hitObj : missObj);
    }

    public void ShowHit() => ShowInternal(hitObj);
    public void ShowMiss() => ShowInternal(missObj);

    void ShowInternal(GameObject obj)
    {
        if (obj == null) return;

        // diðerini kapat
        if (hitObj && hitObj != obj) hitObj.SetActive(false);
        if (missObj && missObj != obj) missObj.SetActive(false);

        // bunu aç
        obj.SetActive(true);

        // süre dolunca kapat
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(HideLater(obj));
    }

    IEnumerator HideLater(GameObject obj)
    {
        yield return new WaitForSeconds(showTime);
        if (obj != null) obj.SetActive(false);
        routine = null;
    }
}
