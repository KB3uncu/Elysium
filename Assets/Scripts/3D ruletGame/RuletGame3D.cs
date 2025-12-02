using System.Collections;
using UnityEngine;
using TMPro;   // TextMeshPro için

public class RuletGame3D : MonoBehaviour
{
    [Header("Can Ayarlarý")]
    public int playerHP = 3;
    public int enemyHP = 3;

    [Header("Body Transformlarý")]
    public Transform playerBody;   // Player düþme animasyonu için
    public Transform enemyBody;    // Enemy düþme animasyonu için

    [Header("Yere Düþme Ayarlarý")]
    public float knockDownAngle = 80f;
    public float knockDuration = 0.2f;
    public float standUpDelay = 0.6f;

    [Header("Zar Ayarlarý")]
    public Transform[] diceObjects;       // Ortadaki 2 zar
    public float diceSpinDuration = 0.7f; // Zarlarýn kaç saniye döneceði
    public float diceSpinSpeed = 720f;    // Saniyede kaç derece dönecek

    [Header("UI (TMP)")]
    public TMP_Text playerRollText;       // Player zar sonucu
    public TMP_Text enemyRollText;        // Enemy zar sonucu

    [Header("Silah")]
    public Transform gunObject;           // Player'ýn týklayacaðý silah (masada / elde)

    [Header("Mermi Sistemi")]
    public int chamberSize = 6;           // Tambur hazne sayýsý
    public int bulletsInChamber = 2;      // Toplam dolu mermi sayýsý (2)

    // PLAYER tamburu
    private bool[] playerChambers;
    private int playerChamberIndex = 0;
    private int playerShotsFired = 0;     // Player kaç kere tetiðe bastý

    // ENEMY tamburu
    private bool[] enemyChambers;
    private int enemyChamberIndex = 0;
    private int enemyShotsFired = 0;      // Enemy kaç kere tetiðe bastý

    // Zar sonuçlarý
    private int lastPlayerRoll = 0;
    private int lastEnemyRoll = 0;

    // Oyun durumlarý
    private bool canRoll = true;          // Zara týklanabilir mi?
    private bool playerTurnToShoot = false;

    private enum TurnState { Idle, Rolling, Shooting }
    private TurnState currentState = TurnState.Idle;

    void Start()
    {
        currentState = TurnState.Idle;
        canRoll = true;

        if (playerRollText != null) playerRollText.gameObject.SetActive(false);
        if (enemyRollText != null) enemyRollText.gameObject.SetActive(false);

        // Oyun baþýnda tamburlarý kar
        InitPlayerRevolver();
        InitEnemyRevolver();
    }

    void Update()
    {
        // 1) Zar atma – SADECE zarlara týklayýnca
        if (canRoll && Input.GetMouseButtonDown(0))
        {
            if (ClickedOnDice())
            {
                StartCoroutine(RollBothRoutine());
            }
        }

        // 2) Player ateþ – sýra bizdeyken, silaha týklanýnca ateþ
        if (currentState == TurnState.Shooting &&
            playerTurnToShoot &&
            Input.GetMouseButtonDown(0))
        {
            if (ClickedOnGun())
            {
                PlayerShoot();
            }
        }
    }

    // ---------------- TIKLAMA KONTROLLERÝ ----------------

    bool ClickedOnDice()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Trigger collider'larý yok say (rulet alanýndaki Box Trigger engellemesin)
        if (Physics.Raycast(ray, out hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {
            foreach (var d in diceObjects)
            {
                if (d == null) continue;

                if (hit.transform == d || hit.transform.IsChildOf(d))
                    return true;
            }
        }

        return false;
    }

    bool ClickedOnGun()
    {
        if (gunObject == null) return false;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == gunObject || hit.transform.IsChildOf(gunObject))
                return true;
        }

