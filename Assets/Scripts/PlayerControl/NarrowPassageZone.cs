using UnityEngine;

public class NarrowPassageZone : MonoBehaviour
{
    [Header("Passage Direction")]
    public Transform passageForwardReference;

    [Header("Passage Settings")]
    public float narrowMoveSpeed = 1.4f;
    public float narrowControllerRadius = 0.18f;
    public float cameraRoll = 8f;
    public bool allowBackwardMovement = true;

    private void Reset()
    {
        if (passageForwardReference == null)
            passageForwardReference = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        FPSController fps = other.GetComponent<FPSController>();
        if (fps == null)
            fps = other.GetComponentInParent<FPSController>();

        if (fps != null)
            fps.EnterNarrowPassageMode(this);
    }

    private void OnTriggerExit(Collider other)
    {
        FPSController fps = other.GetComponent<FPSController>();
        if (fps == null)
            fps = other.GetComponentInParent<FPSController>();

        if (fps != null)
            fps.ExitNarrowPassageMode(this);
    }

    public Vector3 GetPassageForwardFromLook(Vector3 lookForward)
    {
        Transform refTransform = passageForwardReference != null ? passageForwardReference : transform;

        Vector3 passageForward = refTransform.forward;
        passageForward.y = 0f;

        if (passageForward.sqrMagnitude < 0.0001f)
            passageForward = Vector3.forward;

        passageForward.Normalize();

        Vector3 flatLook = lookForward;
        flatLook.y = 0f;

        if (flatLook.sqrMagnitude < 0.0001f)
            flatLook = passageForward;

        flatLook.Normalize();

        float dot = Vector3.Dot(flatLook, passageForward);
        return dot >= 0f ? passageForward : -passageForward;
    }
}