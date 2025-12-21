using UnityEngine;

public class ClickToShoot : MonoBehaviour
{
    public RouletteGameManager gameManager;

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (gameManager == null) return;
        if (gameManager.IsGameOver) return;

        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Masadaki silah
            if (hit.collider.CompareTag("PlayerGunTable"))
            {
                gameManager.PlayerPickGun();
                return;
            }

            // Enemy
            if (hit.collider.CompareTag("Enemy"))
            {
                gameManager.PlayerShoot();
                return;
            }
        }
    }
}
