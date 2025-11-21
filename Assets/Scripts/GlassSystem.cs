using UnityEngine;

public class GlassMinigameController : MonoBehaviour
{
    [Header("Panelleri sýrayla ekle (satýr satýr, soldan saða)")]
    public GlassPanel[] panels;   // Toplam 15 panel
    public int rowCount = 5;
    public int panelsPerRow = 3;

    [Header("Respawn Ayarlarý")]
    public Transform player;       
    public Transform respawnPoint; 

    void Start()
    {
        RandomizeCorrectPanels();
    }

    public void RandomizeCorrectPanels()
    {
        foreach (var p in panels)
        {
            if (p != null)
                p.isCorrect = false;
        }


        for (int row = 0; row < rowCount; row++)
        {
            int correctIndexInRow = Random.Range(0, panelsPerRow);

            int startIndex = row * panelsPerRow;

            for (int i = 0; i < panelsPerRow; i++)
            {
                int index = startIndex + i;

                if (index < panels.Length && panels[index] != null)
                {
                    if (i == correctIndexInRow)
                    {
                        panels[index].isCorrect = true;
                        Debug.Log("Satýr " + (row + 1) + "  Doðru cam: " + panels[index].name);
                    }
                }
            }
        }

    }

    
    public void RespawnPlayerAndReset()
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = respawnPoint.position;
        player.rotation = respawnPoint.rotation;

        if (cc != null) cc.enabled = true;

        foreach (var p in panels)
        {
            if (p != null)
                p.ResetPanel();
        }

        RandomizeCorrectPanels();

        Debug.Log("Oyuncu respawn edildi, mini game sýfýrlandý.");
    }
}
