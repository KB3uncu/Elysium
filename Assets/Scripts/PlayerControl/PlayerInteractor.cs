using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    public float interactDistance = 3f;
    public LayerMask interactLayer = ~0;

    [Header("Crosshair")]
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;
    public bool useCrosshairHighlight = false;

    [Header("World E Prompt")]
    public WorldInteractPrompt interactPrompt;
    public float promptForwardOffset = 0.35f;
    public float promptUpOffset = 0.4f;

    Camera cam;
    IInteractable currentTarget;

    RaycastHit currentHit;
    bool hasHit;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        if (crosshairImage != null)
            crosshairImage.color = normalColor;

        if (interactPrompt != null)
            interactPrompt.HideInstant();
    }

    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
        {
            if (hasHit)
            {
                var wall = currentHit.collider.GetComponentInParent<BreakableWall>();

                if (wall != null)
                    wall.SetLastHit(currentHit.point, currentHit.normal);
            }

            currentTarget.OnInteract();
        }
    }

    void CheckForInteractable()
    {
        hasHit = false;
        currentTarget = null;

        if (cam == null)
            return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer, QueryTriggerInteraction.Collide))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                currentTarget = interactable;
                currentHit = hit;
                hasHit = true;

                if (crosshairImage != null)
                    crosshairImage.color = useCrosshairHighlight ? highlightColor : normalColor;

                ShowPromptOnObject(hit);
                return;
            }
        }

        if (crosshairImage != null)
            crosshairImage.color = normalColor;

        if (interactPrompt != null)
            interactPrompt.Hide();
    }

    void ShowPromptOnObject(RaycastHit hit)
    {
        if (interactPrompt == null || cam == null)
            return;

        Collider col = hit.collider;

        if (col == null)
            return;

        Vector3 objectCenter = col.bounds.center;

        Vector3 cameraDirection = cam.transform.position - objectCenter;
        cameraDirection.y = 0f;

        if (cameraDirection.sqrMagnitude < 0.001f)
            cameraDirection = -cam.transform.forward;

        cameraDirection.Normalize();

        Vector3 promptPosition = objectCenter;
        promptPosition += cameraDirection * promptForwardOffset;
        promptPosition += Vector3.up * promptUpOffset;

        interactPrompt.ShowAt(promptPosition);
    }

    void OnDrawGizmosSelected()
    {
        if (cam == null)
            cam = GetComponent<Camera>() ?? Camera.main;

        if (cam == null)
            return;

        Gizmos.color = Color.yellow;

        Vector3 origin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
        Gizmos.DrawLine(origin, origin + cam.transform.forward * interactDistance);
    }
}