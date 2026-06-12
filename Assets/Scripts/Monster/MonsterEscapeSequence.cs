using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterEscapeSequence : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public MonoBehaviour playerMovementScript;
    public Transform playerCamera;

    [Header("Monster Spawn")]
    public GameObject monsterPrefab;
    public Transform monsterSpawnPoint;

    [Header("Movement")]
    public float moveSpeed = 7f;
    public float stopDistanceToPlayer = 1.8f;
    public float rotationSpeed = 6f;

    [Header("Ground Follow")]
    public bool followGround = true;
    public LayerMask groundMask = ~0;
    public float groundRayStartHeight = 8f;
    public float groundRayDistance = 30f;
    public float groundOffset = 0.05f;
    public float verticalSnapSpeed = 12f;

    [Header("Search Mode")]
    public bool enableSearchMode = true;
    public float searchModeDistance = 35f;
    public float reacquirePlayerDistance = 25f;

    public Collider searchAreaCollider;
    public Transform fallbackSearchCenter;
    public Vector2 fallbackSearchAreaSize = new Vector2(120f, 105f);

    public float searchPointReachDistance = 2f;
    public float searchPointChangeInterval = 4f;
    public float minSearchPointDistance = 8f;
    public int randomPointAttempts = 25;

    public LayerMask searchObstacleMask = 0;
    public float searchObstacleCheckRadius = 0.7f;

    [Header("Animation")]
    public Animator monsterAnimator;
    public string walkBoolName = "Walk";

    [Header("Cinematic")]
    public float spawnDelay = 0.15f;
    public float cinematicLockDuration = 2.5f;
    public bool rotatePlayerToMonsterAtStart = true;
    public float playerRotateSpeed = 6f;

    [Header("Monster Breakable Walls")]
    public BreakableWall[] monsterBreakWalls;
    public float breakDistance = 2.5f;
    public float monsterBreakForceMultiplier = 1.5f;

    [Header("Camera Shake")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;
    public float shakeFrequency = 35f;

    [Header("Optional Sound")]
    public AudioSource rumbleAudio;
    public float rumbleFadeInSpeed = 1.5f;
    public float rumbleFadeOutSpeed = 2f;

    [Header("Lose Screen")]
    public GameObject losePanel;

    [Tooltip("Canavar bu mesafeye girince oyuncu yakalanýr. Stop Distance deðerinden küçük olmasýn.")]
    public float catchDistance = 1.9f;

    [Tooltip("Kapalýysa sinematik kilit sýrasýnda oyuncu yakalanmaz.")]
    public bool canCatchDuringCinematic = false;

    public bool pauseGameOnLose = true;
    public bool unlockCursorOnLose = true;

    private GameObject spawnedMonster;
    private Transform monsterTransform;
    private Vector3 cameraOriginalLocalPos;

    private bool sequenceStarted = false;
    private bool controlReturned = false;
    private bool playerCaught = false;

    private int currentWallIndex = 0;
    private Coroutine shakeRoutine;

    private enum MonsterMode
    {
        Chase,
        Search
    }

    private MonsterMode currentMode = MonsterMode.Chase;
    private Vector3 currentSearchTarget;
    private bool hasSearchTarget = false;
    private float searchTimer = 0f;

    void Awake()
    {
        Time.timeScale = 1f;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (playerCamera != null)
            cameraOriginalLocalPos = playerCamera.localPosition;

        if (losePanel != null)
            losePanel.SetActive(false);
    }

    void Update()
    {
        if (!sequenceStarted) return;
        if (monsterTransform == null) return;
        if (player == null) return;
        if (playerCaught) return;

        MoveMonster();
        CheckMonsterWallBreak();
        CheckPlayerCaught();

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
        playerCaught = false;

        currentWallIndex = 0;
        currentMode = MonsterMode.Chase;
        hasSearchTarget = false;
        searchTimer = 0f;

        if (losePanel != null)
            losePanel.SetActive(false);

        SetPlayerControl(false);

        if (rumbleAudio != null)
        {
            rumbleAudio.volume = 0f;
            rumbleAudio.Play();
        }

        yield return new WaitForSeconds(spawnDelay);

        SpawnMonster();

        SetWalkAnimation(true);
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

        SnapMonsterToGroundInstant();

        if (monsterAnimator == null)
            monsterAnimator = spawnedMonster.GetComponentInChildren<Animator>();
    }

    void MoveMonster()
    {
        float distanceToPlayer = GetFlatDistance(monsterTransform.position, player.position);

        UpdateMonsterMode(distanceToPlayer);

        if (currentMode == MonsterMode.Search)
        {
            MoveMonsterSearch();
        }
        else
        {
            MoveMonsterToTarget(player.position, stopDistanceToPlayer);
        }

        UpdateRumble(distanceToPlayer);
    }

    void UpdateMonsterMode(float distanceToPlayer)
    {
        if (!enableSearchMode || !controlReturned)
        {
            currentMode = MonsterMode.Chase;
            return;
        }

        if (currentMode == MonsterMode.Chase)
        {
            if (distanceToPlayer >= searchModeDistance)
            {
                currentMode = MonsterMode.Search;
                PickNewSearchTarget();
            }
        }
        else
        {
            if (distanceToPlayer <= reacquirePlayerDistance)
            {
                currentMode = MonsterMode.Chase;
                hasSearchTarget = false;
            }
        }
    }

    void MoveMonsterSearch()
    {
        searchTimer -= Time.deltaTime;

        bool needNewPoint = false;

        if (!hasSearchTarget)
            needNewPoint = true;

        if (hasSearchTarget)
        {
            float distanceToSearchPoint = GetFlatDistance(monsterTransform.position, currentSearchTarget);

            if (distanceToSearchPoint <= searchPointReachDistance)
                needNewPoint = true;

            if (searchTimer <= 0f)
                needNewPoint = true;
        }

        if (needNewPoint)
            PickNewSearchTarget();

        if (hasSearchTarget)
            MoveMonsterToTarget(currentSearchTarget, searchPointReachDistance);
    }

    void PickNewSearchTarget()
    {
        hasSearchTarget = TryGetRandomSearchPoint(out currentSearchTarget);
        searchTimer = searchPointChangeInterval;
    }

    bool TryGetRandomSearchPoint(out Vector3 point)
    {
        Bounds bounds;

        if (searchAreaCollider != null)
        {
            bounds = searchAreaCollider.bounds;
        }
        else
        {
            Vector3 center = fallbackSearchCenter != null ? fallbackSearchCenter.position : Vector3.zero;
            bounds = new Bounds(center, new Vector3(fallbackSearchAreaSize.x, 20f, fallbackSearchAreaSize.y));
        }

        for (int i = 0; i < randomPointAttempts; i++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float z = Random.Range(bounds.min.z, bounds.max.z);

            Vector3 probe = new Vector3(x, bounds.max.y + groundRayStartHeight, z);

            if (!TryGetGroundY(probe, out float groundY))
                continue;

            Vector3 candidate = new Vector3(x, groundY + groundOffset, z);

            if (GetFlatDistance(monsterTransform.position, candidate) < minSearchPointDistance)
                continue;

            if (searchObstacleMask.value != 0)
            {
                Vector3 checkPos = candidate + Vector3.up * 0.7f;

                if (Physics.CheckSphere(
                    checkPos,
                    searchObstacleCheckRadius,
                    searchObstacleMask,
                    QueryTriggerInteraction.Ignore))
                {
                    continue;
                }
            }

            point = candidate;
            return true;
        }

        point = monsterTransform.position;
        return false;
    }

    void MoveMonsterToTarget(Vector3 targetWorldPosition, float stopDistance)
    {
        Vector3 monsterFlat = monsterTransform.position;
        Vector3 targetFlat = targetWorldPosition;

        monsterFlat.y = 0f;
        targetFlat.y = 0f;

        Vector3 flatDir = targetFlat - monsterFlat;
        float flatDistance = flatDir.magnitude;

        if (flatDistance > stopDistance)
        {
            Vector3 nextFlat = Vector3.MoveTowards(
                monsterFlat,
                targetFlat,
                moveSpeed * Time.deltaTime
            );

            float nextY = monsterTransform.position.y;

            if (followGround)
            {
                Vector3 groundProbe = new Vector3(
                    nextFlat.x,
                    monsterTransform.position.y,
                    nextFlat.z
                );

                if (TryGetGroundY(groundProbe, out float groundY))
                {
                    nextY = Mathf.MoveTowards(
                        monsterTransform.position.y,
                        groundY + groundOffset,
                        verticalSnapSpeed * Time.deltaTime
                    );
                }
            }
            else
            {
                nextY = Mathf.MoveTowards(
                    monsterTransform.position.y,
                    targetWorldPosition.y,
                    verticalSnapSpeed * Time.deltaTime
                );
            }

            monsterTransform.position = new Vector3(nextFlat.x, nextY, nextFlat.z);
        }

        if (flatDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir.normalized, Vector3.up);

            monsterTransform.rotation = Quaternion.Slerp(
                monsterTransform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    bool TryGetGroundY(Vector3 position, out float groundY)
    {
        groundY = position.y;

        Vector3 origin = position + Vector3.up * groundRayStartHeight;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            Vector3.down,
            groundRayDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        if (hits == null || hits.Length == 0)
            return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            Transform hitTransform = hits[i].transform;

            if (monsterTransform != null && hitTransform.IsChildOf(monsterTransform))
                continue;

            if (player != null && hitTransform.IsChildOf(player))
                continue;

            groundY = hits[i].point.y;
            return true;
        }

        return false;
    }

    void SnapMonsterToGroundInstant()
    {
        if (!followGround || monsterTransform == null) return;

        if (TryGetGroundY(monsterTransform.position, out float groundY))
        {
            Vector3 p = monsterTransform.position;
            p.y = groundY + groundOffset;
            monsterTransform.position = p;
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

            monsterPos.y = 0f;
            wallPos.y = 0f;

            float distance = Vector3.Distance(monsterPos, wallPos);

            if (distance > breakDistance)
                break;

            Vector3 forceDir = (targetWall.transform.position - monsterTransform.position).normalized;
            forceDir += new Vector3(0f, 0.25f, 0f);
            forceDir.Normalize();

            BreakWallByMonster(targetWall, forceDir);

            TriggerShake(shakeDuration, shakeMagnitude);

            currentWallIndex++;
        }
    }

    void BreakWallByMonster(BreakableWall wall, Vector3 forceDir)
    {
        if (wall == null) return;

        var method = wall.GetType().GetMethod(
            "BreakFromWorld",
            new System.Type[] { typeof(Vector3), typeof(float) }
        );

        if (method != null)
        {
            method.Invoke(wall, new object[] { forceDir, monsterBreakForceMultiplier });
        }
        else
        {
            wall.FinishBreak(null);
        }
    }

    void CheckPlayerCaught()
    {
        if (playerCaught) return;
        if (monsterTransform == null || player == null) return;

        if (!controlReturned && !canCatchDuringCinematic)
            return;

        float distance = GetFlatDistance(monsterTransform.position, player.position);

        if (distance <= catchDistance)
            LoseGame();
    }

    void LoseGame()
    {
        if (playerCaught) return;

        playerCaught = true;

        SetPlayerControl(false);
        SetWalkAnimation(false);

        if (rumbleAudio != null)
            rumbleAudio.Stop();

        if (losePanel != null)
            losePanel.SetActive(true);

        if (unlockCursorOnLose)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (pauseGameOnLose)
            Time.timeScale = 0f;
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    float GetFlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void UpdateRumble(float flatDistance)
    {
        if (rumbleAudio == null) return;

        float targetVolume = flatDistance < 10f ? 1f : 0.6f;
        float fadeSpeed = controlReturned ? rumbleFadeOutSpeed : rumbleFadeInSpeed;

        rumbleAudio.volume = Mathf.MoveTowards(
            rumbleAudio.volume,
            targetVolume,
            fadeSpeed * Time.deltaTime
        );
    }

    void RotatePlayerTowardMonster()
    {
        Vector3 dir = monsterTransform.position - player.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);

        player.rotation = Quaternion.Slerp(
            player.rotation,
            targetRot,
            playerRotateSpeed * Time.deltaTime
        );
    }

    void SetPlayerControl(bool enabled)
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = enabled;
    }

    void SetWalkAnimation(bool walking)
    {
        if (monsterAnimator == null) return;
        if (string.IsNullOrEmpty(walkBoolName)) return;

        monsterAnimator.SetBool(walkBoolName, walking);
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

    void OnDrawGizmosSelected()
    {
        if (searchAreaCollider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(searchAreaCollider.bounds.center, searchAreaCollider.bounds.size);
        }
        else
        {
            Vector3 center = fallbackSearchCenter != null ? fallbackSearchCenter.position : Vector3.zero;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(center, new Vector3(fallbackSearchAreaSize.x, 2f, fallbackSearchAreaSize.y));
        }

        Gizmos.color = Color.red;

        if (monsterTransform != null)
            Gizmos.DrawWireSphere(monsterTransform.position, catchDistance);
    }
}