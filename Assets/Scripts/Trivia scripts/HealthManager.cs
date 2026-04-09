using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    [Header("Trivia Health Objects")]
    public GameObject[] healthObjects;

    public int healthLeft {  get; private set; }
    void Start()
    {
        healthLeft = healthObjects.Length;
        Refresh();
    }

    public void LoseHealth()
    {
        if (healthLeft <= 0)
            return;

        healthLeft--;
        Refresh();
    }
    void Refresh()
    {
        for (int i = 0; i < healthObjects.Length; i++)
        {
            if (healthObjects[i] != null)
                healthObjects[i].SetActive(i < healthLeft);
        }
    }

    public bool IsDead()
    {
        return healthLeft <= 0;
    }

}
