using UnityEngine;

public class Revolver
{
    private bool[] chambers;
    private int chamberIndex;
    private int shotsThisCycle;

    private int chamberCount;
    private int bulletCount;

    public int ShotsThisCycle => shotsThisCycle;
    public int ChamberCount => chamberCount;

    public Revolver(int chamberCount, int bulletCount)
    {
        this.chamberCount = Mathf.Max(1, chamberCount);
        this.bulletCount = Mathf.Clamp(bulletCount, 0, this.chamberCount);

        BuildAndShuffle();
    }

    public void BuildAndShuffle()
    {
        chambers = new bool[chamberCount];
        chamberIndex = 0;
        shotsThisCycle = 0;

        int placed = 0;
        while (placed < bulletCount)
        {
            int idx = Random.Range(0, chamberCount);
            if (!chambers[idx])
            {
                chambers[idx] = true;
                placed++;
            }
        }
    }

    /// <summary>
    /// Returns true if bullet fired.
    /// Outputs cycleCompleted when full cycle ends.
    /// </summary>
    public bool Fire(out bool cycleCompleted)
    {
        cycleCompleted = false;

        bool bullet = chambers[chamberIndex];

        chamberIndex++;
        shotsThisCycle++;

        if (chamberIndex >= chamberCount)
            chamberIndex = 0;

        if (shotsThisCycle >= chamberCount)
        {
            BuildAndShuffle();
            cycleCompleted = true;
        }

        return bullet;
    }
}
