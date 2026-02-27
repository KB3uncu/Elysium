using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    public VisualEffect[] effects;
    public float[] durations;

    public MonoBehaviour playerMovementScript;
    public Transform playerRoot;

    public float overlapSeconds = 0.1f;
    public float postShatterHold = 2f;

    public GameObject shatterVfxPrefab;

    bool isRunning = false;

    Rigidbody playerRb;
    bool rbWasKinematic;

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
        StopAllEffectsHard();
    }

    void StopAllEffectsHard()
    {
        if (effects == null) return;

        foreach (var vfx in effects)
        {
            if (!vfx) continue;
            vfx.Reinit();
            vfx.Stop();
        }
    }

    public void StartWallBreakSequence(BreakableWall wall, PlayerGlove glove)
    {
        if (isRunning || wall == null) return;
        StartCoroutine(RunSequence(wall, glove));
    }

    IEnumerator RunSequence(BreakableWall wall, PlayerGlove glove)
    {
        isRunning = true;

        LockPlayer();
        StopAllEffectsHard();

        int n = effects != null ? effects.Length : 0;

        if (durations == null || durations.Length != n)
        {
            durations = new float[n];
            for (int i = 0; i < n; i++) durations[i] = 8f;
        }

        if (n > 0 && effects[0]) effects[0].Play();

        for (int i = 0; i < n; i++)
        {
            float d = Mathf.Max(0f, durations[i]);

            if (i < n - 1)
            {
                float waitBeforeNext = Mathf.Max(0f, d - overlapSeconds);
                yield return new WaitForSecondsRealtime(waitBeforeNext);

                if (effects[i + 1]) effects[i + 1].Play();

                float remaining = Mathf.Max(0f, d - waitBeforeNext);
                yield return new WaitForSecondsRealtime(remaining);

                if (effects[i]) effects[i].Stop();
            }
            else
            {
                yield return new WaitForSecondsRealtime(d);
            }
        }

        wall.FinishBreak(glove);

        PlayShatterAt(wall.transform.position);

        yield return new WaitForSecondsRealtime(postShatterHold);

        for (int i = 0; i < n; i++)
            if (effects[i]) effects[i].Stop();

        UnlockPlayer();
        isRunning = false;
    }

    void PlayShatterAt(Vector3 pos)
    {
        if (!shatterVfxPrefab) return;

        GameObject go = Instantiate(shatterVfxPrefab, pos, Quaternion.identity);
        var vfx = go.GetComponent<VisualEffect>();
        if (vfx)
        {
            vfx.Reinit();
            vfx.Play();
        }
        Destroy(go, postShatterHold + 0.2f);
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