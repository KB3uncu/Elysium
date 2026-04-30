using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioSource shieldSource;

    [Header("SFX Clips")]
    public AudioClip diceRoll;
    public AudioClip gunShot;
    public AudioClip hit;
    public AudioClip miss;
    public AudioClip emptyGun;
    public AudioClip win;
    public AudioClip lose;
    public AudioClip shieldBlock;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // sahne deðiþse bile kalýr
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================
    // GENERIC PLAY
    // =========================
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // =========================
    // SHORTCUT METHODS
    // =========================
    public void PlayDice() => PlaySFX(diceRoll);
    public void PlayGun() => PlaySFX(gunShot);
    public void PlayHit() => PlaySFX(hit);
    public void PlayMiss() => PlaySFX(miss);
    public void PlayEmpty() => PlaySFX(emptyGun);
    public void PlayWin() => PlaySFX(win);
    public void PlayLose() => PlaySFX(lose);
    public void PlayShield()
    {
        if (shieldBlock == null) return;
        shieldSource.PlayOneShot(shieldBlock);
    }
}