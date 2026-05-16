using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OpeningCutsceneController : MonoBehaviour
{
    [Header("Cutscene UI")]
    public GameObject cutsceneRoot;
    public CanvasGroup rootCanvasGroup;
    public Image blackBackground;
    public Image cutsceneImage;

    [Header("Görseller")]
    public Sprite[] slides = new Sprite[3];

    [Header("Geçiþ Ayarlarý")]
    public float fadeDuration = 0.45f;
    public float firstInputDelay = 0.5f;
    public float inputCooldown = 0.2f;

    [Header("Input Ayarlarý")]
    public bool acceptAnyKey = false;
    public bool allowEscapeToSkipAll = true;

    [Header("Oyun Baþlangýcý")]
    public bool playOnStart = true;
    public bool pauseGameTime = true;

    [Header("Cutscene Boyunca Kapatýlacak Scriptler")]
    public MonoBehaviour[] scriptsToDisableDuringCutscene;

    private int currentSlideIndex = 0;
    private bool cutsceneActive = false;
    private bool isTransitioning = false;
    private float inputAllowedTime = 0f;
    private float previousTimeScale = 1f;

    void Awake()
    {
        if (cutsceneRoot == null)
            cutsceneRoot = gameObject;

        if (rootCanvasGroup == null)
            rootCanvasGroup = cutsceneRoot.GetComponent<CanvasGroup>();

        if (rootCanvasGroup == null)
            rootCanvasGroup = cutsceneRoot.AddComponent<CanvasGroup>();

        if (cutsceneImage == null)
        {
            Transform imageChild = cutsceneRoot.transform.Find("CutsceneImage");
            if (imageChild != null)
                cutsceneImage = imageChild.GetComponent<Image>();
        }

        if (blackBackground == null)
        {
            Transform bgChild = cutsceneRoot.transform.Find("BlackBackground");
            if (bgChild != null)
                blackBackground = bgChild.GetComponent<Image>();
        }

        if (cutsceneImage == null)
            Debug.LogError("OpeningCutsceneController: CutsceneImage bulunamadý.");

        SetupInitialUI();
    }

    void Start()
    {
        if (playOnStart)
            StartCutscene();
        else
            HideCutsceneUI();
    }

    void Update()
    {
        if (!cutsceneActive)
            return;

        if (isTransitioning)
            return;

        if (Time.unscaledTime < inputAllowedTime)
            return;

        if (allowEscapeToSkipAll && Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(EndCutsceneRoutine());
            return;
        }

        if (NextInputPressed())
        {
            GoNextSlide();
        }
    }

    void SetupInitialUI()
    {
        rootCanvasGroup.alpha = 0f;
        rootCanvasGroup.blocksRaycasts = false;
        rootCanvasGroup.interactable = false;

        if (blackBackground != null)
        {
            blackBackground.gameObject.SetActive(true);
            blackBackground.color = Color.black;
            blackBackground.raycastTarget = false;
        }

        if (cutsceneImage != null)
        {
            cutsceneImage.gameObject.SetActive(true);
            cutsceneImage.color = Color.white;
            cutsceneImage.preserveAspect = true;
            cutsceneImage.raycastTarget = false;
        }
    }

    public void StartCutscene()
    {
        if (slides == null || slides.Length == 0)
        {
            Debug.LogWarning("OpeningCutsceneController: Slides listesi boþ.");
            EndCutsceneImmediate();
            return;
        }

        if (slides[0] == null)
        {
            Debug.LogError("OpeningCutsceneController: Ýlk slide boþ.");
            EndCutsceneImmediate();
            return;
        }

        cutsceneActive = true;
        isTransitioning = false;
        currentSlideIndex = 0;

        if (cutsceneRoot != null)
            cutsceneRoot.SetActive(true);

        rootCanvasGroup.alpha = 1f;
        rootCanvasGroup.blocksRaycasts = true;
        rootCanvasGroup.interactable = true;

        if (blackBackground != null)
            blackBackground.color = Color.black;

        if (cutsceneImage != null)
        {
            cutsceneImage.sprite = slides[currentSlideIndex];
            SetImageAlpha(1f);
        }

        SetGameplayScriptsEnabled(false);

        previousTimeScale = Time.timeScale;

        if (pauseGameTime)
            Time.timeScale = 0f;

        inputAllowedTime = Time.unscaledTime + firstInputDelay;
    }

    void GoNextSlide()
    {
        if (currentSlideIndex >= slides.Length - 1)
        {
            StartCoroutine(EndCutsceneRoutine());
            return;
        }

        currentSlideIndex++;

        if (slides[currentSlideIndex] == null)
        {
            Debug.LogError("OpeningCutsceneController: Slides içinde boþ görsel var. Index: " + currentSlideIndex);
            return;
        }

        StartCoroutine(ChangeSlideRoutine(currentSlideIndex));
    }

    IEnumerator ChangeSlideRoutine(int newIndex)
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeImage(1f, 0f));

        if (cutsceneImage != null)
            cutsceneImage.sprite = slides[newIndex];

        yield return StartCoroutine(FadeImage(0f, 1f));

        inputAllowedTime = Time.unscaledTime + inputCooldown;
        isTransitioning = false;
    }

    IEnumerator EndCutsceneRoutine()
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeImage(1f, 0f));

        yield return StartCoroutine(FadeRoot(1f, 0f));

        EndCutsceneImmediate();
    }

    void EndCutsceneImmediate()
    {
        cutsceneActive = false;
        isTransitioning = false;

        if (pauseGameTime)
            Time.timeScale = previousTimeScale;

        SetGameplayScriptsEnabled(true);

        HideCutsceneUI();
    }

    void HideCutsceneUI()
    {
        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = 0f;
            rootCanvasGroup.blocksRaycasts = false;
            rootCanvasGroup.interactable = false;
        }

        if (cutsceneRoot != null)
            cutsceneRoot.SetActive(false);
    }

    IEnumerator FadeImage(float from, float to)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;

            float k = Mathf.Clamp01(t / fadeDuration);
            float smoothK = Mathf.SmoothStep(0f, 1f, k);

            SetImageAlpha(Mathf.Lerp(from, to, smoothK));

            yield return null;
        }

        SetImageAlpha(to);
    }

    IEnumerator FadeRoot(float from, float to)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;

            float k = Mathf.Clamp01(t / fadeDuration);
            float smoothK = Mathf.SmoothStep(0f, 1f, k);

            rootCanvasGroup.alpha = Mathf.Lerp(from, to, smoothK);

            yield return null;
        }

        rootCanvasGroup.alpha = to;
    }

    void SetImageAlpha(float alpha)
    {
        if (cutsceneImage == null)
            return;

        Color c = cutsceneImage.color;
        c.a = alpha;
        cutsceneImage.color = c;
    }

    bool NextInputPressed()
    {
        if (acceptAnyKey && Input.anyKeyDown)
            return true;

        if (Input.GetKeyDown(KeyCode.Space))
            return true;

        if (Input.GetKeyDown(KeyCode.Return))
            return true;

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
            return true;

        if (Input.GetMouseButtonDown(0))
            return true;

        if (Input.GetKeyDown(KeyCode.JoystickButton0))
            return true;

        return false;
    }

    void SetGameplayScriptsEnabled(bool enabled)
    {
        if (scriptsToDisableDuringCutscene == null)
            return;

        for (int i = 0; i < scriptsToDisableDuringCutscene.Length; i++)
        {
            if (scriptsToDisableDuringCutscene[i] != null)
                scriptsToDisableDuringCutscene[i].enabled = enabled;
        }
    }
}