        return false;
    }

    // ---------------- ZAR ANÝMASYONU ----------------

    IEnumerator SpinDiceRoutine()
    {
        float elapsed = 0f;

        while (elapsed < diceSpinDuration)
        {
            elapsed += Time.deltaTime;

            foreach (var d in diceObjects)
            {
                if (d == null) continue;

                d.Rotate(Vector3.up, diceSpinSpeed * Time.deltaTime, Space.World);
            }

            yield return null;
        }
    }

    // ---------------- TEK TIKLAMADA PLAYER + ENEMY ZARI ----------------

    IEnumerator RollBothRoutine()
    {
        canRoll = false;
        currentState = TurnState.Rolling;
        playerTurnToShoot = false;

        // UI baþta tamamen kapalý
        if (playerRollText != null) playerRollText.gameObject.SetActive(false);
        if (enemyRollText != null) enemyRollText.gameObject.SetActive(false);

        // --- PLAYER ROLL ---
        yield return StartCoroutine(SpinDiceRoutine());

        lastPlayerRoll = Random.Range(1, 7);
        Debug.Log("PLAYER roll: " + lastPlayerRoll);

        if (playerRollText != null)
        {
            playerRollText.gameObject.SetActive(true);
            playerRollText.text = "Player: " + lastPlayerRoll;
        }

        if (enemyRollText != null)
        {
            enemyRollText.gameObject.SetActive(false);
            enemyRollText.text = "Enemy: -";
        }

        // --- ENEMY ROLL ---
        yield return StartCoroutine(SpinDiceRoutine());

        lastEnemyRoll = Random.Range(1, 7);
        Debug.Log("ENEMY roll: " + lastEnemyRoll);

        if (enemyRollText != null)
        {
            enemyRollText.gameObject.SetActive(true);
            enemyRollText.text = "Enemy: " + lastEnemyRoll;
        }

        // Kazananý belirle
        DecideWinnerAfterRolls();
    }

    // ---------------- MERMÝ SÝSTEMÝ – PLAYER ----------------

    void InitPlayerRevolver()
    {
        playerChambers = new bool[chamberSize];

        for (int i = 0; i < chamberSize; i++)
            playerChambers[i] = false;

        // 2 farklý random dolu hazne
        for (int b = 0; b < bulletsInChamber; b++)
        {
            int idx;
            do
            {
                idx = Random.Range(0, chamberSize);
            }
            while (playerChambers[idx] == true);

            playerChambers[idx] = true;
        }

        playerChamberIndex = Random.Range(0, chamberSize);
        playerShotsFired = 0;

        Debug.Log("PLAYER tamburu karýldý.");
    }

    bool IsPlayerChamberLoaded()
    {
        if (playerChambers == null || playerChambers.Length == 0) return false;
        return playerChambers[playerChamberIndex];
    }

    void AdvancePlayerChamber()
    {
        if (playerChambers == null || playerChambers.Length == 0) return;

        playerChamberIndex = (playerChamberIndex + 1) % playerChambers.Length;
        playerShotsFired++;

        // 6 atýþ tamamlandýysa tambur yeniden kar
        if (playerShotsFired >= chamberSize)
        {
            Debug.Log("PLAYER 6 atýþý tamamladý, tambur yeniden karýlýyor.");
            InitPlayerRevolver();
        }
    }

    // ---------------- MERMÝ SÝSTEMÝ – ENEMY ----------------

    void InitEnemyRevolver()
    {
        enemyChambers = new bool[chamberSize];

        for (int i = 0; i < chamberSize; i++)
            enemyChambers[i] = false;

        // 2 farklý random dolu hazne
        for (int b = 0; b < bulletsInChamber; b++)
        {
            int idx;
            do
            {
                idx = Random.Range(0, chamberSize);
            }
            while (enemyChambers[idx] == true);

            enemyChambers[idx] = true;
        }

        enemyChamberIndex = Random.Range(0, chamberSize);
        enemyShotsFired = 0;

        Debug.Log("ENEMY tamburu karýldý.");
    }

    bool IsEnemyChamberLoaded()
    {
        if (enemyChambers == null || enemyChambers.Length == 0) return false;
        return enemyChambers[enemyChamberIndex];
    }

    void AdvanceEnemyChamber()
    {
        if (enemyChambers == null || enemyChambers.Length == 0) return;

        enemyChamberIndex = (enemyChamberIndex + 1) % enemyChambers.Length;
        enemyShotsFired++;

        if (enemyShotsFired >= chamberSize)
        {
            Debug.Log("ENEMY 6 atýþý tamamladý, tambur yeniden karýlýyor.");
            InitEnemyRevolver();
        }
    }

    // ---------------- KAZANANI BELÝRLEME ----------------

    void DecideWinnerAfterRolls()
    {
        Debug.Log($"Sonuçlar -> Player: {lastPlayerRoll} | Enemy: {lastEnemyRoll}");

        if (lastPlayerRoll > lastEnemyRoll)
        {
            Debug.Log("Player kazandý, bombastik atýþ geliyor...");
            playerTurnToShoot = true;
            currentState = TurnState.Shooting;
        }
        else if (lastEnemyRoll > lastPlayerRoll)
        {
            Debug.Log("Enemy kazandý, enayi vurmayý deneyecek...");
            playerTurnToShoot = false;
            currentState = TurnState.Shooting;

            // Enemy kendi sýrasý için otomatik ateþ edecek
            StartCoroutine(EnemyShootRoutine());
        }
        else
        {
            Debug.Log("Berabere, moto moto bidaha atýyor...");
            currentState = TurnState.Idle;
            canRoll = true;

            if (playerRollText != null) playerRollText.gameObject.SetActive(false);
            if (enemyRollText != null) enemyRollText.gameObject.SetActive(false);
        }
    }

    // ---------------- PLAYER SHOOT ----------------

    void PlayerShoot()
    {
        StartCoroutine(PlayerShootRoutine());
    }

    IEnumerator PlayerShootRoutine()
    {
        playerTurnToShoot = false;

        bool loaded = IsPlayerChamberLoaded();

        if (loaded)
        {
            enemyHP--;
            Debug.Log("Babaððð pompiþledi! (DOLU) Enemy HP: " + enemyHP);
            StartCoroutine(KnockDownAndUp(enemyBody));
        }
        else
        {
            Debug.Log("Týk... Player'ýn silahý BOÞTU.");
        }

        // Tamburu ilerlet
        AdvancePlayerChamber();

        // Debug'i görebilmek için ufak bekleme
        yield return new WaitForSeconds(1f);

        CheckEndOrNextRound();
    }

    // ---------------- ENEMY SHOOT ----------------

    IEnumerator EnemyShootRoutine()
    {
        yield return new WaitForSeconds(1f); // Ateþ etmeden önce ufak bekleme

        bool loaded = IsEnemyChamberLoaded();

        if (loaded)
        {
            playerHP--;
            Debug.Log($"Ucube ateþ etti. (DOLU) Player HP: {playerHP}");
            StartCoroutine(KnockDownAndUp(playerBody));
        }
        else
        {
            Debug.Log("Enemy'nin silahý BOÞ çýktý, klik!");
        }

        AdvanceEnemyChamber();

        yield return new WaitForSeconds(1f); // Debug gözüksün

        CheckEndOrNextRound();
    }

    // ---------------- DÜÞME / KALKMA ----------------

    IEnumerator KnockDownAndUp(Transform target)
    {
        if (target == null) yield break;

        Quaternion startRot = target.rotation;
        Quaternion knockedRot = Quaternion.Euler(
            target.eulerAngles.x + knockDownAngle,
            target.eulerAngles.y,
            target.eulerAngles.z
        );

        float t = 0f;

        // Yere düþme
        while (t < 1f)
        {
            t += Time.deltaTime / knockDuration;
            target.rotation = Quaternion.Slerp(startRot, knockedRot, t);
            yield return null;
        }

        // Yerde bekleme
        yield return new WaitForSeconds(standUpDelay);

        // Ayaða kalkma
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / knockDuration;
            target.rotation = Quaternion.Slerp(knockedRot, startRot, t);
            yield return null;
        }
    }

    // ---------------- TUR / OYUN BÝTÝÞ KONTROLÜ ----------------

    void CheckEndOrNextRound()
    {
        if (playerHP <= 0)
        {
            Debug.Log("Babaððð öldü. Kaybettik.");
            currentState = TurnState.Idle;
            canRoll = false;

            if (playerRollText != null) playerRollText.gameObject.SetActive(false);
            if (enemyRollText != null) enemyRollText.gameObject.SetActive(false);
            return;
        }

        if (enemyHP <= 0)
        {
            Debug.Log("Babaððð pompiþledi, kazandýk!");
            currentState = TurnState.Idle;
            canRoll = false;

            if (playerRollText != null) playerRollText.gameObject.SetActive(false);
            if (enemyRollText != null) enemyRollText.gameObject.SetActive(false);
            return;
        }

        // Oyun bitmediyse yeni tur
        currentState = TurnState.Idle;
        canRoll = true;
        playerTurnToShoot = false;

        if (playerRollText != null) playerRollText.gameObject.SetActive(false);
        if (enemyRollText != null) enemyRollText.gameObject.SetActive(false);

        Debug.Log("Yeni tur: tekrar zara týklayabilirsin.");
    }
}
