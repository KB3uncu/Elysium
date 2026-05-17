using UnityEngine;

public class RouletteDice : MonoBehaviour, IInteractable
{
    public RouletteGameManager gameManager;
    public bool isPlayerDice = true;

    public bool OnInteract()
    {
        if (gameManager == null)
            return false;

        if (!isPlayerDice)
            return false;

        gameManager.PlayerRollDice();

        return true;
    }
}