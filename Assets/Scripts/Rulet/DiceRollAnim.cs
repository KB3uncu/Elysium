using System.Collections;
using UnityEngine;

public class DiceRollAnim : MonoBehaviour
{
    [Header("Dönme Ayarlarý")]
    public float duration = 0.8f;
    public float spinSpeed = 900f;

    [Header("Büyüme / Küçülme")]
    public float growDuration = 0.2f;
    public float shrinkDuration = 0.25f;
    public float bigScaleMultiplier = 1.5f;
    public float resultHoldDuration = 2f;

    [Header("Yukarý Kalkma")]
    public float liftHeight = 0.35f;

    [Header("Sonuç Rotasyonlarý")]
    [Tooltip("0 = 1 gelecek yüz, 1 = 2 gelecek yüz, ... 5 = 6 gelecek yüz")]
    public Vector3[] resultEulerAngles = new Vector3[6];

    private Vector3 originalScale;
    private Vector3 originalLocalPosition;
    private Coroutine routine;

    void Awake()
    {
        originalScale = transform.localScale;
        originalLocalPosition = transform.localPosition;
    }

    public void PlayRoll()
    {
        PlayRoll(Random.Range(1, 7));
    }

    public void PlayRoll(int result)
    {
        result = Mathf.Clamp(result, 1, 6);

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(RollRoutine(result));
    }

    IEnumerator RollRoutine(int result)
    {
        Vector3 bigScale = originalScale * bigScaleMultiplier;
        Vector3 liftedPosition = originalLocalPosition + Vector3.up * liftHeight;

        yield return StartCoroutine(ScaleAndMoveTo(bigScale, liftedPosition, growDuration));

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            transform.Rotate(Vector3.right, spinSpeed * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.up, spinSpeed * 0.8f * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.forward, spinSpeed * 0.6f * Time.deltaTime, Space.Self);

            yield return null;
        }

        SetResultRotation(result);

        yield return new WaitForSeconds(resultHoldDuration);

        yield return StartCoroutine(ScaleAndMoveTo(originalScale, originalLocalPosition, shrinkDuration));

        routine = null;
    }

    void SetResultRotation(int result)
    {
        if (resultEulerAngles == null || resultEulerAngles.Length < 6)
        {
            Debug.LogWarning("DiceRollAnim: resultEulerAngles dizisi 6 elemanlý olmalý.");
            return;
        }

        transform.localRotation = Quaternion.Euler(resultEulerAngles[result - 1]);
    }

    IEnumerator ScaleAndMoveTo(Vector3 targetScale, Vector3 targetLocalPosition, float time)
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.localPosition;

        if (time <= 0f)
        {
            transform.localScale = targetScale;
            transform.localPosition = targetLocalPosition;
            yield break;
        }

        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / time);

            transform.localScale = Vector3.Lerp(startScale, targetScale, k);
            transform.localPosition = Vector3.Lerp(startPosition, targetLocalPosition, k);

            yield return null;
        }

        transform.localScale = targetScale;
        transform.localPosition = targetLocalPosition;
    }
}