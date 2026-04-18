using System.Collections;
using UnityEngine;

public class MonsterEscapeSequence : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public MonoBehaviour playerMovementScript;
    public Transform playerCamera;

    [Header("Monster")]
    public GameObject monsterPrefab;
    public Transform monsterSpawnPoint;
    public float monsterMoveSpeed = 8f;
    public float stopDistanceToPlayer = 1.8f;
    public float rotationSpeed = 10f;

    [Header("Monster Animation")]
    public Animator monsterAnimator;
    public string runTrigger = "Run";

    [Header("Cinematic")]
    public float spawnDelay = 0.15f;
    public float cinematicLockDuration = 2.5f;
    public bool rotatePlayerToMonsterAtStart = true;
    public float playerRotateSpeed = 6f;

    [Header("Monster Breakable Walls")]
    public BreakableWall[] monsterBreakWalls;
    public float breakDistance = 2.0f;
    public float monsterBreakForceMultiplier = 1.5f;

    private int currentWallIndex = 0;

    [Header("Camera Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;
    public float shakeFrequency = 35f;

    [Header("Optional Sound")]
    public AudioSource rumbleAudio;
    public float rumbleFadeInSpeed = 1.5f;
    public float rumbleFadeOutSpeed = 2f;

    private GameObject spawnedMonster;
    private Transform monsterTransform;
    private Vector3 cameraOriginalLocalPos;
    private bool sequenceStarted = false;
    private bool controlReturned = false;
    private Coroutine shakeRoutine;

    void Awake()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (playerCamera != null)
            cameraOriginalLocalPos = playerCamera.localPosition;
    }

    void Update()
    {
        if (!sequenceStarted) return;
        if (monsterTransform == null) return;
        if (player == null) return;

        MoveMonster();

        CheckMonsterWallBreak();

        if (!controlReturned && rotatePlayerToMonsterAtStart)
            RotatePlayerTowardMonster();
    }

    public void StartSequence()
    {
        if (sequenceStarted) return;
        StartCoroutine(SequenceRoutine());
    }

    IEnumerator SequenceRoutine()
    {
        sequenceStarted = true;
        controlReturned = false;
        currentWallIndex = 0;

        SetPlayerControl(false);

        if (rumbleAudio != null)
        {
            rumbleAudio.volume = 0f;
            rumbleAudio.Play();
        }

        yield return new WaitForSeconds(spawnDelay);

        SpawnMonster();

        if (monsterAnimator != null && !string.IsNullOrEmpty(runTrigger))
            monsterAnimator.SetTrigger(runTrigger);

        TriggerShake(shakeDuration, shakeMagnitude);

        float timer = 0f;
        while (timer < cinematicLockDuration)
        {
            timer += Time.deltaTime;

            if (rumbleAudio != null)
            {
                rumbleAudio.volume = Mathf.MoveTowards(
                    rumbleAudio.volume,
                    1f,
                    rumbleFadeInSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        controlReturned = true;
        SetPlayerControl(true);
    }

    void SpawnMonster()
    {
        if (monsterPrefab == null || monsterSpawnPoint == null)
        {
            Debug.LogWarning("MonsterEscapeSequence: Monster Prefab veya Spawn Point atanmadý.");
            return;
        }

        spawnedMonster = Instantiate(
            monsterPrefab,
            monsterSpawnPoint.position,
            monsterSpawnPoint.rotation
        );

        monsterTransform = spawnedMonster.transform;

        if (monsterAnimator == null)
            monsterAnimator = spawnedMonster.GetComponentInChildren<Animator>();
    }

    void MoveMonster()
    {
        Vector3 targetPos = player.position;
        targetPos.y = monsterTransform.position.y;

        Vector3 dir = targetPos - monsterTransform.position;
        float distance = dir.magnitude;

        if (distance > stopDistanceToPlayer)
        {
            Vector3 moveDir = dir.normalized;
            monsterTransform.position += moveDir * monsterMoveSpeed * Time.deltaTime;

            if (moveDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                monsterTransform.rotation = Quaternion.Slerp(
                    monsterTransform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        if (rumbleAudio != null)
        {
            float targetVolume = distance < 10f ? 1f : 0.6f;
            float fadeSpeed = controlReturned ? rumbleFadeOutSpeed : rumbleFadeInSpeed;
            rumbleAudio.volume = Mathf.MoveTowards(rumbleAudio.volume, targetVolume, fadeSpeed * Time.deltaTime);
        }
    }

    void CheckMonsterWallBreak()
    {
        if (monsterBreakWalls == null || monsterBreakWalls.Length == 0) return;
        if (monsterTransform == null) return;

        while (currentWallIndex < monsterBreakWalls.Length)
        {
            BreakableWall targetWall = monsterBreakWalls[currentWallIndex];

            if (targetWall == null)
            {
                currentWallIndex++;
                continue;
            }

            Vector3 monsterPos = monsterTransform.position;
            Vector3 wallPos = targetWall.transform.position;

            // sadece yatay mesafe kontrolü
            monsterPos.y = 0f;
            wallPos.y = 0f;

            float distance = Vector3.Distance(monsterPos, wallPos);

            if (distance > breakDistance)
                break;

            Vector3 forceDir = (targetWall.transform.position - monsterTransform.position).normalized;
            forceDir += new Vector3(0f, 0.2f, 0f);
            forceDir.Normalize();

            targetWall.BreakFromWorld(forceDir, monsterBreakForceMultiplier);
            TriggerShake(shakeDuration, shakeMagnitude);

            currentWallIndex++;
        }
    }

    void RotatePlayerTowardMonster()
    {
        if (player == null || monsterTransform == null) return;

        Vector3 dir = monsterTransform.position - player.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        player.rotation = Quaternion.Slerp(player.rotation, targetRot, playerRotateSpeed * Time.deltaTime);
    }

    void SetPlayerControl(bool enabled)
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = enabled;
    }

    void TriggerShake(float duration, float magnitude)
    {
        if (playerCamera == null) return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeCameraRoutine(duration, magnitude));
    }

    IEnumerator ShakeCameraRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x = Mathf.Sin(Time.time * shakeFrequency) * magnitude;
            float y = Mathf.Cos(Time.time * shakeFrequency * 1.13f) * magnitude * 0.7f;

            playerCamera.localPosition = cameraOriginalLocalPos + new Vector3(x, y, 0f);
            yield return null;
        }

        playerCamera.localPosition = cameraOriginalLocalPos;
    }
}