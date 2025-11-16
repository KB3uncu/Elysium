using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public Button gunButton;              // Silah butonu (player)
    public Text infoText;

    [Header("Damage Ekran Efekti")]
    public Image damageOverlay;           // Canvas altında full screen kırmızı Image
    public float maxDamageAlpha = 0.6f;   // En fazla ne kadar kırmızılaşsın
    public float damageFadeDuration = 0.4f;

    [Header("Win / Lose Panelleri")]
    public GameObject winPanel;           // WIN ekranı (arka plan + kart + WİN yazısı + sağ ok)
    public GameObject losePanel;          // LOSE ekranı (arka plan + kart + LOSE yazısı + restart ok)
    public Button winNextButton;          // Win ekranındaki sağ ok butonu
    public Button loseRestartButton;      // Lose ekranındaki restart ok butonu

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

    // damage için
    private int playerMaxHealth;
    private Coroutine damageRoutine;

    // oyun bitti mi
    private bool isGameOver = false;

    void Start()
    {
        playerMaxHealth = playerHealth;

        // Damage overlay başlangıçta görünmez
        if (damageOverlay != null)
        {
            Color c = damageOverlay.color;
            c.a = 0f;
            damageOverlay.color = c;
        }

        // Win/Lose panelleri kapalı
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Restart / Next butonları
        if (winNextButton != null)
            winNextButton.onClick.AddListener(RestartScene);

        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(RestartScene);

        // Silah butonu
        if (gunButton != null)
        {
            gunButton.onClick.AddListener(OnPlayerShoot);
            gunButton.gameObject.SetActive(false);
        }

        if (infoText != null)
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

    // Zar sprite'ına bağlı script burayı çağıracak
    public void OnDiceClick()
    {
        if (isGameOver) return;
        if (!canRoll) return;
        if (isRolling) return;
        if (canPlayerShoot) return;

        StartCoroutine(RollDicePhase());
    }

    IEnumerator RollDicePhase()
    {
        isRolling = true;
        canRoll = false;
        if (infoText != null)
            infoText.text = "🎲 Zarlar atılıyor...";

        // Zar animasyonlarını tetikle
        if (diceAnimatorPlayer != null) diceAnimatorPlayer.SetTrigger("Roll");
        if (diceAnimatorEnemy != null) diceAnimatorEnemy.SetTrigger("Roll");

        // Zar dönme süresi
        yield return new WaitForSeconds(diceRollDuration);

        // Zar sonuçları
        playerDice = Random.Range(1, 7);
        enemyDice = Random.Range(1, 7);

        // UI yüzleri
        if (diceFaces != null && diceFaces.Length >= 6)
        {
            if (diceImagePlayer != null)
                diceImagePlayer.sprite = diceFaces[playerDice - 1];
            if (diceImageEnemy != null)
                diceImageEnemy.sprite = diceFaces[enemyDice - 1];
        }

        // Metin
        if (infoText != null)
            infoText.text = $"Oyuncu: {playerDice}  |  Düşman: {enemyDice}";

        // Sonucu biraz göster
        yield return new WaitForSeconds(1.5f);

        // Karşılaştırma
        if (playerDice > enemyDice)
        {
            if (infoText != null)
                infoText.text = "🎯 Oyuncu kazandı! Silaha tıkla ve ateş et!";
            canPlayerShoot = true;
            if (gunButton != null)
                gunButton.gameObject.SetActive(true);
        }
        else if (enemyDice > playerDice)
        {
            if (infoText != null)
                infoText.text = "💀 Düşman kazandı! Ateş ediyor...";
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(EnemyShoot());
            StartNewRound();
        }
        else
        {
            if (infoText != null)
                infoText.text = "🤝 Berabere! Tekrar zar at.";
            canRoll = true;
        }

        isRolling = false;
    }

    void OnPlayerShoot()
    {
        if (isGameOver) return;
        if (!canPlayerShoot) return;

        canPlayerShoot = false;
        if (gunButton != null)
            gunButton.gameObject.SetActive(false);

        StartCoroutine(PlayerShoot());
    }

    IEnumerator PlayerShoot()
    {
        bool bullet = FirePlayer();

        if (bullet)
        {
            // OYUNCU ATEŞ
            if (playerAnimator != null)
                playerAnimator.SetTrigger("Shoot");

            // DÜŞMAN HURT
            if (enemyAnimator != null)
                enemyAnimator.SetTrigger("Hurt");

            yield return new WaitForSeconds(0.4f);

            enemyHealth--;
            if (infoText != null)
                infoText.text = $"🎯 Düşman vuruldu! (Kalan Can: {enemyHealth})";

            yield return new WaitForSeconds(0.5f);

            // CAN 0'A DÜŞTÜYSE
            if (enemyHealth <= 0)
            {
                CheckGameOver();
                yield break; // oyun bitti, yeni round yok
            }
        }
        else
        {
            // BOŞ MERMİ
            if (playerAnimator != null)
                playerAnimator.SetTrigger("Miss");

            if (infoText != null)
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
            // DÜŞMAN ATEŞ
            if (enemyAnimator != null)
                enemyAnimator.SetTrigger("Shoot");

            // OYUNCU HURT
            if (playerAnimator != null)
                playerAnimator.SetTrigger("Hurt");

            yield return new WaitForSeconds(0.4f);

            playerHealth--;
            if (infoText != null)
                infoText.text = $"💥 Oyuncu vuruldu! (Kalan Can: {playerHealth})";

            // Ekran kırmızı efekti
            ApplyPlayerDamageEffect();

            yield return new WaitForSeconds(0.5f);

            if (playerHealth <= 0)
            {
                CheckGameOver();
                yield break;
            }
        }
        else
        {
            // DÜŞMAN BOŞ MERMİ
            if (enemyAnimator != null)
                enemyAnimator.SetTrigger("Miss");

            if (infoText != null)
                infoText.text = "😮 Düşman ıskaladı! Şanslısın!";
            yield return new WaitForSeconds(1f);
        }
    }

    void StartNewRound()
    {
        if (isGameOver) return;

        if (playerHealth > 0 && enemyHealth > 0)
        {
            if (infoText != null)
                infoText.text += "\n🎲 Yeni round! Zara tıkla!";
            canRoll = true;
        }
        else
        {
            canRoll = false;
        }
    }

    void CheckGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        canRoll = false;
        canPlayerShoot = false;

        if (gunButton != null)
            gunButton.gameObject.SetActive(false);

        if (playerHealth <= 0)
        {
            if (infoText != null)
                infoText.text = "☠️ Oyuncu öldü! Oyun bitti.";
            ShowLosePanel();
        }
        else if (enemyHealth <= 0)
        {
            if (infoText != null)
                infoText.text = "🏆 Düşman öldü! Kazandın!";
            ShowWinPanel();
        }
    }

    // ----- WIN / LOSE PANEL FONKSİYONLARI -----

    void ShowWinPanel()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (losePanel != null) losePanel.SetActive(false);
    }

    void ShowLosePanel()
    {
        if (losePanel != null) losePanel.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
    }

    void RestartScene()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    // ----- DAMAGE OVERLAY FONKSİYONLARI -----

    void ApplyPlayerDamageEffect()
    {
        if (damageOverlay == null || playerMaxHealth <= 0)
            return;

        // can azaldıkça healthRatio 0 → 1 arası büyüyor
        float healthRatio = Mathf.Clamp01(1f - (float)playerHealth / playerMaxHealth);
        float targetAlpha = Mathf.Lerp(0f, maxDamageAlpha, healthRatio);

        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        damageRoutine = StartCoroutine(DamageFlashRoutine(targetAlpha));
    }

    IEnumerator DamageFlashRoutine(float targetAlpha)
    {
        Color c = damageOverlay.color;

        // önce bir anda koyu kırmızıya geç
        c.a = targetAlpha;
        damageOverlay.color = c;

        // sonra yarısına kadar yumuşakça düşsün
        float t = 0f;
        float startAlpha = targetAlpha;
        float endAlpha = targetAlpha * 0.5f;

        while (t < damageFadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, endAlpha, t / damageFadeDuration);
            c.a = a;
            damageOverlay.color = c;
            yield return null;
        }

        c.a = endAlpha;
        damageOverlay.color = c;
    }
}
