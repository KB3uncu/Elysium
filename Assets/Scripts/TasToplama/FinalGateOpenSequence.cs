using System.Collections;
using UnityEngine;

public class FinalGateOpenSequence : MonoBehaviour
{
    [Header("Kapýlar")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Tooltip("Sol kapýnýn kaç derece açýlacaðý. Ters açýlýrsa 90 yerine -90 yap.")]
    public float leftDoorOpenAngle = -90f;

    [Tooltip("Sað kapýnýn kaç derece açýlacaðý. Ters açýlýrsa -90 yerine 90 yap.")]
    public float rightDoorOpenAngle = 90f;

    [Tooltip("Genelde demir kapý için Y ekseni doðru olur.")]
    public Vector3 rotationAxis = Vector3.up;

    public float openDuration = 2f;

    [Header("Oyuncu Kamerasý")]
    public Camera playerCamera;

    [Tooltip("Oyuncu kamerasýnýn kapýyý göstermek için gideceði nokta.")]
    public Transform gateViewPoint;

    [Tooltip("Kamera kapý noktasýna kaç saniyede gitsin?")]
    public float cameraMoveToGateDuration = 1f;

    [Tooltip("Kamera geri kaç saniyede dönsün?")]
    public float cameraReturnDuration = 1f;

    [Tooltip("Kamera kapýya gittikten sonra kapý açýlmadan önce bekleme.")]
    public float waitBeforeOpen = 0.3f;

    [Tooltip("Kapý açýldýktan sonra kameranýn kapýda bekleme süresi.")]
    public float holdAfterOpen = 1.5f;

    [Header("Oyuncu Kontrolü")]
    public MonoBehaviour[] scriptsToDisableDuringSequence;

    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;

    private Quaternion leftOpenRotation;
    private Quaternion rightOpenRotation;

    private bool isPlaying = false;
    private bool opened = false;

    private Transform originalCameraParent;
    private Vector3 originalCameraLocalPosition;
    private Quaternion originalCameraLocalRotation;

    void Awake()
    {
        if (leftDoor != null)
        {
            leftClosedRotation = leftDoor.localRotation;
            leftOpenRotation = leftClosedRotation * Quaternion.AngleAxis(leftDoorOpenAngle, rotationAxis.normalized);
        }

        if (rightDoor != null)
        {
            rightClosedRotation = rightDoor.localRotation;
            rightOpenRotation = rightClosedRotation * Quaternion.AngleAxis(rightDoorOpenAngle, rotationAxis.normalized);
        }
    }

    public void PlayPuzzleCompleteSequence()
    {
        if (isPlaying) return;
        if (opened) return;

        StartCoroutine(PuzzleCompleteSequence());
    }

    IEnumerator PuzzleCompleteSequence()
    {
        isPlaying = true;
        opened = true;

        SetPlayerScripts(false);

        SaveCameraOriginalTransform();

        yield return StartCoroutine(MoveCameraToGatePoint());

        yield return new WaitForSeconds(waitBeforeOpen);

        yield return StartCoroutine(OpenDoorsRoutine());

        yield return new WaitForSeconds(holdAfterOpen);

        yield return StartCoroutine(ReturnCameraToPlayer());

        RestoreCameraParent();

        SetPlayerScripts(true);

        isPlaying = false;
    }

    void SaveCameraOriginalTransform()
    {
        if (playerCamera == null) return;

        Transform camTransform = playerCamera.transform;

        originalCameraParent = camTransform.parent;
        originalCameraLocalPosition = camTransform.localPosition;
        originalCameraLocalRotation = camTransform.localRotation;

        camTransform.SetParent(null, true);
    }

    IEnumerator MoveCameraToGatePoint()
    {
        if (playerCamera == null || gateViewPoint == null)
            yield break;

        Transform camTransform = playerCamera.transform;

        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;

        Vector3 targetPos = gateViewPoint.position;
        Quaternion targetRot = gateViewPoint.rotation;

        float timer = 0f;

        while (timer < cameraMoveToGateDuration)
        {
            timer += Time.deltaTime;
            float t = timer / cameraMoveToGateDuration;
            float smoothT = SmoothStep(t);

            camTransform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            camTransform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);

            yield return null;
        }

        camTransform.position = targetPos;
        camTransform.rotation = targetRot;
    }

    IEnumerator ReturnCameraToPlayer()
    {
        if (playerCamera == null || originalCameraParent == null)
            yield break;

        Transform camTransform = playerCamera.transform;

        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;

        Vector3 targetPos = originalCameraParent.TransformPoint(originalCameraLocalPosition);
        Quaternion targetRot = originalCameraParent.rotation * originalCameraLocalRotation;

        float timer = 0f;

        while (timer < cameraReturnDuration)
        {
            timer += Time.deltaTime;
            float t = timer / cameraReturnDuration;
            float smoothT = SmoothStep(t);

            camTransform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            camTransform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);

            yield return null;
        }

        camTransform.position = targetPos;
        camTransform.rotation = targetRot;
    }

    void RestoreCameraParent()
    {
        if (playerCamera == null) return;

        Transform camTransform = playerCamera.transform;

        camTransform.SetParent(originalCameraParent, true);
        camTransform.localPosition = originalCameraLocalPosition;
        camTransform.localRotation = originalCameraLocalRotation;
    }

    IEnumerator OpenDoorsRoutine()
    {
        float timer = 0f;

        Quaternion leftStart = leftDoor != null ? leftDoor.localRotation : Quaternion.identity;
        Quaternion rightStart = rightDoor != null ? rightDoor.localRotation : Quaternion.identity;

        while (timer < openDuration)
        {
            timer += Time.deltaTime;
            float t = timer / openDuration;
            float smoothT = SmoothStep(t);

            if (leftDoor != null)
                leftDoor.localRotation = Quaternion.Slerp(leftStart, leftOpenRotation, smoothT);

            if (rightDoor != null)
                rightDoor.localRotation = Quaternion.Slerp(rightStart, rightOpenRotation, smoothT);

            yield return null;
        }

        if (leftDoor != null)
            leftDoor.localRotation = leftOpenRotation;

        if (rightDoor != null)
            rightDoor.localRotation = rightOpenRotation;
    }

    void SetPlayerScripts(bool enabled)
    {
        if (scriptsToDisableDuringSequence == null) return;

        for (int i = 0; i < scriptsToDisableDuringSequence.Length; i++)
        {
            if (scriptsToDisableDuringSequence[i] != null)
                scriptsToDisableDuringSequence[i].enabled = enabled;
        }
    }

    float SmoothStep(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }
}