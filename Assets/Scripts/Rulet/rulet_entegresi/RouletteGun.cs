using UnityEngine;

public class RouletteGun : MonoBehaviour, IInteractable
{
    public RouletteGameManager gameManager;

    bool isPicked = false;

    public bool OnInteract()
    {
        if (isPicked)
            return true;

        if (gameManager == null)
            return false;

        bool success = gameManager.PlayerPickGun();

        if (!success)
            return false;

        isPicked = true;
        return true;
    }
}