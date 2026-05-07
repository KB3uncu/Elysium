using UnityEngine;

public class StoneTabletSocket : MonoBehaviour, IInteractable
{
    [Header("Tablet Requirement")]
    public RelicType requiredRelic;

    [Header("Placement")]
    public Transform placementPoint;

    [Header("References")]
    public RelicInventoryManager inventory;
    public RelicPuzzleManager puzzleManager;

    [Header("State")]
    public bool isFilled = false;

    void Awake()
    {
        if (placementPoint == null)
            placementPoint = transform;

        if (inventory == null)
            inventory = RelicInventoryManager.Instance;

        if (puzzleManager == null)
            puzzleManager = FindFirstObjectByType<RelicPuzzleManager>();
    }

    public void OnInteract()
    {
        if (isFilled) return;

        if (inventory == null)
            inventory = RelicInventoryManager.Instance;

        if (inventory == null)
        {
            Debug.LogWarning("StoneTabletSocket: RelicInventoryManager bulunamadý.");
            return;
        }

        if (!inventory.HasRelic(requiredRelic))
        {
            Debug.Log("Bu tablete uygun relic oyuncuda yok: " + requiredRelic);
            return;
        }

        CollectibleRelic relic = inventory.TakeRelic(requiredRelic);

        if (relic == null)
            return;

        isFilled = true;

        relic.PlaceOnTablet(placementPoint);

        if (puzzleManager != null)
            puzzleManager.OnTabletFilled(this);
    }
}