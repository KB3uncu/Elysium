using UnityEngine;

public class HitMissPopup : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject hitPrefab;
    public GameObject missPrefab;

    [Header("Spawn Point (tek nokta)")]
    public Transform spawnPoint;

    [Header("Timing")]
    public float lifeTime = 1.2f;

    public void ShowHit()
    {
        Spawn(hitPrefab);
    }

    public void ShowMiss()
    {
        Spawn(missPrefab);
    }

    public void Show(bool isHit)
    {
        Spawn(isHit ? hitPrefab : missPrefab);
    }

    void Spawn(GameObject prefab)
    {
        if (spawnPoint == null) return;
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        Destroy(go, lifeTime);
    }
}
