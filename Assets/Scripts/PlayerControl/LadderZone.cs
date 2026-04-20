using UnityEngine;

public class LadderZone : MonoBehaviour
{
    [Header("Ladder Settings")]
    public float climbSpeed = 2f;
    public float ladderCameraBobSpeed = 7f;
    public float ladderCameraBobAmount = 0.025f;

    [Header("Directions")]
    public Transform ladderUpReference;
    public Transform ladderForwardReference;

    private void Reset()
    {
        if (ladderUpReference == null)
            ladderUpReference = transform;
        if (ladderForwardReference == null)
            ladderForwardReference = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        FPSController fps = other.GetComponent<FPSController>();
        if (fps == null)
            fps = other.GetComponentInParent<FPSController>();

        if (fps != null)
            fps.EnterLadderMode(this);
    }

    private void OnTriggerExit(Collider other)
    {
        FPSController fps = other.GetComponent<FPSController>();
        if (fps == null)
            fps = other.GetComponentInParent<FPSController>();

        if (fps != null)
            fps.ExitLadderMode(this);
    }

    public Vector3 GetLadderUp()
    {
        Vector3 up = ladderUpReference != null ? ladderUpReference.up : transform.up;
        up.y = Mathf.Abs(up.y) < 0.001f ? 1f : up.y;
        return up.normalized;
    }

    public Vector3 GetLadderForward()
    {
        Vector3 forward = ladderForwardReference != null ? ladderForwardReference.forward : transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        return forward.normalized;
    }
}