using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class RouletteGameManager : MonoBehaviour
{
    [Header("Config (ScriptableObject)")]
    public RouletteSO config;

    [Header("Gun Visuals")]
    public GunVisual playerGun;
    public GunVisual enemyGun;

    [Header("Shield System")]
    public PlayerShield playerShield;
    float shieldDecisionTime = 3f;
    bool waitingForShieldInput = false;

    [Header("Muzzle VFX")]
    public GunMuzzleVFX playerMuzzle;
    public GunMuzzleVFX enemyMuzzle;

    [Header("Dice Displays")]
    public DicePipDisplay playerDiceDisplay;
    public DicePipDisplay enemyDiceDisplay;

    [Header("Lives Display")]
    public LivesDisplay playerLivesDisplay;
    public LivesDisplay enemyLivesDisplay;

    [Header("Enemy Timing")]
    public float enemyDiceHold = 0.3f;
    public float enemyPickupHold = 0.35f;
    public float enemyAfterShotHold = 0.25f;

    private Revolver revolver;

    private int playerLives;
    private int enemyLives;

    [Header("Hit Reactions")]
    public CharacterHitReaction enemyHit;
    public CameraShake cameraShake;

    [Header("Dice Roll Anim")]
    public DiceRollAnim playerDiceAnim;
    public DiceRollAnim enemyDiceAnim;

    [Header("Hit/Miss Display")]
    public HitMissPopup hitMissPopup;

    [Header("ENDGAME - Win Chest")]
    public GameObject chest;

    [Header("ENDGAME - Lose Respawn")]
    public Transform player;          
    public Transform respawnPoint;      

    [Header("ENDGAME - Reset Room Scripts")]
    public MonoBehaviour[] scriptsToReset; 

    [Header("ENDGAME - Timing")]
    public float endDelay = 0.2f;

    [Header("Enemy Dissolve")]
    public Renderer enemyRenderer;

    private Material[] enemyMats;
    private int enemyHitCount = 0;
    private float currentDissolve = 0f;
    private float targetDissolve = 0f;
    public float dissolveSpeed = 0.2f;


    // "Bu round'da kim ate� edecek?"
    private enum Shooter { Player, Enemy }
    private Shooter shooter;

    // Player ak���: Zar -> Silah al -> Ate� et
    private enum Phase { NeedDiceRoll, NeedGunPickup, NeedShootTarget, NeedShieldDecision }
    private Phase phase;

    private bool gameOver;
    public bool IsGameOver => gameOver;

    // bool enemyIsShooting = false;
    bool enemyShotThisRound = false;

    public int LastPlayerRoll { get; private set; }
    public int LastEnemyRoll { get; private set; }

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("RouletteGameManager: Config (RouletteSO) is not assigned!");
            enabled = false;
            return;
        }

        revolver = new Revolver(config.chamberCount, config.bulletCount);

        playerLives = config.maxLives;
        enemyLives = config.maxLives;

        playerLivesDisplay?.SetLives(playerLives);
        enemyLivesDisplay?.SetLives(enemyLives);

        playerGun?.PutDown();
        enemyGun?.PutDown();

        playerDiceDisplay?.SetCount(0);
        enemyDiceDisplay?.SetCount(0);

        playerShield?.GiveShield(); //kalkan pickupımsı

        phase = Phase.NeedDiceRoll;

        chest.SetActive(false);

        if (enemyRenderer != null)
            enemyMats = enemyRenderer.materials;
        currentDissolve = 0f;
        targetDissolve = 0f;
        SetEnemyDissolve(0f);

        Debug.Log($"START | P:{playerLives} E:{enemyLives} | bullets:{config.bulletCount}/{config.chamberCount}");
        Debug.Log("Round start: Click PLAYER DICE to roll.");
    }

    void Update()
    {
        if (currentDissolve != targetDissolve)
        {
            currentDissolve = Mathf.MoveTowards(currentDissolve, targetDissolve, dissolveSpeed * Time.deltaTime);
            SetEnemyDissolve(currentDissolve);
        }
    }

    // =========================
    // DICE DUEL
    // =========================
    public void PlayerRollDice()
    {
        if (gameOver) return;
        if (phase != Phase.NeedDiceRoll) return;

        StopAllCoroutines();

        phase = Phase.NeedGunPickup;

        AudioManager.Instance.PlayDice();
        playerDiceAnim?.PlayRoll();
        StartCoroutine(PlayerRollAfterAnim());
    }

    IEnumerator PlayerRollAfterAnim()
    {
        yield return new WaitForSeconds(0.6f); 

        LastPlayerRoll = Random.Range(1, 7);
        Debug.Log($"PLAYER DICE: {LastPlayerRoll}");
        playerDiceDisplay?.SetCount(LastPlayerRoll);

        StartCoroutine(EnemyRollAndDecideRoutine());
    }

    IEnumerator EnemyRollAndDecideRoutine()
    {
        enemyDiceAnim?.PlayRoll();
        yield return new WaitForSeconds(0.6f);

        LastEnemyRoll = Random.Range(1, 7);
        Debug.Log($"ENEMY DICE: {LastEnemyRoll}");
        enemyDiceDisplay?.SetCount(LastEnemyRoll);

        yield return new WaitForSeconds(enemyDiceHold);

        if (LastPlayerRoll == LastEnemyRoll)
        {
            Debug.Log("DICE TIE! Roll again.");
            phase = Phase.NeedDiceRoll;
            yield break;
        }

        if (LastPlayerRoll > LastEnemyRoll)
        {
            shooter = Shooter.Player;
            phase = Phase.NeedGunPickup;
            Debug.Log("Player wins dice. Click TABLE GUN to pick up.");
        }
        else
        {
            shooter = Shooter.Enemy;
            Debug.Log("Enemy wins dice.");

            if (playerShield != null && playerShield.CanOfferShield())
            {
                StartCoroutine(ShieldDecisionRoutine());
                Debug.Log("Player has shield. Ask player: Use shield?  q = use / r = dont use");
            }
            else
            {
                Debug.Log("Enemy will shoot.");
                yield return StartCoroutine(EnemyShootRoutine());
                StartNextRound();
            }
        }
    }
    IEnumerator ShieldDecisionRoutine()
    {
        waitingForShieldInput = true;
        phase = Phase.NeedShieldDecision;

        Debug.Log("3 saniye içinde kalkan kullanmak için E bas!");

        float timer = 0f;

        while (timer < shieldDecisionTime)
        {
            if (!waitingForShieldInput)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        // süre doldu → shield kullanılmadı
        waitingForShieldInput = false;

        if (playerShield != null)
            playerShield.ChooseToUseShield(false);

        Debug.Log("Süre bitti → enemy ateş ediyor");

        yield return StartCoroutine(EnemyShootRoutine());

        if (!gameOver)
            StartNextRound();
    }
    public void PlayerPressedShield()
    {
        if (gameOver) return;
        if (phase != Phase.NeedShieldDecision) return;
        if (!waitingForShieldInput) return;

        waitingForShieldInput = false;

        if (playerShield != null)
        {
            playerShield.PickUpShield();
            playerShield.ChooseToUseShield(true);
        }

        StartCoroutine(EnemyShootAfterShieldDecision());
    }


    // =========================
    // PLAYER INPUT ACTIONS
    // =========================
    public void PlayerPickGun()
    {
        if (gameOver) return;
        if (shooter != Shooter.Player) return;
        if (phase != Phase.NeedGunPickup) return;

        playerGun?.Pickup();
        phase = Phase.NeedShootTarget;
        Debug.Log("Gun picked. Now click ENEMY to shoot.");
    }

    public void PlayerShoot()
    {
        if (gameOver) return;
        if (shooter != Shooter.Player) return;
        if (phase != Phase.NeedShootTarget) return;

        ResolveShot(Shooter.Player);

        StartCoroutine(PlayerShootDelayRoutine()); 
    }
    IEnumerator PlayerShootDelayRoutine()
    {
        yield return new WaitForSeconds(0.8f); // 🔥 süreyi buradan ayarla

        playerGun?.PutDown();

        StartNextRound();
    }
    public void PlayerChooseShieldYes()
    {
        if (gameOver) return;
        if (shooter != Shooter.Enemy) return;
        if (phase != Phase.NeedShieldDecision) return;
        if (playerShield == null) return;

        playerShield.PickUpShield();
        playerShield.ChooseToUseShield(true);

        StartCoroutine(EnemyShootAfterShieldDecision());
    }

    public void PlayerChooseShieldNo()
    {
        if (gameOver) return;
        if (shooter != Shooter.Enemy) return;
        if (phase != Phase.NeedShieldDecision) return;
        if (playerShield == null) return;

        playerShield.ChooseToUseShield(false);
        StartCoroutine(EnemyShootAfterShieldDecision());
    }

    IEnumerator EnemyShootAfterShieldDecision()
    {
        phase = Phase.NeedGunPickup;

        bool usedShield = playerShield != null && playerShield.willUseThisShot;

        yield return StartCoroutine(EnemyShootRoutine());

        if (playerShield != null && usedShield && playerShield.hasShield)
        {
           yield return StartCoroutine(ShieldReturnDelay());
        }

        if (!gameOver)
            StartNextRound();
    }
    IEnumerator ShieldReturnDelay()
    {
        yield return new WaitForSeconds(0.8f); // 🔥 süreyi buradan ayarla

        playerShield.PutShieldBackToTable();
    }

    // =========================
    // ENEMY SHOOT (AUTO)
    // =========================
    IEnumerator EnemyShootRoutine()
    {
        if (enemyShotThisRound) yield break;

        enemyShotThisRound = true;

        enemyGun?.Pickup();
        yield return new WaitForSeconds(enemyPickupHold);

        ResolveShot(Shooter.Enemy);

        yield return new WaitForSeconds(enemyAfterShotHold);

        enemyGun?.PutDown();
    }

    // =========================
    // CORE RESOLVE
    // =========================
    void ResolveShot(Shooter who)
    {
        bool cycleCompleted;
        bool bullet = revolver.Fire(out cycleCompleted);

        bool shieldBlockedBullet = false;

        if (who == Shooter.Enemy && playerShield != null)
        {
            shieldBlockedBullet = playerShield.ConsumeShieldAgainstShot(bullet);
        }

        if (bullet)
        {
            // 🔊 GERÇEK MERMİ
            AudioManager.Instance.PlayGun();

            hitMissPopup?.Show(true);
            cameraShake?.Play();

            if (who == Shooter.Player)
            {
                playerMuzzle?.PlayOnce();
                enemyLives--;
                enemyHit?.PlayFallAndStandUp();
            }
            else
            {
                enemyMuzzle?.PlayOnce();

                if (!shieldBlockedBullet)
                {
                    playerLives--;
                    AudioManager.Instance.PlayHit();
                }
                else
                {
                    AudioManager.Instance.PlayShield();

                    Debug.Log("Shield blocked the bullet!");
                }
            }

            playerLivesDisplay?.SetLives(playerLives);
            enemyLivesDisplay?.SetLives(enemyLives);
        }
        else
        {
            // 🔊 BOŞ MERMİ
            AudioManager.Instance.PlayEmpty();

            hitMissPopup?.Show(false);
        }

        if (playerLives <= 0) { EndGame("PLAYER DEAD"); return; }
        if (enemyLives <= 0) { EndGame("ENEMY DEAD"); return; }

        if (cycleCompleted)
            Debug.Log("=== CYCLE COMPLETED. Revolver shuffled. ===");
    }

    void SetEnemyDissolve(float value)
    {
        if (enemyMats == null || enemyMats.Length == 0) return;

        for (int i = 0; i < enemyMats.Length; i++)
        {
            if (enemyMats[i] != null && enemyMats[i].HasProperty("_DissolveAmount"))
            {
                enemyMats[i].SetFloat("_DissolveAmount", value);
            }
        }
    }

    void UpdateEnemyDissolve()
    {
        enemyHitCount++;

        if (enemyHitCount == 1)
            targetDissolve = 0.2f;
        else if (enemyHitCount == 2)
            targetDissolve = 0.3f;
        else if (enemyHitCount >= 3)
            targetDissolve = 0.4f;
    }


    void ResetRoomAndGame()
    {
        if (scriptsToReset != null)
        {
            foreach (var s in scriptsToReset)
                if (s != null) s.enabled = false;

            foreach (var s in scriptsToReset)
                if (s != null) s.enabled = true;
        }

        revolver = new Revolver(config.chamberCount, config.bulletCount);

        playerLives = config.maxLives;
        enemyLives = config.maxLives;

        playerLivesDisplay?.SetLives(playerLives);
        enemyLivesDisplay?.SetLives(enemyLives);

        playerGun?.PutDown();
        enemyGun?.PutDown();

        playerDiceDisplay?.SetCount(0);
        enemyDiceDisplay?.SetCount(0);

        enemyHitCount = 0;
        currentDissolve = 0f;
        targetDissolve = 0f;
        SetEnemyDissolve(0f);

        if (playerShield != null)
        {
            playerShield.GiveShield(); 
        }

        phase = Phase.NeedDiceRoll;
        gameOver = false;

        Debug.Log("RESET TAMAMLANDI");
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


    void StartNextRound()
    {
        StopAllCoroutines();
        if (gameOver) return;

        playerGun?.PutDown();
        enemyGun?.PutDown();

        // �stersen round ba��nda s�f�rla
        playerDiceDisplay?.SetCount(0);
        enemyDiceDisplay?.SetCount(0);

        if (playerShield != null)
            playerShield.ClearDecision();

        enemyShotThisRound = false;

        phase = Phase.NeedDiceRoll;
        Debug.Log("Round start: Click PLAYER DICE to roll.");

    }

    void EndGame(string reason)
    {
        gameOver = true;
        StopAllCoroutines();
        Debug.Log("GAME OVER: " + reason);

        bool playerWon = enemyLives <= 0;
        bool enemyWon = playerLives <= 0;

        if (playerWon)
        {
            chest.SetActive(true);
            return;
        }

        if (enemyWon)
        {
            StartCoroutine(LoseEndRoutine());
            return;
        }
    }

    IEnumerator LoseEndRoutine()
    {
        yield return new WaitForSeconds(endDelay);

        RespawnPlayer();
        ResetRoomAndGame();
    }
}
