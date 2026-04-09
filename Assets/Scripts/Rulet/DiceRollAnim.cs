using System.Collections;
using UnityEngine;

public class DiceRollAnim : MonoBehaviour
{
    public float duration = 0.6f;
    public float spinSpeed = 900f; // derece/sn

    Coroutine routine;

    public void PlayRoll()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(RollRoutine());
    }

    IEnumerator RollRoutine()
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            // rastgele eksenlerde döndür
            transform.Rotate(Vector3.right, spinSpeed * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.up, spinSpeed * 0.8f * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.forward, spinSpeed * 0.6f * Time.deltaTime, Space.Self);

            yield return null;
        }

        routine = null;
    }
}
