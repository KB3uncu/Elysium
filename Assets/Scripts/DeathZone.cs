using UnityEngine;

public class DeathZone : MonoBehaviour
{
    public GlassMinigameController minigameController;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        minigameController.RespawnPlayerAndReset();
    }
}
