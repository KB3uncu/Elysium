using UnityEngine;

public class ShieldInteractable : MonoBehaviour, IInteractable
{
    public RouletteGameManager gameManager;

    public void OnInteract()
    {
        Debug.Log("SHIELD INTERACT ÇALIÞTI");

        if (gameManager == null) return;

        gameManager.PlayerPressedShield();
    }
}