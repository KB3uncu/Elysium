using UnityEngine;
using System.Collections;

public class ChestOpenOnApproach : MonoBehaviour
{
    [Header("References")]
    public Transform lid;
    public Transform player;

    [Header("Open Settings")]
    public float openDistance = 1.6f;
    public float openAngle = 135f;
    public float openDuration = 0.6f;

    public Vector3 localOpenAxis = Vector3.right; // X ekseni

    private bool opened = false;
    private Quaternion closedRot;
    private Quaternion openRot;

    void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (lid != null)
        {
            closedRot = lid.localRotation;
            openRot = closedRot * Quaternion.AngleAxis(openAngle, localOpenAxis.normalized);
        }
    }

    void Update()
    {
        if (opened) return;
        if (lid == null || player == null) return;

        float d = Vector3.Distance(player.position, transform.position);
        if (d <= openDistance)
        {
            opened = true;
            StartCoroutine(OpenLid());
        }
    }

    IEnumerator OpenLid()
    {
        float t = 0f;
        while (t < openDuration)
        {
            t += Time.deltaTime;
            float k = t / openDuration;
            lid.localRotation = Quaternion.Slerp(closedRot, openRot, k);
            yield return null;
        }

        lid.localRotation = openRot;
    }
}
