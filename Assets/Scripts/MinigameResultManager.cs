using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class MinigameResultManager : MonoBehaviour
{
    public enum ResultType { None, Win, Lose }

    [Header("Minigame Sonunda Kapatılacak Scriptler")]
    public MonoBehaviour[] scriptsToDisable;

    [Header("KAZANMA - Sandık")]
    public GameObject chest;

    [Header("Parmaklık Ayarları")]
    public Transform gate;
    public Vector3 gateUpOffset = new Vector3(0f, 2f, 0f);
    public float gateLiftDuration = 1f;

    [Header("KAYBETME - Animasyon Hook")]
    public UnityEvent onLose;

    [Header("Opsiyonel")]
    public UnityEvent onWin;

    [Header("START/RESET - Başlangıçta Sistem Kapalı Beklesin")]
    public bool startInWaitingMode = true;

    [Tooltip("Oyuncu girince minigame başlasın mı? (StartMinigame() çağırman yeterli)")]
    public bool autoStartOnPlayerEnter = true;

    [Header("RESET - Fare Başlangıç Noktası")]
    public Transform mouse;
    public Transform mouseStartPoint;

    [Header("RESET - Kapı inerken kullanılacak offset")]
    public Vector3 gateDownOffset = new Vector3(0f, -2f, 0f);
    public float gateDropDuration = 0.6f;

    [Header("RESET - Tur bitince kaç sn sonra reset?")]
    public float resetDelayAfterEnd = 0.8f;

    [Header("KAYBETME - Dev Balyoz Vuruş + Respawn")]
    public SideHammerHit leftHammer;
    public SideHammerHit rightHammer;

    public Transform player;
    public Transform respawnPoint;

    public float hitDelay = 0.05f;
    public float respawnDelay = 0.45f;

    public ResultType CurrentResult { get; private set; } = ResultType.None;

    private Vector3 gateClosedLocalPos;
    private Vector3 gateOpenedLocalPos;
    private bool waitingForPlayer = true;
    private Coroutine gateMoveRoutine;

    void Awake()
    {
        if (gate != null)
        {
            gateClosedLocalPos = gate.localPosition;
            gateOpenedLocalPos = gateClosedLocalPos + gateDownOffset;
        }
    }

    void Start()
    {
        if (chest != null)
            chest.SetActive(false);

        if (startInWaitingMode)
        {
            EnterWaitingMode();

            if (gate != null)
                gate.localPosition = gateClosedLocalPos;
        }
        else
        {
            waitingForPlayer = false;
        }
    }

    public void StartMinigame()
    {
        if (!waitingForPlayer)
            return;

        waitingForPlayer = false;
        CurrentResult = ResultType.None;

        SetScriptsEnabled(true);

        MoveGateTo(gateOpenedLocalPos, gateDropDuration);
    }

    public void Win()
    {
        if (CurrentResult != ResultType.None)
            return;

        CurrentResult = ResultType.Win;

        EndCommon();

        MoveGateTo(gateClosedLocalPos, gateLiftDuration);

        onWin?.Invoke();

        if (chest != null)
            chest.SetActive(true);

        if (mouse != null)
            Destroy(mouse.gameObject);
    }

    public void Lose()
    {
        if (CurrentResult != ResultType.None)
            return;

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
        if (scriptsToDisable == null)
            return;

        foreach (var s in scriptsToDisable)
        {
            if (s != null)
                s.enabled = enabled;
        }
    }

    IEnumerator LoseSequence()
    {
        yield return new WaitForSeconds(hitDelay);

        if (leftHammer != null) leftHammer.Hit();
        if (rightHammer != null) rightHammer.Hit();

        yield return new WaitForSeconds(respawnDelay);

        RespawnPlayer();
        RespawnMouse();
    }

    void RespawnPlayer()
    {
        if (player == null || respawnPoint == null)
            return;

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

    void RespawnMouse()
    {
        if (mouse == null || mouseStartPoint == null)
            return;

        var mrb = mouse.GetComponent<Rigidbody>();
        if (mrb != null)
        {
            mrb.linearVelocity = Vector3.zero;
            mrb.angularVelocity = Vector3.zero;
        }

        mouse.position = mouseStartPoint.position;
        mouse.rotation = mouseStartPoint.rotation;
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelayAfterEnd);
        ResetRoom();
    }

    public void ResetRoom()
    {
        RespawnMouse();

        MoveGateTo(gateClosedLocalPos, gateLiftDuration);

        EnterWaitingMode();
    }

    void EnterWaitingMode()
    {
        SetScriptsEnabled(false);
        CurrentResult = ResultType.None;
        waitingForPlayer = true;
    }

    void MoveGateTo(Vector3 targetLocalPos, float duration)
    {
        if (gate == null)
            return;

        if (gateMoveRoutine != null)
            StopCoroutine(gateMoveRoutine);

        gateMoveRoutine = StartCoroutine(MoveGateRoutine(targetLocalPos, duration));
    }

    IEnumerator MoveGateRoutine(Vector3 targetLocalPos, float duration)
    {
        Vector3 startPos = gate.localPosition;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            gate.localPosition = Vector3.Lerp(startPos, targetLocalPos, t / duration);
            yield return null;
        }

        gate.localPosition = targetLocalPos;
        gateMoveRoutine = null;
    }
}