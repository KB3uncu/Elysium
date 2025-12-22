using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float duration = 0.08f;
    public float strength = 0.12f;

    Vector3 startLocalPos;
    Coroutine routine;

    void Awake()
    {
        startLocalPos = transform.localPosition;
    }

    public void Play()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Routine());
    }

    IEnumerator Routine()
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            Vector3 offset = (Vector3)Random.insideUnitCircle * strength;
            transform.localPosition = startLocalPos + offset;
            yield return null;
        }

        transform.localPosition = startLocalPos;
        routine = null;
    }
}
