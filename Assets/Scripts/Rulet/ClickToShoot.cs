using UnityEngine;

public class ClickToShoot : MonoBehaviour
{
    public RouletteGameManager gameManager;

    [Header("Distances")]
    public float interactDistance = 3f;   // E için (zar + silah)
    public float shootDistance = 30f;     // Mouse için (enemy)

    void Update()
    {
        if (gameManager == null) return;
        if (gameManager.IsGameOver) return;

        if (Camera.main == null) return;

        // FPS: ekranýn ortasýndan ray
        Ray centerRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // E: Zar + Masadaki Silah
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(centerRay, out RaycastHit hit, interactDistance))
            {
                // Masadaki silah
                if (hit.collider.CompareTag("PlayerGunTable"))
                {
                    gameManager.PlayerPickGun();
                    return;
                }

                // Player zarý
                if (hit.collider.CompareTag("PlayerDice"))
                {
                    gameManager.PlayerRollDice();
                    return;
                }
            }
        }

        // Mouse Sol: Ateþ (sadece enemy)
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(centerRay, out RaycastHit hit, shootDistance))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    gameManager.PlayerShoot();
                    return;
                }
            }
        }
    }
}
