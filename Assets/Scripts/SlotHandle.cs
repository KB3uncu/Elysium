using UnityEngine;
using System.Collections;

public class SlotHandle : MonoBehaviour, IInteractable
{
    [Header("Silindirler")]
    public SlotCylinder[] cylinders;
    private bool isPulled = false;

    [Header("Animasyon Ayarlarý")]
    public float pullAngle = 45f;
    public float pullDuration = 0.3f;

    public void OnInteract()
    {
        if (!isPulled)
            StartCoroutine(PullHandle());
    }

    private IEnumerator PullHandle()
    {
        isPulled = true;

        Quaternion startRot = transform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(pullAngle, 0f, 0f);

        float t = 0f;
        while (t < pullDuration)
        {
            float k = t / pullDuration;
            transform.localRotation = Quaternion.Slerp(startRot, endRot, k);
            t += Time.deltaTime;
            yield return null;
        }

        foreach (var cyl in cylinders)
        {
            if (cyl != null)
                cyl.Spin();
        }

        t = 0f;
        while (t < pullDuration)
        {
            float k = t / pullDuration;
            transform.localRotation = Quaternion.Slerp(endRot, startRot, k);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = startRot;
        isPulled = false;
    }
}
