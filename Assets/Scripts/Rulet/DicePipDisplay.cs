using UnityEngine;

public class DicePipDisplay : MonoBehaviour
{
    [Tooltip("Size 6. Active (colored) pips in order 1..6")]
    public GameObject[] activePips = new GameObject[6];

    void Awake()
    {
        SetCount(0);
    }

    public void SetCount(int count)
    {
        count = Mathf.Clamp(count, 0, 6);

        for (int i = 0; i < activePips.Length; i++)
        {
            if (activePips[i] == null) continue;
            activePips[i].SetActive(i < count);
        }
    }
}
