using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuletGame : MonoBehaviour
{
    [Header("Zar Ayarları")]
    public Animator diceAnimatorPlayer;   // Oyuncu zarı Animator
    public Animator diceAnimatorEnemy;    // Düşman zarı Animator
    public float diceRollDuration = 1.0f; // Zar dönme süresi

    [Header("Zar Yüzleri (Sonuç Göstermek İçin)")]
    public Sprite[] diceFaces;            // 0=1, 1=2, ..., 5=6
    public Image diceImagePlayer;         // Oyuncu zar sonucu (UI Image)
    public Image diceImageEnemy;          // Düşman zar sonucu (UI Image)

    [Header("Karakter / Silah Ayarları")]
    public Animator playerAnimator;       // Player Animator
    public Animator enemyAnimator;        // Enemy Animator
    public int playerHealth = 3;
    public int enemyHealth = 3;

    [Header("UI ve Objeler")]
    // Artık zar için buton yok, sadece silah butonu ve yazı var
    public Button gunButton;              // Silah butonu (player)
    public Text infoText;

    [Header("Rus Ruleti Tamburları")]
    private bool[] cylinderPlayer = new bool[6];
    private int currentChamberPlayer = 0;

    private bool[] cylinderEnemy = new bool[6];
    private int currentChamberEnemy = 0;

    private int playerDice;
    private int enemyDice;
    private bool canPlayerShoot = false;
    private bool isRolling = false;
    private bool canRoll = true;          // Zara tıklanabilir mi?

    void Start()
    {
        // Zar için buton yok, ama silah butonu hâlâ UI'dan geliyor
        gunButton.onClick.AddListener(OnPlayerShoot);

        gunButton.gameObject.SetActive(false);
        infoText.text = "🎲 Oyuncu, zara tıklayarak oyuna başla!";

        InitCylinderPlayer();
        InitCylinderEnemy();
    }

    // Player tamburu (6 hazne, 2 mermi)
    void InitCylinderPlayer()
    {
        for (int i = 0; i < cylinderPlayer.Length; i++)
            cylinderPlayer[i] = false;

        int m1 = Random.Range(0, cylinderPlayer.Length);
        int m2;
        do { m2 = Random.Range(0, cylinderPlayer.Length); }
        while (m2 == m1);

        cylinderPlayer[m1] = true;
        cylinderPlayer[m2] = true;
        currentChamberPlayer = Random.Range(0, cylinderPlayer.Length);
    }

    // Enemy tamburu
    void InitCylinderEnemy()
    {
        for (int i = 0; i < cylinderEnemy.Length; i++)
            cylinderEnemy[i] = false;

        int m1 = Random.Range(0, cylinderEnemy.Length);
        int m2;
        do { m2 = Random.Range(0, cylinderEnemy.Length); }
        while (m2 == m1);

        cylinderEnemy[m1] = true;
        cylinderEnemy[m2] = true;
        currentChamberEnemy = Random.Range(0, cylinderEnemy.Length);
    }

    bool FirePlayer()
    {
        bool isBullet = cylinderPlayer[currentChamberPlayer];
        currentChamberPlayer++;

        if (currentChamberPlayer >= cylinderPlayer.Length)
            InitCylinderPlayer();

        return isBullet;
    }

    bool FireEnemy()
    {
        bool isBullet = cylinderEnemy[currentChamberEnemy];
        currentChamberEnemy++;

        if (currentChamberEnemy >= cylinderEnemy.Length)
            InitCylinderEnemy();

        return isBullet;
    }

    // ARTIK dışarıdan (zara tıklama script'inden) çağrılacak
    public void OnDiceClick()
    {
        if (!canRoll) return;         // Sırası değilse / oyun bittiyse
        if (isRolling) return;        // Zaten dönüyorsa
        if (canPlayerShoot) return;   // Şu an ateş etme aşamasındaysa

        StartCoroutine(RollDicePhase());
    }

    IEnumerator RollDicePhase()
    {
        isRolling = true;
        canRoll = false;  // Zar dönüyorken tekrar tıklanmasın
        infoText.text = "🎲 Zarlar atılıyor...";

        // 1) Zar animasyonlarını tetikle
        if (diceAnimatorPlayer != null) diceAnimatorPlayer.SetTrigger("Roll");
        if (diceAnimatorEnemy != null) diceAnimatorEnemy.SetTrigger("Roll");

        // 2) Zar dönme süresi kadar bekle
        yield return new WaitForSeconds(diceRollDuration);

        // 3) Zar sonuçlarını üret
        playerDice = Random.Range(1, 7);
        enemyDice = Random.Range(1, 7);

        // 4) UI'deki zar yüzlerini güncelle
        if (diceFaces != null && diceFaces.Length >= 6)
        {
            if (diceImagePlayer != null)
                diceImagePlayer.sprite = diceFaces[playerDice - 1];

            if (diceImageEnemy != null)
                diceImageEnemy.sprite = diceFaces[enemyDice - 1];
        }

        // 5) Metin olarak sonucu göster
        infoText.text = $"Oyuncu: {playerDice}  |  Düşman: {enemyDice}";

        // Biraz sonuç ekranda kalsın
        yield return new WaitForSeconds(1.5f);

        // 6) Kazananı belirle
        if (playerDice > enemyDice)
        {
            infoText.text = "🎯 Oyuncu kazandı! Silaha tıkla ve ateş et!";
            canPlayerShoot = true;
            gunButton.gameObject.SetActive(true);   // Ateş için tıklanacak
        }
        else if (enemyDice > playerDice)
        {
            infoText.text = "💀 Düşman kazandı! Ateş ediyor...";
            yield return new WaitForSeconds(1f);     // Biraz bekle
            yield return StartCoroutine(EnemyShoot());
            StartNewRound();
        }
        else
        {
            infoText.text = "🤝 Berabere! Tekrar zar at.";
            canRoll = true;  // Tekrar zara tıklanabilir
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
        bool bullet = FirePlayer();

        if (bullet)
        {
            if (playerAnimator != null)
                playerAnimator.SetTrigger("Shoot");   // dolu mermi

            yield return new WaitForSeconds(0.4f);

            enemyHealth--;
            infoText.text = $"🎯 Düşman vuruldu! (Kalan Can: {enemyHealth})";
            yield return new WaitForSeconds(0.5f);

            CheckGameOver();
        }
        else
        {
            if (playerAnimator != null)
                playerAnimator.SetTrigger("Miss");    // boş mermi

            infoText.text = "🔁 Boş mermi! Iskaladın!";
            yield return new WaitForSeconds(1f);
        }

        StartNewRound();
    }

    IEnumerator EnemyShoot()
    {
        bool bullet = FireEnemy();

        if (bullet)
        {
            if (enemyAnimator != null)
                enemyAnimator.SetTrigger("Shoot");

            yield return new WaitForSeconds(0.4f);

            playerHealth--;
            infoText.text = $"💥 Oyuncu vuruldu! (Kalan Can: {playerHealth})";
            yield return new WaitForSeconds(0.5f);

            CheckGameOver();
        }
        else
        {
            if (enemyAnimator != null)
                enemyAnimator.SetTrigger("Miss");

            infoText.text = "😮 Düşman ıskaladı! Şanslısın!";
            yield return new WaitForSeconds(1f);
        }
    }

    void StartNewRound()
    {
        if (playerHealth > 0 && enemyHealth > 0)
        {
            infoText.text += "\n🎲 Yeni round! Zara tıkla!";
            canRoll = true;   // Yeni elde tekrar zara basılabilir
        }
        else
        {
            canRoll = false;
        }
    }

    void CheckGameOver()
    {
        if (playerHealth <= 0)
        {
            if (playerAnimator != null)
                playerAnimator.SetTrigger("Death");

            infoText.text = "☠️ Oyuncu öldü! Oyun bitti.";
            canRoll = false;
        }

        if (enemyHealth <= 0)
        {
            if (enemyAnimator != null)
                enemyAnimator.SetTrigger("Death");

            infoText.text = "🏆 Düşman öldü! Kazandın!";
            canRoll = false;
        }
    }
}
