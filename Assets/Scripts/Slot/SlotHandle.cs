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

    [Header("Kazanma Durumu")]
    public GameObject WinPanel;


    public void OnInteract()
    {
        if (!isPulled)
            StartCoroutine(PullHandle());
    }

    private IEnumerator PullHandle()
    {
        isPulled = true;

        Quaternion startRot = transform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 0f, pullAngle);

        float t = 0f;
        while (t < pullDuration)
        {
            transform.localRotation = Quaternion.Slerp(startRot, endRot, t / pullDuration);
            t += Time.deltaTime;
            yield return null;
        }

        foreach (var cyl in cylinders)
            cyl?.Spin();

        yield return new WaitUntil(() =>
        {
            foreach (var c in cylinders)
                if (c.IsSpinning)
                    return false;
            return true;
        });

        CheckWinCondition();

        t = 0f;
        while (t < pullDuration)
        {
            transform.localRotation = Quaternion.Slerp(endRot, startRot, t / pullDuration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = startRot;
        isPulled = false;
    }

    void CheckWinCondition()
    {
        if (cylinders.Length < 3) return;

        int face = cylinders[0].CurrentFaceIndex;

        for (int i = 1; i < cylinders.Length; i++)
        {
            if (cylinders[i].CurrentFaceIndex != face)
            {
                Debug.Log("Kaybettin");
                return;
            }
        }

        Debug.Log(" KAZANDIN ");
        WinPanel.SetActive(true);
    }
}
