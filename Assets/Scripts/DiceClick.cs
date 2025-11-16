using UnityEngine;

public class DiceClick : MonoBehaviour
{
    public RuletGame game;   // GameManager üzerindeki RuletGame'i buraya sürükleyeceðiz

    private void OnMouseDown()
    {
        if (game != null)
        {
            game.OnDiceClick();   // Zara týklanýnca RuletGame içindeki fonksiyonu çaðýr
        }
    }
}
