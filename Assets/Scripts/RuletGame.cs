using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuletGame : MonoBehaviour
{
    [Header("Zar Ayarları")]
    public Animator diceAnimatorPlayer;
    public Animator diceAnimatorEnemy;
    public Image diceImagePlayer;
    public Image diceImageEnemy;
    public Sprite[] diceFaces; // 0=1, 1=2, ..., 5=6

    [Header("Karakter Ayarları")]
    public Animator playerAnimator;
    public Animator enemyAnimator;
    public int playerHealth = 3;
    public int enemyHealth = 3;

    [Header("UI ve Objeler")]
    public Button diceButton;  // zar sprite’ına tıklama için
    public Button gunButton;   // silah sprite’ına tıklama için
    public Text infoText;

    private int playerDice;
    private int enemyDice;
    private bool canPlayerShoot = false;
    private bool isRolling = false;

    void Start()
    {
        diceButton.onClick.AddListener(OnDiceClick);
        gunButton.onClick.AddListener(OnPlayerShoot);

        gunButton.gameObject.SetActive(false);
        infoText.text = "🎲 Oyuncu, zara tıklayarak oyuna başla!";
    }

    void OnDiceClick()
    {
        if (isRolling || canPlayerShoot) return;
        StartCoroutine(RollDicePhase());
    }

    IEnumerator RollDicePhase()
    {
        isRolling = true;
        diceButton.interactable = false;
        infoText.text = "🎲 Zarlar atılıyor...";

        // Animasyonları tetikle
        diceAnimatorPlayer.SetTrigger("Roll");
        diceAnimatorEnemy.SetTrigger("Roll");

        yield return new WaitForSeconds(1.2f); // animasyon süresi

        // Zar sonuçlarını üret
        playerDice = Random.Range(1, 7);
        enemyDice = Random.Range(1, 7);

        // Zar yüzeyini göster
        diceImagePlayer.sprite = diceFaces[playerDice - 1];
        diceImageEnemy.sprite = diceFaces[enemyDice - 1];

        infoText.text = $"Oyuncu: {playerDice}  |  Düşman: {enemyDice}";

        yield return new WaitForSeconds(0.5f);

        if (playerDice > enemyDice)
        {
            infoText.text = "🎯 Oyuncu kazandı! Silaha tıkla ve ateş et!";
            canPlayerShoot = true;
            gunButton.gameObject.SetActive(true);
        }
        else if (enemyDice > playerDice)
        {
            infoText.text = "💀 Düşman kazandı! Ateş ediyor...";
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(EnemyShoot());
            StartNewRound();
        }
        else
        {
            infoText.text = "🤝 Berabere! Tekrar zar at.";
            diceButton.interactable = true;
        }

        isRolling = false;
    }

    void OnPlayerShoot()
    {
        if (!canPlayerShoot) return;
        canPlayerShoot = false;
        gunButton.gameObject.SetActive(false);
        StartCoroutine(PlayerShoot());
    }

    IEnumerator PlayerShoot()
    {
        playerAnimator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.5f);
        enemyAnimator.SetTrigger("Hit");

        enemyHealth--;
        infoText.text = $"🎯 Düşman vuruldu! (Kalan Can: {enemyHealth})";

        yield return new WaitForSeconds(1f);
        CheckGameOver();
        StartNewRound();
    }

    IEnumerator EnemyShoot()
    {
        enemyAnimator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.5f);
        playerAnimator.SetTrigger("Hit");

        playerHealth--;
        infoText.text = $"💥 Oyuncu vuruldu! (Kalan Can: {playerHealth})";

        yield return new WaitForSeconds(1f);
        CheckGameOver();
    }

    void StartNewRound()
    {
        if (playerHealth > 0 && enemyHealth > 0)
        {
            infoText.text = "🎲 Yeni round! Zara tıkla!";
            diceButton.interactable = true;
        }
    }

    void CheckGameOver()
    {
        if (playerHealth <= 0)
        {
            playerAnimator.SetTrigger("Death");
            infoText.text = "☠️ Oyuncu öldü! Oyun bitti.";
            diceButton.interactable = false;
        }

        if (enemyHealth <= 0)
        {
            enemyAnimator.SetTrigger("Death");
            infoText.text = "🏆 Düşman öldü! Kazandın!";
            diceButton.interactable = false;
        }
    }
}
