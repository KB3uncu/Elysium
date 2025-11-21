using UnityEngine;

public class MinigameControllerFare : MonoBehaviour
{
    public MonoBehaviour[] scriptsToToggle;  
    private bool started = false;
    

    void Awake()
    {
        
        SetScriptsActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartGame();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopGame();
        }
    }

    public void StartGame()
    {
        if (started) return;
        started = true;
        SetScriptsActive(true);
    }

    public void StopGame()
    {
        started = false;
        SetScriptsActive(false);
       
    }

    void SetScriptsActive(bool value)
    {
        foreach (var s in scriptsToToggle)
        {
            if (s != null)
                s.enabled = value;
        }
    }
}
