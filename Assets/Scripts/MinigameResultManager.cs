using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class MinigameResultManager : MonoBehaviour
{
    public enum ResultType { None, Win, Lose }

    [Header("Minigame Sonunda Kapat�lacak Scriptler")]
    public MonoBehaviour[] scriptsToDisable;

    [Header("KAZANMA - Sand�k")]
    public GameObject chest;


    [Header("Parmakl�k (Win'de Kalkar)")]
    public Transform gate;
    public Vector3 gateUpOffset = new Vector3(0f, 2f, 0f);
    public float gateLiftDuration = 1f;

    [Header("KAYBETME - Animasyon Hook")]
    public UnityEvent onLose;

    [Header("Opsiyonel")]
    public UnityEvent onWin;


    [Header("START/RESET - Ba�lang��ta Sistem Kapal� Beklesin")]
    public bool startInWaitingMode = true;

    [Tooltip("Oyuncu girince minigame ba�las�n m�? (StartMinigame() �a��rman yeterli)")]
    public bool autoStartOnPlayerEnter = true;

    [Header("RESET - Fare Ba�lang�� Noktas�")]
    public Transform mouse;
    public Transform mouseStartPoint;

    [Header("RESET - Kap� �nerken (Start) kullan")]
    public Vector3 gateDownOffset = new Vector3(0f, -2f, 0f);
    public float gateDropDuration = 0.6f;

    [Header("RESET - Tur bitince ka� sn sonra reset?")]
    public float resetDelayAfterEnd = 0.8f;


    [Header("KAYBETME - Dev Balyoz Vuru� + Respawn")]
    public SideHammerHit leftHammer;
    public SideHammerHit rightHammer;

    public Transform player;
    public Transform respawnPoint;

    public float hitDelay = 0.05f;
    public float respawnDelay = 0.45f;

    public ResultType CurrentResult { get; private set; } = ResultType.None;



    Vector3 gateUpLocalPos;
    bool waitingForPlayer = true;

    void Awake()
    {
        if (gate != null)
            gateUpLocalPos = gate.localPosition;
    }

    void Start()
    {
        chest.SetActive(false);

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
        chest.SetActive(true);


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
            rb.linearVelocity = Vector3.zero;
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


        if (mouse != null && mouseStartPoint != null)
        {

            var mrb = mouse.GetComponent<Rigidbody>();
            if (mrb != null)
            {
                mrb.linearVelocity = Vector3.zero;
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
