using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml;

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

    [Header("UI")]
    public TMP_Text playerRollText;           // Player zar sonucu
    public TMP_Text enemyRollText;            // Enemy zar sonucu

    [Header("Silah Ayarlarý")]
    public Transform gunObject;           // Player'ýn týklayacaðý silah (masadaki ya da elindeki)

    [Header("Mermi Sistemi")]
    public int chamberSize = 6;           // Tamburdaki hazne sayýsý
    public int bulletsInChamber = 2;      // Toplam dolu mermi sayýsý (2)

    private bool[] chambers;              // true = dolu hazne
    private int currentChamberIndex = 0;  // Þu an ateþlenen hazne indexi

    // Zar sonuçlarý
    private int lastPlayerRoll = 0;
    private int lastEnemyRoll = 0;

    // Oyun durumlarý
    private bool canRoll = true;          // Zara týklanabilir mi?
    private bool playerTurnToShoot = false;
    private bool enemyTurnToShoot = false;

    private enum TurnState { Idle, Rolling, Shooting }
    private TurnState currentState = TurnState.Idle;

    void Start()
    {
        currentState = TurnState.Idle;
        canRoll = true;

        if (playerRollText != null) playerRollText.gameObject.SetActive(false);
        if (enemyRollText != null) enemyRollText.gameObject.SetActive(false);
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

        // 2) Player ateþ – SADECE sýra player'dayken ve silaha týklayýnca
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

    // -------------- TIKLAMA KONTROLLERÝ --------------

    bool ClickedOnDice()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Trigger collider'larý yok say (rulet box'ý engellemesin)
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

    // -------------- ZAR ANÝMASYONU --------------

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

    // -------------- TEK TIKLAMADA ÝKÝ TARAFIN ZAR SEANSI --------------

    IEnumerator RollBothRoutine()
    {
        canRoll = false;
        currentState = TurnState.Rolling;
        playerTurnToShoot = false;
        enemyTurnToShoot = false;

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

        // Yeni el için tamburu kar
        InitializeRevolver();

        // Kazananý belirle
        DecideWinnerAfterRolls();
    }

    // -------------- MERMÝ SÝSTEMÝ --------------

    void InitializeRevolver()
    {
        chambers = new bool[chamberSize];
        for (int i = 0; i < chamberSize; i++)
        {
            chambers[i] = false;
        }

        // 2 farklý random index seç
        int first = Random.Range(0, chamberSize);
        int second;
        do
        {
            second = Random.Range(0, chamberSize);
        } while (second == first);

        chambers[first] = true;
        chambers[second] = true;

        // Tamburun baþlayacaðý yer
        currentChamberIndex = Random.Range(0, chamberSize);

        Debug.Log($"Tambur karýldý. Dolu hazneler: {first}, {second}. Baþlangýç index: {currentChamberIndex}");
    }

    bool IsCurrentChamberLoaded()
    {
        if (chambers == null || chambers.Length == 0) return false;
        return chambers[currentChamberIndex];
    }

    void AdvanceChamber()
    {
        if (chambers == null || chambers.Length == 0) return;
        currentChamberIndex = (currentChamberIndex + 1) % chambers.Length;
    }

    // -------------- KAZANANI BELÝRLEME --------------

    void DecideWinnerAfterRolls()
    {
        Debug.Log($"Sonuçlar -> Player: {lastPlayerRoll} | Enemy: {lastEnemyRoll}");

        if (lastPlayerRoll > lastEnemyRoll)
        {
            Debug.Log("Player kazandý, bombastik atýþ geliyor...");
            playerTurnToShoot = true;
            enemyTurnToShoot = false;
            currentState = TurnState.Shooting;
        }
        else if (lastEnemyRoll > lastPlayerRoll)
        {
            Debug.Log("Enemy kazandý, enayi vurmayý deneyecek...");
            playerTurnToShoot = false;
            enemyTurnToShoot = true;
            currentState = TurnState.Shooting;

            // Enemy kendi sýrasý için otomatik ateþ edecek
            StartCoroutine(EnemyShootRoutine());
        }
        else
        {
            Debug.Log("Berabere, moto moto bidaha atýyor...");
            currentState = TurnState.Idle;
            canRoll = true;
        }
    }

    // -------------- PLAYER SHOOT --------------

    void PlayerShoot()
    {
        playerTurnToShoot = false;

        bool loaded = IsCurrentChamberLoaded();

        if (loaded)
        {
            enemyHP--;
            Debug.Log("Babaððð pompiþledi! (DOLU) Enemy HP: " + enemyHP);
            StartCoroutine(KnockDownAndUp(enemyBody));
        }
        else
        {
            Debug.Log("Týk... Silah BOÞTU, babaððð boþa çekti.");
        }

        // Sonraki atýþ için tamburu döndür
        AdvanceChamber();

        CheckEndOrNextRound();
    }

    // -------------- ENEMY SHOOT --------------

    IEnumerator EnemyShootRoutine()
    {
        enemyTurnToShoot = false;

        yield return new WaitForSeconds(1f); // ufak bekleme

        bool loaded = IsCurrentChamberLoaded();

        if (loaded)
        {
            playerHP--;
            Debug.Log($"Ucube ateþ etti. (DOLU) Player HP: {playerHP}");
            StartCoroutine(KnockDownAndUp(playerBody));
        }
        else
        {
            Debug.Log("Enemy'nin silahý boþ çýktý, klik!");
        }

        AdvanceChamber();

        CheckEndOrNextRound();
    }

    // -------------- DÜÞME / KALKMA --------------

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

    // -------------- TUR / OYUN BÝTÝÞ KONTROLÜ --------------

    void CheckEndOrNextRound()
    {
        if (playerHP <= 0)
        {
            Debug.Log("Babaðððððð öldü. Kaybettik goddammet");
            currentState = TurnState.Idle;
            canRoll = false;
            if (playerRollText != null) playerRollText.gameObject.SetActive(false);
            if (enemyRollText != null) enemyRollText.gameObject.SetActive(false);
            return;
        }

        if (enemyHP <= 0)
        {
            Debug.Log("Babaðððððð pompiþlediiii. Kazandýk ihtiyar");
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
        enemyTurnToShoot = false;

        // Yeni tur baþlarken UI kapansýn
        if (playerRollText != null) playerRollText.gameObject.SetActive(false);
        if (enemyRollText != null) enemyRollText.gameObject.SetActive(false);

        Debug.Log("Yeni tur: tekrar zara týklayabilirsin.");
    }
}
