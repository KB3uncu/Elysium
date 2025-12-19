using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class MinigameResultManager : MonoBehaviour
{
    public enum ResultType { None, Win, Lose }

    [Header("Minigame Sonunda Kapatýlacak Scriptler")]
    public MonoBehaviour[] scriptsToDisable;

    [Header("KAZANMA - Sandýk")]
    public GameObject chestPrefab;       // Bu minigame’e özel sandýk
    public Transform chestSpawnPoint;    // Sandýðýn çýkacaðý yer

    [Header("Parmaklýk (Win'de Kalkar)")]
    public Transform gate;
    public Vector3 gateUpOffset = new Vector3(0f, 2f, 0f);
    public float gateLiftDuration = 1f;

    [Header("KAYBETME - Animasyon Hook")]
    public UnityEvent onLose;             // Þimdilik boþ, animasyon buraya

    [Header("Opsiyonel")]
    public UnityEvent onWin;              // VFX / SFX vs.

    public ResultType CurrentResult { get; private set; } = ResultType.None;

    GameObject spawnedChest;

    public void Win()
    {
        if (CurrentResult != ResultType.None) return;
        CurrentResult = ResultType.Win;

        EndCommon();

        StartCoroutine(LiftGate());

        onWin?.Invoke();
        SpawnChest();
    }

    public void Lose()
    {
        if (CurrentResult != ResultType.None) return;
        CurrentResult = ResultType.Lose;

        EndCommon();

        onLose?.Invoke();
    }

    void EndCommon()
    {
        if (scriptsToDisable != null)
        {
            foreach (var s in scriptsToDisable)
                if (s != null) s.enabled = false;
        }
    }

    void SpawnChest()
    {
        if (chestPrefab == null || chestSpawnPoint == null) return;
        if (spawnedChest != null) return;

        spawnedChest = Instantiate(
            chestPrefab,
            chestSpawnPoint.position,
            chestSpawnPoint.rotation
        );
    }

    IEnumerator LiftGate()
    {
        if (gate == null) yield break;

        Vector3 startPos = gate.localPosition;
        Vector3 endPos = startPos + gateUpOffset;

        float t = 0f;
        while (t < gateLiftDuration)
        {
            t += Time.deltaTime;
            gate.localPosition = Vector3.Lerp(startPos, endPos, t / gateLiftDuration);
            yield return null;
        }

        gate.localPosition = endPos;
    }



}
