using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassMinigameController : MonoBehaviour
{
    [Header("Panelleri sýrayla ekle (satýr satýr, soldan saða)")]
    public GlassPanel[] panels;
    public int rowCount = 5;
    public int panelsPerRow = 3;

    [Header("Respawn Ayarlarý")]
    public Transform player;
    public Transform respawnPoint;

    [Header("Parlama Ayarlarý")]
    public float flashDuration = 0.4f;
    public float flashDelayBetween = 0.15f;

    private List<GlassPanel> correctPanels = new List<GlassPanel>();

    void Start()
    {
        RandomizeCorrectPanels();
    }

    public void RandomizeCorrectPanels()
    {
        correctPanels.Clear();

        foreach (var p in panels)
        {
            if (p != null)
                p.isCorrect = false;
        }

        Debug.Log("=== DOÐRU CAMLAR ===");

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
                        correctPanels.Add(panels[index]);

                        Debug.Log("Satýr " + (row + 1) + "  Doðru cam: " + panels[index].name);
                    }
                }
            }
        }

        Debug.Log("======================");

        StopAllCoroutines();
        StartCoroutine(FlashCorrectPanelsInOrder());
    }

    private IEnumerator FlashCorrectPanelsInOrder()
    {
        yield return new WaitForSeconds(0.2f);

        foreach (var panel in correctPanels)
        {
            if (panel != null)
            {
                panel.Flash(flashDuration);
                yield return new WaitForSeconds(flashDuration + flashDelayBetween);
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
