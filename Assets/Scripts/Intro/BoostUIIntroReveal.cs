using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BoostUIIntroReveal : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;

    [Header("Baþlangýç")]
    public bool hideOnAwake = true;

    [Header("Göz Açýldýktan Sonra")]
    public float delayAfterIntro = 0.15f;

    [Header("Yanýp Sönme")]
    public int blinkCount = 3;
    public float blinkDuration = 0.9f;
    [Range(0f, 1f)] public float minBlinkAlpha = 0.15f;
    [Range(0f, 1f)] public float maxBlinkAlpha = 1f;

    [Header("Son Belirme")]
    public float finalFadeDuration = 0.25f;

    private Coroutine routine;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (hideOnAwake)
            HideImmediate();
    }

    public void HideImmediate()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void PlayReveal()
    {
        if (canvasGroup == null)
            return;

        gameObject.SetActive(true);

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(RevealRoutine());
    }

    IEnumerator RevealRoutine()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (delayAfterIntro > 0f)
            yield return new WaitForSecondsRealtime(delayAfterIntro);

        int safeBlinkCount = Mathf.Max(1, blinkCount);
        float singleFadeDuration = blinkDuration / (safeBlinkCount * 2f);

        for (int i = 0; i < safeBlinkCount; i++)
        {
            yield return StartCoroutine(FadeAlpha(minBlinkAlpha, maxBlinkAlpha, singleFadeDuration));
            yield return StartCoroutine(FadeAlpha(maxBlinkAlpha, minBlinkAlpha, singleFadeDuration));
        }

        yield return StartCoroutine(FadeAlpha(canvasGroup.alpha, 1f, finalFadeDuration));

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        routine = null;
    }

    IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            float k = Mathf.Clamp01(t / duration);
            float smoothK = Mathf.SmoothStep(0f, 1f, k);

            canvasGroup.alpha = Mathf.Lerp(from, to, smoothK);

            yield return null;
        }

        canvasGroup.alpha = to;
    }
}