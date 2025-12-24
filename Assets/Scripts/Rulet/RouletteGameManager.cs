using System.Collections;
using UnityEngine;

public class RouletteGameManager : MonoBehaviour
{
    [Header("Config (ScriptableObject)")]
    public RouletteSO config;

    [Header("Gun Visuals")]
    public GunVisual playerGun;
    public GunVisual enemyGun;

    [Header("Dice Displays")]
    public DicePipDisplay playerDiceDisplay;
    public DicePipDisplay enemyDiceDisplay;

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


    // "Bu round'da kim ateþ edecek?"
    private enum Shooter { Player, Enemy }
    private Shooter shooter;

    // Player akýþý: Zar -> Silah al -> Ateþ et
    private enum Phase { NeedDiceRoll, NeedGunPickup, NeedShootTarget }
    private Phase phase;

    private bool gameOver;
    public bool IsGameOver => gameOver;

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

        playerGun?.PutDown();
        enemyGun?.PutDown();

        playerDiceDisplay?.SetCount(0);
        enemyDiceDisplay?.SetCount(0);

        phase = Phase.NeedDiceRoll;

        Debug.Log($"START | P:{playerLives} E:{enemyLives} | bullets:{config.bulletCount}/{config.chamberCount}");
        Debug.Log("Round start: Click PLAYER DICE to roll.");
    }

    // =========================
    // DICE DUEL
    // =========================
    public void PlayerRollDice()
    {
        if (gameOver) return;
        if (phase != Phase.NeedDiceRoll) return;

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
            Debug.Log("Enemy wins dice. Enemy will shoot.");

            yield return StartCoroutine(EnemyShootRoutine());
            StartNextRound();
        }
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

        playerGun?.PutDown();

        StartNextRound();
    }

    // =========================
    // ENEMY SHOOT (AUTO)
    // =========================
    IEnumerator EnemyShootRoutine()
    {
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

        if (bullet)
        {
            cameraShake?.Play();

            if (who == Shooter.Player)
            {
                enemyLives--;
                enemyHit?.PlayFallAndStandUp();   // sadece enemy düþsün
            }
            else
            {
                playerLives--; // player can azalýr ama animasyon yok
            }

            Debug.Log($"[{who}] HIT!  P:{playerLives} E:{enemyLives}  (shot {revolver.ShotsThisCycle}/{revolver.ChamberCount})");
        }


        if (playerLives <= 0) { EndGame("PLAYER DEAD"); return; }
        if (enemyLives <= 0) { EndGame("ENEMY DEAD"); return; }

        if (cycleCompleted)
            Debug.Log("=== CYCLE COMPLETED. Revolver shuffled. ===");
    }


    void StartNextRound()
    {
        if (gameOver) return;

        playerGun?.PutDown();
        enemyGun?.PutDown();

        // Ýstersen round baþýnda sýfýrla
        playerDiceDisplay?.SetCount(0);
        enemyDiceDisplay?.SetCount(0);

        phase = Phase.NeedDiceRoll;
        Debug.Log("Round start: Click PLAYER DICE to roll.");
    }

    void EndGame(string reason)
    {
        gameOver = true;
        StopAllCoroutines();
        Debug.Log("GAME OVER: " + reason);
    }
}
