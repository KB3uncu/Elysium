using UnityEngine;
using UnityEngine.Events;

public class RelicPuzzleManager : MonoBehaviour
{
    [Header("Tablets")]
    public StoneTabletSocket[] tablets;

    [Header("Puzzle Complete")]
    public UnityEvent onAllRelicsPlaced;

    private bool completed = false;

    public void OnTabletFilled(StoneTabletSocket filledTablet)
    {
        if (completed) return;

        if (AreAllTabletsFilled())
        {
            completed = true;
            Debug.Log("Tüm relicler doðru tabletlere yerleþtirildi.");

            onAllRelicsPlaced?.Invoke();
        }
    }

    bool AreAllTabletsFilled()
    {
        if (tablets == null || tablets.Length == 0)
            return false;

        for (int i = 0; i < tablets.Length; i++)
        {
            if (tablets[i] == null)
                continue;

            if (!tablets[i].isFilled)
                return false;
        }

        return true;
    }

    public void ResetToEscapeCheckpoint()
    {
        completed = false;

        Debug.Log("RelicPuzzleManager: Checkpoint resetlendi.");
    }
}