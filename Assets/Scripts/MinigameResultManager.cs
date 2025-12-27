using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class MinigameResultManager : MonoBehaviour
{
    public enum ResultType { None, Win, Lose }

    [Header("Minigame Sonunda Kapatýlacak Scriptler")]
    public MonoBehaviour[] scriptsToDisable;

    [Header("KAZANMA - Sandýk")]
    public GameObject chestPrefab;
    public Transform chestSpawnPoint;

    [Header("Parmaklýk (Win'de Kalkar)")]
    public Transform gate;
    public Vector3 gateUpOffset = new Vector3(0f, 2f, 0f);
    public float gateLiftDuration = 1f;

    [Header("KAYBETME - Animasyon Hook")]
    public UnityEvent onLose;

    [Header("Opsiyonel")]
    public UnityEvent onWin;


    [Header("START/RESET - Baþlangýçta Sistem Kapalý Beklesin")]
    public bool startInWaitingMode = true;

    [Tooltip("Oyuncu girince minigame baþlasýn mý? (StartMinigame() çaðýrman yeterli)")]
    public bool autoStartOnPlayerEnter = true;

    [Header("RESET - Fare Baþlangýç Noktasý")]
    public Transform mouse;
    public Transform mouseStartPoint;

    [Header("RESET - Kapý Ýnerken (Start) kullan")]
    public Vector3 gateDownOffset = new Vector3(0f, -2f, 0f);
    public float gateDropDuration = 0.6f;

    [Header("RESET - Tur bitince kaç sn sonra reset?")]
    public float resetDelayAfterEnd = 0.8f;


    [Header("KAYBETME - Dev Balyoz Vuruþ + Respawn")]
    public SideHammerHit leftHammer;
    public SideHammerHit rightHammer;

    public Transform player;
    public Transform respawnPoint;

    public float hitDelay = 0.05f;
    public float respawnDelay = 0.45f;

    public ResultType CurrentResult { get; private set; } = ResultType.None;

    GameObject spawnedChest;

    Vector3 gateUpLocalPos;
    bool waitingForPlayer = true;

    void Awake()
    {
        if (gate != null)
            gateUpLocalPos = gate.localPosition;
    }

    void Start()
    {
        if (startInWaitingMode)
        {
            EnterWaitingMode();
        }
        else
        {
            waitingForPlayer = false;
        }
    }


    public void StartMinigame()
    {
        if (!waitingForPlayer) return;
        waitingForPlayer = false;



        CurrentResult = ResultType.None;


        if (spawnedChest != null)
        {
            Destroy(spawnedChest);
            spawnedChest = null;
        }


        SetScriptsEnabled(true);


        if (gate != null)
            StartCoroutine(MoveGate(gateUpLocalPos, gateUpLocalPos + gateDownOffset, gateDropDuration));
    }

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

        StartCoroutine(LoseSequence());


        StartCoroutine(ResetAfterDelay());
    }

    void EndCommon()
    {
        SetScriptsEnabled(false);
    }

    void SetScriptsEnabled(bool enabled)
    {
        if (scriptsToDisable != null)
        {
            foreach (var s in scriptsToDisable)
                if (s != null) s.enabled = enabled;
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

    IEnumerator LoseSequence()
    {
        yield return new WaitForSeconds(hitDelay);

        if (leftHammer != null) leftHammer.Hit();
        if (rightHammer != null) rightHammer.Hit();

        yield return new WaitForSeconds(respawnDelay);
        RespawnPlayer();
    }

    void RespawnPlayer()
    {
        if (player == null || respawnPoint == null) return;

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.position = respawnPoint.position;
        player.rotation = respawnPoint.rotation;

        if (cc != null) cc.enabled = true;
    }


    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelayAfterEnd);
        ResetRoom();
    }

    public void ResetRoom()
    {

        if (spawnedChest != null)
        {
            Destroy(spawnedChest);
            spawnedChest = null;
        }


        if (mouse != null && mouseStartPoint != null)
        {

            var mrb = mouse.GetComponent<Rigidbody>();
            if (mrb != null)
            {
                mrb.velocity = Vector3.zero;
                mrb.angularVelocity = Vector3.zero;
            }

            mouse.position = mouseStartPoint.position;
            mouse.rotation = mouseStartPoint.rotation;
        }


        if (gate != null)
        {
            StopAllCoroutines(); 
            StartCoroutine(MoveGate(gate.localPosition, gateUpLocalPos, gateLiftDuration));
        }


        EnterWaitingMode();
    }

    void EnterWaitingMode()
    {

        SetScriptsEnabled(false);

        CurrentResult = ResultType.None;

        waitingForPlayer = true;
    }

    IEnumerator MoveGate(Vector3 from, Vector3 to, float duration)
    {
        if (gate == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            gate.localPosition = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        gate.localPosition = to;
    }
}
