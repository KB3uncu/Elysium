using UnityEngine;

public class DiceClick : MonoBehaviour
{
    public RuletGame game;

    private void OnMouseDown()
    {
        if (game != null)
        {
            game.OnDiceClick(); 
        }
    }
}
