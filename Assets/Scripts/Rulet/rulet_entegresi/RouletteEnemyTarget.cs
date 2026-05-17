using UnityEngine;

public class RouletteEnemyTarget : MonoBehaviour
{
    public RouletteGameManager gameManager;

    public void Shoot()
    {
        if (gameManager == null)
            return;

        gameManager.PlayerShoot();
    }
}