using UnityEngine;
using System.Collections;

public class SlotCylinder : MonoBehaviour
{
    [Header("Dönüþ Ayarlarý")]
    public float spinDuration = 2f;
    public int minFullSpins = 3;
    public int maxFullSpins = 5;

    [Header("Yüz Ayarlarý")]
    public int faceCount = 4;
    public float faceOffset = 0f;
    public Axis spinAxis = Axis.X;

    public int CurrentFaceIndex { get; private set; }

    private bool isSpinning = false;

    public enum Axis { X, Y, Z }

    public bool IsSpinning => isSpinning;

    public void Spin()
    {
        if (!isSpinning)
            StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;


        int fullSpins = Random.Range(minFullSpins, maxFullSpins + 1);
        float randomExtraAngle = Random.Range(0f, 360f);
        float totalAngle = fullSpins * 360f + randomExtraAngle;

        Vector3 startRot = transform.localEulerAngles;
        float startAngle = GetAxisAngle(startRot);

        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            float t = elapsed / spinDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);
            float currentAngle = startAngle + totalAngle * t;
            ApplyRotation(currentAngle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        float finalAngle = startAngle + totalAngle;
        float step = 360f / faceCount;
        float snappedAngle = Mathf.Round(finalAngle / step) * step + faceOffset;

        ApplyRotation(snappedAngle);

        float normalized = (snappedAngle % 360f + 360f) % 360f;
        CurrentFaceIndex = Mathf.RoundToInt(normalized / step) % faceCount;

        isSpinning = false;

        Debug.Log($"[{name}] Yüz Index: {CurrentFaceIndex}");
    }

    void ApplyRotation(float angle)
    {
        switch (spinAxis)
        {
            case Axis.X:
                transform.localRotation = Quaternion.Euler(angle, -90f, -90f);
                break;
            case Axis.Y:
                transform.localRotation = Quaternion.Euler(-90f, angle, -90f);
                break;
            case Axis.Z:
                transform.localRotation = Quaternion.Euler(-90f, -90f, angle);
                break;
        }
    }

    float GetAxisAngle(Vector3 euler)
    {
        switch (spinAxis)
        {
            case Axis.Y: return euler.y;
            case Axis.Z: return euler.z;
            default: return euler.x;
        }
    }
}
