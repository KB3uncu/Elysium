using UnityEngine;

public class ShieldInteractable : MonoBehaviour, IInteractable
{
    public RouletteGameManager gameManager;

    public bool OnInteract()
    {
        Debug.Log("SHIELD INTERACT ÇALIÞTI");

        if (gameManager == null)
            return false;

        gameManager.PlayerPressedShield();

        return true;
    }
}