using System.Collections;
using UnityEngine;

public class RouletteGameManager : MonoBehaviour
{
    [Header("Config (ScriptableObject)")]
    public RouletteSO config;

    [Header("Gun Visuals")]
    public GunVisual playerGun;
    public GunVisual enemyGun;

    [Header("Enemy Visual Timing")]
    public float enemyDiceHold = 0.3f;
    public float enemyPickupHold = 0.35f;
    public float enemyAfterShotHold = 0.25f;

    private Revolver revolver;

    private int playerLives;
    private int enemyLives;

    private enum Turn { Player, Enemy }
    private Turn turn; // Bu artýk "bu round'da kim ateþ edecek?" anlamýnda

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

        // Round her zaman zar ile baþlar (player týklar)
        phase = Phase.NeedDiceRoll;
        Debug.Log($"START | P:{playerLives} E:{enemyLives} | bullets:{config.bulletCount}/{config.chamberCount}");
        Debug.Log("Round start: Click PLAYER DICE to roll.");
    }

    // =========================
    // DICE DUEL (Player clicks, Enemy auto rolls, compare)
    // =========================
    public void PlayerRollDice()
    {
        if (gameOver) return;
        if (phase != Phase.NeedDiceRoll) return;

        LastPlayerRoll = Random.Range(1, 7);
        Debug.Log($"PLAYER DICE: {LastPlayerRoll}");

        // Enemy de otomatik atsýn ve karþýlaþtýralým
        StartCoroutine(EnemyRollAndDecideRoutine());
    }

    IEnumerator EnemyRollAndDecideRoutine()
    {
        // Enemy roll
        LastEnemyRoll = Random.Range(1, 7);
        Debug.Log($"ENEMY DICE: {LastEnemyRoll}");
        yield return new WaitForSeconds(enemyDiceHold);

        if (LastPlayerRoll == LastEnemyRoll)
        {
            // Tie -> reroll
            Debug.Log("DICE TIE! Roll again.");
            phase = Phase.NeedDiceRoll;
            yield break;
        }

        if (LastPlayerRoll > LastEnemyRoll)
        {
            // Player kazanýr -> player shoot akýþýna izin ver
            turn = Turn.Player;
            phase = Phase.NeedGunPickup;
            Debug.Log("Player wins dice. Click TABLE GUN to pick up.");
        }
        else
        {
            // Enemy kazanýr -> player hiçbir þey yapamaz, enemy otomatik shoot
            turn = Turn.Enemy;
            phase = Phase.NeedDiceRoll; // player input kilitli kalsýn, round sonunda tekrar zar isteyeceðiz
            Debug.Log("Enemy wins dice. Enemy will shoot.");

            yield return StartCoroutine(EnemyShootRoutine());

            // Enemy shoot bitti -> yeni round
            StartNextRound();
        }
    }

    // =========================
    // PLAYER INPUT ACTIONS
    // =========================
    public void PlayerPickGun()
    {
        if (gameOver) return;
        if (turn != Turn.Player) return;              // enemy kazandýysa engel
        if (phase != Phase.NeedGunPickup) return;

        playerGun?.Pickup();
        phase = Phase.NeedShootTarget;
        Debug.Log("Gun picked. Now click ENEMY to shoot.");
    }

    public void PlayerShoot()
    {
        if (gameOver) return;
        if (turn != Turn.Player) return;              // enemy kazandýysa engel
        if (phase != Phase.NeedShootTarget) return;

        ResolveShot();           // player ateþ eder
        playerGun?.PutDown();

        // Shot bitti -> yeni round
        StartNextRound();
    }

    // =========================
    // ENEMY SHOOT (called when enemy wins dice)
    // =========================
    IEnumerator EnemyShootRoutine()
    {
        // 1) Silahý al
        enemyGun?.Pickup();
        yield return new WaitForSeconds(enemyPickupHold);

        // 2) Ateþ et
        ResolveShot();
        yield return new WaitForSeconds(enemyAfterShotHold);

        // 3) Silahý býrak
        enemyGun?.PutDown();
    }

    // =========================
    // CORE RESOLVE (uses 'turn' as shooter)
    // =========================
    void ResolveShot()
    {
        bool cycleCompleted;
        bool bullet = revolver.Fire(out cycleCompleted);

        if (bullet)
        {
            if (turn == Turn.Player) enemyLives--;
            else playerLives--;

            Debug.Log($"[{turn}] HIT!  P:{playerLives} E:{enemyLives}  (shot {revolver.ShotsThisCycle}/{revolver.ChamberCount})");
        }
        else
        {
            Debug.Log($"[{turn}] MISS. P:{playerLives} E:{enemyLives}  (shot {revolver.ShotsThisCycle}/{revolver.ChamberCount})");
        }

        if (playerLives <= 0) { EndGame("PLAYER DEAD"); return; }
        if (enemyLives <= 0) { EndGame("ENEMY DEAD"); return; }

        if (cycleCompleted)
            Debug.Log("=== CYCLE COMPLETED. Revolver shuffled. ===");
    }

    void StartNextRound()
    {
        if (gameOver) return;

        // Round reset
        turn = Turn.Player;              // bir sonraki round'u yine player zar ile baþlatýyor
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
