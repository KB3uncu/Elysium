using UnityEngine;
using System.Collections;

public class SideHammerHit : MonoBehaviour
{
    [Header("Hit Settings")]
    public float hitAngle = 80f;
    public float hitDuration = 0.15f;
    public float returnDuration = 0.25f;

    [Tooltip("Yerel eksen: hangi eksende 80 derece vuracak")]
    public Vector3 localAxis = Vector3.forward;

    private Coroutine routine;

    public void Hit()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        Quaternion start = transform.localRotation;
        Quaternion hit = start * Quaternion.AngleAxis(hitAngle, localAxis.normalized);

        float t = 0f;
        while (t < hitDuration)
        {
            transform.localRotation = Quaternion.Slerp(start, hit, t / hitDuration);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = hit;

        t = 0f;
        while (t < returnDuration)
        {
            transform.localRotation = Quaternion.Slerp(hit, start, t / returnDuration);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = start;

        routine = null;
    }
}
