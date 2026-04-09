using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer = ~0;

    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;

    Camera cam;
    IInteractable currentTarget;

    RaycastHit currentHit;
    bool hasHit;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        if (crosshairImage != null) crosshairImage.color = normalColor;
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
                {
                    wall.SetLastHit(currentHit.point, currentHit.normal);
                }
            }

            currentTarget.OnInteract();
        }
    }

    void CheckForInteractable()
    {
        hasHit = false;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer, QueryTriggerInteraction.Collide))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentTarget = interactable;
                currentHit = hit;
                hasHit = true;

                if (crosshairImage != null) crosshairImage.color = highlightColor;
                return;
            }
        }

        currentTarget = null;
        if (crosshairImage != null) crosshairImage.color = normalColor;
    }

    void OnDrawGizmosSelected()
    {
        if (cam == null) cam = GetComponent<Camera>() ?? Camera.main;
        if (cam == null) return;

        Gizmos.color = Color.yellow;
        Vector3 origin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        Gizmos.DrawLine(origin, origin + cam.transform.forward * interactDistance);
    }
}