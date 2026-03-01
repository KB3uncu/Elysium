using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    public VisualEffect gatherVfx;
    public float gatherDuration = 8f;

    public VisualEffect readyVfx;

    public GameObject shatterVfxPrefab;
    public float shatterLifetime = 2f;

    public MonoBehaviour playerMovementScript;
    public Transform playerRoot;

    public Animator playerAnimator;
    public string punchTrigger = "Punch";
    public float punchLockDuration = 1.0f;
    public string pickupTrigger = "Pickup";
    public float pickupLockDuration = 0.8f;

    public float punchImpactDelay = 0.35f;

    enum State { None, Gathering, Ready, Punching }
    State state = State.None;

    Rigidbody playerRb;
    bool rbWasKinematic;
    Coroutine gatherRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (playerRoot) playerRb = playerRoot.GetComponent<Rigidbody>();
        StopAll();
    }

    void StopAll()
    {
        if (gatherVfx) { gatherVfx.Reinit(); gatherVfx.Stop(); }
        if (readyVfx) { readyVfx.Reinit(); readyVfx.Stop(); }
        state = State.None;
    }

    public void OnGloveEquipped()
    {
        if (state == State.Punching) return;

        if (gatherRoutine != null) StopCoroutine(gatherRoutine);
        gatherRoutine = StartCoroutine(GatherThenReady());
    }

    public void PlayPickupAnim()
    {
        if (playerAnimator == null) return;

        StartCoroutine(PickupSequence());
    }

    IEnumerator PickupSequence()
    {
        LockPlayer();

        playerAnimator.SetFloat("Speed", 0f);
        if (!string.IsNullOrEmpty(pickupTrigger))
            playerAnimator.SetTrigger(pickupTrigger);

        yield return new WaitForSecondsRealtime(pickupLockDuration);

        UnlockPlayer();
    }

    IEnumerator GatherThenReady()
    {
        state = State.Gathering;

        if (readyVfx) readyVfx.Stop();
        if (gatherVfx)
        {
            gatherVfx.Reinit();
            gatherVfx.Play();
        }

        yield return new WaitForSecondsRealtime(gatherDuration);

        if (gatherVfx) gatherVfx.Stop();

        state = State.Ready;

        if (readyVfx)
        {
            readyVfx.Reinit();
            readyVfx.Play();
        }

        gatherRoutine = null;
    }

    public void PunchWall(BreakableWall wall, PlayerGlove glove)
    {
        if (wall == null) return;
        if (state != State.Ready) return;

        StartCoroutine(PunchSequence(wall, glove));
    }

    IEnumerator PunchSequence(BreakableWall wall, PlayerGlove glove)
    {
        state = State.Punching;

        LockPlayer();

        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", 0f);
            if (!string.IsNullOrEmpty(punchTrigger))
                playerAnimator.SetTrigger(punchTrigger);
        }

        yield return new WaitForSecondsRealtime(punchImpactDelay);

        wall.GetShatterHit(out Vector3 p, out Vector3 n);
        PlayShatterAt(p, n);

        wall.FinishBreak(glove);

        float remaining = Mathf.Max(0f, punchLockDuration - punchImpactDelay);
        if (remaining > 0f)
            yield return new WaitForSecondsRealtime(remaining);

        if (readyVfx) readyVfx.Stop();

        UnlockPlayer();
        state = State.None;
    }

    void PlayShatterAt(Vector3 pos, Vector3 normal)
    {
        if (!shatterVfxPrefab) return;

        pos += normal * 0.03f;
        pos.y -= 0.8f;

        Quaternion rot = Quaternion.LookRotation(-normal) * Quaternion.Euler(0f, 90f, 0f);

        GameObject go = Instantiate(shatterVfxPrefab, pos, rot);

        var vfx = go.GetComponentInChildren<VisualEffect>(true);
        if (vfx)
        {
            vfx.gameObject.SetActive(true);
            vfx.Reinit();
            vfx.Play();
        }

        Destroy(go, Mathf.Max(0.1f, shatterLifetime));
    }

    void LockPlayer()
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        if (playerRb != null)
        {
            rbWasKinematic = playerRb.isKinematic;
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }
    }

    void UnlockPlayer()
    {
        if (playerRb != null)
        {
            playerRb.isKinematic = rbWasKinematic;
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;
    }
}