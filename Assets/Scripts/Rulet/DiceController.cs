using UnityEngine;

public enum DiceOwner { Player, Enemy }

public class DiceController : MonoBehaviour
{
    public DiceOwner owner;
    public RouletteGameManager gameManager;

    void OnMouseDown()
    {
        if (gameManager == null) return;

        // Sadece player zarý týklanabilir
        if (owner == DiceOwner.Player)
            gameManager.PlayerRollDice();
    }
}
