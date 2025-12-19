using UnityEngine;
using System.Collections;

public class MiniGameTriviaLine : MonoBehaviour
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

    public void StartGame()
    {
        if (started) return;
        started = true;

        SetScriptsActive(true);
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
