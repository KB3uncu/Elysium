using UnityEngine;

public class RouletteEnemyTarget : MonoBehaviour, IInteractable
{
    public RouletteGameManager gameManager;

    public void OnInteract()
    {
        if (gameManager == null) return;

        gameManager.PlayerShoot();
    }
}