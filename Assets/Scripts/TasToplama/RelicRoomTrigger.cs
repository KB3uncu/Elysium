using System.Collections;
using UnityEngine;

public class RelicRoomTrigger : MonoBehaviour
{
    [Header("Relic To Collect")]
    public CollectibleRelic relic;

    [Header("Trigger Settings")]
    public bool collectOnlyOnce = true;
    public float collectDelay = 0f;

    private bool triggered = false;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered && collectOnlyOnce) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        StartCoroutine(CollectAfterDelay());
    }

    IEnumerator CollectAfterDelay()
    {
        if (collectDelay > 0f)
            yield return new WaitForSeconds(collectDelay);

        if (relic != null)
            relic.BeginCollect();
        else
            Debug.LogWarning("RelicRoomTrigger: Relic referansý boþ.");
    }
}