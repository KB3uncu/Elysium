using UnityEngine;

public class RouletteDice : MonoBehaviour, IInteractable
{
    public RouletteGameManager gameManager;
    public bool isPlayerDice = true;

    public void OnInteract()
    {
        if (gameManager == null) return;
        if (!isPlayerDice) return;

        gameManager.PlayerRollDice();
    }
}