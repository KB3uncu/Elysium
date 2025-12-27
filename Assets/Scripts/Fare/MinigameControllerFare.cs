using UnityEngine;
using System.Collections;

public class MinigameControllerFare : MonoBehaviour

{
    public MinigameResultManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (manager == null) return;

        if (other.CompareTag("Player"))
        {
            manager.StartMinigame();
        }
    }
}



