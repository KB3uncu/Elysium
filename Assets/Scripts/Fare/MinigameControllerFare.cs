using UnityEngine;
using System.Collections;

public class MinigameControllerFare : MonoBehaviour
{
    public MonoBehaviour[] scriptsToToggle;
    private bool started = false;

    [Header("Demir Parmaklýk")]
    public Transform gate;
    public float dropDistance = 7f;
    public float dropSpeed = 3f;

    private Vector3 gateStartPos;

    void Awake()
    {

        SetScriptsActive(false);

        if (gate != null)
            gateStartPos = gate.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartGame();
        }
    }

   // void OnTriggerExit(Collider other)
   // {
     //   if (other.CompareTag("Player"))
     //   {
        //    StopGame();
      //  }
   // }

    public void StartGame()
    {
        if (started) return;
        started = true;

        SetScriptsActive(true);

        if (gate != null)
            StartCoroutine(MoveGateDown());
    }

   // public void StopGame()
   // {
    //    started = false;
     //   SetScriptsActive(false);
   // }

    void SetScriptsActive(bool value)
    {
        foreach (var s in scriptsToToggle)
        {
            if (s != null)
                s.enabled = value;
        }
    }

    IEnumerator MoveGateDown()
    {
        Vector3 targetPos = gateStartPos + Vector3.down * dropDistance;

        while (Vector3.Distance(gate.position, targetPos) > 0.01f)
        {
            gate.position = Vector3.MoveTowards(
                gate.position,
                targetPos,
                dropSpeed * Time.deltaTime
            );
            yield return null;
        }

        gate.position = targetPos;
    }
}
