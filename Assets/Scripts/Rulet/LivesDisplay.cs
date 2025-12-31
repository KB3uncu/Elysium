using UnityEngine;

public class LivesDisplay : MonoBehaviour
{
    [Tooltip("Sahnede dizdiðin can objeleri (soldan saða).")]
    public GameObject[] lifeObjects;

    public void SetLives(int lives)
    {
        if (lifeObjects == null) return;

        for (int i = 0; i < lifeObjects.Length; i++)
        {
            if (lifeObjects[i] == null) continue;
            lifeObjects[i].SetActive(i < lives);
        }
    }
}
