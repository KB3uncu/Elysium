using UnityEngine;

public class WinZone : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject boostStaminaUI;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Win panel aç
        winPanel.SetActive(true);

        // stamina UI kapat
        if (boostStaminaUI != null)
            boostStaminaUI.SetActive(false);

        // baþka UI varsa komple kapatmak istersen:
        Canvas canvas = winPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            foreach (Transform child in canvas.transform)
            {
                if (child.gameObject != winPanel)
                    child.gameObject.SetActive(false);
            }
        }

        Debug.Log("WIN!");
    }
}