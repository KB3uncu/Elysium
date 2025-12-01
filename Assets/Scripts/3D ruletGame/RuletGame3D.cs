using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public TMP_Text enemyRollText;           // Enemy zar sonucu

    // Zar sonuçlarý
    private int lastPlayerRoll = 0;
    private int lastEnemyRoll = 0;

    // Oyun durumlarý
    private bool canRoll = true;
    private bool playerTurnToShoot = false;
    private bool enemyTurnToShoot = false;

    private enum TurnState { Idle, PlayerRoll, EnemyRoll, Shooting }
    private TurnState currentState = TurnState.PlayerRoll;

    void Start()
    {
        // Oyun baþlarken ilk tur: Player zar atacak
        currentState = TurnState.PlayerRoll;
        canRoll = true;

        if (playerRollText != null) playerRollText.text = "Player: -";
        if (enemyRollText != null) enemyRollText.text = "Enemy: -";
    }

    void Update()
    {
        // 1) ZAR ATMA SIRASI
        if (canRoll && Input.GetMouseButtonDown(0))
        {
            if (ClickedOnDice())
            {
                if (currentState == TurnState.PlayerRoll)
                {
                    StartCoroutine(PlayerRollRoutine());
                }
                else if (currentState == TurnState.EnemyRoll)
                {
                    StartCoroutine(EnemyRollRoutine());
                }
            }
        }

        // 2) PLAYER ATEÞ (þimdilik sadece sol týk – silah, mermi vs. sonra gelecek)
        if (currentState == TurnState.Shooting &&
            playerTurnToShoot &&
            Input.GetMouseButtonDown(0))
        {
            PlayerShoot();
        }
    }

    // -------------------- ZAR TIKLAMA KONTROLÜ --------------------

    bool ClickedOnDice()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
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

    // -------------------- ZAR ANÝMASYONU --------------------

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

    // -------------------- PLAYER ZAR ATMA --------------------

    IEnumerator PlayerRollRoutine()
    {
        canRoll = false;

        // Zar dönsün
        yield return StartCoroutine(SpinDiceRoutine());

        // Zar sonucu
        lastPlayerRoll = Random.Range(1, 7); // 1–6
        Debug.Log("PLAYER roll: " + lastPlayerRoll);

        if (playerRollText != null)
            playerRollText.text = "Player: " + lastPlayerRoll;

        // Þimdi sýra enemy'de
        currentState = TurnState.EnemyRoll;
        canRoll = true;
    }

    // -------------------- ENEMY ZAR ATMA --------------------

    IEnumerator EnemyRollRoutine()
    {
        canRoll = false;

        yield return StartCoroutine(SpinDiceRoutine());

        lastEnemyRoll = Random.Range(1, 7);
        Debug.Log("ENEMY roll: " + lastEnemyRoll);

        if (enemyRollText != null)
            enemyRollText.text = "Enemy: " + lastEnemyRoll;

        // Sonuçlarý karþýlaþtýr
        DecideWinnerAfterRolls();
    }

    // -------------------- KAZANANI BELÝRLEME --------------------

    void DecideWinnerAfterRolls()
    {
        Debug.Log($"Sonuçlar -> Player: {lastPlayerRoll} | Enemy: {lastEnemyRoll}");

        if (lastPlayerRoll > lastEnemyRoll)
        {
            Debug.Log("Player kazandý, bombastik atýþ geliyor...");
            playerTurnToShoot = true;
            enemyTurnToShoot = false;
            currentState = TurnState.Shooting;
            // BURAYA: Player silahý alacak (sonraki aþamada ekleyeceðiz)
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
            currentState = TurnState.PlayerRoll;
            canRoll = true;
        }
    }

    // -------------------- PLAYER SHOOT --------------------

    void PlayerShoot()
    {
        playerTurnToShoot = false;

        enemyHP--;
        Debug.Log("Babaððð pompiþledi! Enemy HP: " + enemyHP);

        StartCoroutine(KnockDownAndUp(enemyBody));
        CheckEndOrNextRound();
    }

    // -------------------- ENEMY SHOOT --------------------

    IEnumerator EnemyShootRoutine()
    {
        enemyTurnToShoot = false;

        yield return new WaitForSeconds(1f); // ufak bekleme

        playerHP--;
        Debug.Log($"Ucube ateþ etti. Player HP: {playerHP}");

        StartCoroutine(KnockDownAndUp(playerBody));
        CheckEndOrNextRound();
    }

    // -------------------- DÜÞME / KALKMA --------------------

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

    // -------------------- TUR / OYUN BÝTÝÞ KONTROLÜ --------------------

    void CheckEndOrNextRound()
    {
        if (playerHP <= 0)
        {
            Debug.Log("Babaðððððð öldü. Kaybettik goddammet");
            currentState = TurnState.Idle;
            canRoll = false;
            return;
        }

        if (enemyHP <= 0)
        {
            Debug.Log("Babaðððððð pompiþlediiii. Kazandýk ihtiyar");
            currentState = TurnState.Idle;
            canRoll = false;
            return;
        }

        // Oyun bitmediyse yeni tur
        currentState = TurnState.PlayerRoll;
        canRoll = true;
        playerTurnToShoot = false;
        enemyTurnToShoot = false;

        Debug.Log("Yeni tur: önce Player zarý atacak.");
    }
}
