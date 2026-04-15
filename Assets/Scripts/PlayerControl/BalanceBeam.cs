using UnityEngine;

public class BalanceBeam : MonoBehaviour
{
    [Header("Beam Direction")]
    public Transform beamForwardReference;

    [Header("Movement")]
    public float balanceWalkSpeed = 2.2f;
    public bool allowBackwardMovement = true;

    [Header("Balance Steps")]
    public int maxBalanceStep = 5;

    [Header("Random Sway Timing")]
    public float swayIntervalMin = 0.35f;
    public float swayIntervalMax = 0.9f;

    [Header("Random Sway Amount")]
    public int minSwayStep = 1;
    public int maxSwayStep = 2;

    [Header("Sudden Sway Chance")]
    [Range(0f, 1f)] public float suddenSwayChance = 0.30f;
    public int suddenSwayMinStep = 2;
    public int suddenSwayMaxStep = 3;

    [Header("Fail")]
    public float failPushForce = 3.5f;
    public float failUpForce = 2.5f;

    [Header("Visual")]
    public float maxCameraRoll = 14f;

    private void Reset()
    {
        if (beamForwardReference == null)
            beamForwardReference = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        FPSController fps = other.GetComponent<FPSController>();
        if (fps == null)
            fps = other.GetComponentInParent<FPSController>();

        if (fps != null)
            fps.EnterBalanceMode(this);
    }

    private void OnTriggerExit(Collider other)
    {
        FPSController fps = other.GetComponent<FPSController>();
        if (fps == null)
            fps = other.GetComponentInParent<FPSController>();

        if (fps != null)
            fps.ExitBalanceMode(this);
    }

    public Vector3 GetRawBeamForward()
    {
        Transform refTransform = beamForwardReference != null ? beamForwardReference : transform;
        Vector3 forward = refTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;

        return forward.normalized;
    }

    public Vector3 GetBeamForwardFromLook(Vector3 lookForward)
    {
        Vector3 beamForward = GetRawBeamForward();

        Vector3 flatLook = lookForward;
        flatLook.y = 0f;

        if (flatLook.sqrMagnitude < 0.0001f)
            flatLook = beamForward;

        flatLook.Normalize();

        float dot = Vector3.Dot(flatLook, beamForward);

        return dot >= 0f ? beamForward : -beamForward;
    }

    public Vector3 GetBeamRightFromMoveDirection(Vector3 moveForward)
    {
        Vector3 right = Vector3.Cross(Vector3.up, moveForward).normalized;

        if (right.sqrMagnitude < 0.0001f)
            right = Vector3.right;

        return right;
    }
}