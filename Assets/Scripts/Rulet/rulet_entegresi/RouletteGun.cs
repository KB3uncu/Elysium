using UnityEngine;

public class RouletteGun : MonoBehaviour, IInteractable
{
    public RouletteGameManager gameManager;

    public void OnInteract()
    {
        if (gameManager == null) return;

        gameManager.PlayerPickGun();
    }
}