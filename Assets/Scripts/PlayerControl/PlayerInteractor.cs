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

    [Header("World Interact UI")]
    public WorldInteractUI worldInteractUI;

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

        if (worldInteractUI != null)
            worldInteractUI.HideInstant();
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

                ShowWorldUI(hit);
                return;
            }
        }

        if (crosshairImage != null)
            crosshairImage.color = normalColor;

        if (worldInteractUI != null)
            worldInteractUI.Hide();
    }

    void ShowWorldUI(RaycastHit hit)
    {
        if (worldInteractUI == null)
            return;

        InteractAnchor anchor = hit.collider.GetComponentInParent<InteractAnchor>();

        if (anchor == null)
            anchor = hit.collider.GetComponentInChildren<InteractAnchor>();

        if (anchor == null && hit.collider.transform.parent != null)
            anchor = hit.collider.transform.parent.GetComponentInChildren<InteractAnchor>();

        if (anchor == null)
        {
            worldInteractUI.Hide();
            return;
        }

        worldInteractUI.ShowAtAnchor(anchor.transform);
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