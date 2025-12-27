using UnityEngine;
using System.Collections;

public class MinigameCameraIntro : MonoBehaviour
{
    [Header("Intro Hareketi")]
    public Vector3 offset = new Vector3(0f, 1.2f, -2.5f);
    public float moveDuration = 0.6f;

    [Header("Geri Dönüþ")]
    public float returnDuration = 0.5f;

    [Header("Opsiyonel")]
    public MonoBehaviour followScript;

    Vector3 savedPos;
    Quaternion savedRot;
    bool hasSaved;
    Coroutine routine;

    public void PlayIntro(Transform player)
    {
        if (player == null) return;

        if (!hasSaved)
        {
            savedPos = transform.position;
            savedRot = transform.rotation;
            hasSaved = true;
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(IntroRoutine(player));
    }

    public void ReturnToStart()
    {
        if (!hasSaved) return;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ReturnRoutine());
    }

    IEnumerator IntroRoutine(Transform player)
    {
        if (followScript != null) followScript.enabled = false;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = player.TransformPoint(offset);
        Quaternion targetRot = Quaternion.LookRotation(
            player.position + Vector3.up * 1.2f - targetPos
        );

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float k = t / moveDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, k);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, k);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        routine = null;
    }

    IEnumerator ReturnRoutine()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float k = t / returnDuration;
            transform.position = Vector3.Lerp(startPos, savedPos, k);
            transform.rotation = Quaternion.Slerp(startRot, savedRot, k);
            yield return null;
        }

        transform.position = savedPos;
        transform.rotation = savedRot;

        if (followScript != null) followScript.enabled = true;

        hasSaved = false;

        routine = null;
    }
}
