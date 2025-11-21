using UnityEngine;

public class GlassMinigameController : MonoBehaviour
{
    public GlassPanel[] panels;  
    public int rowCount = 5;    
    public int panelsPerRow = 3; 

    void Start()
    {
        RandomizeCorrectPanels();
    }

    void RandomizeCorrectPanels()
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

        Debug.Log("Bütün satýrlarda rastgele doðru camlar seçildi.");
    }
}
