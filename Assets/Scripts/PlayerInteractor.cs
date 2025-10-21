using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast Ayarlarý")]
    public float interactDistance = 3f;
    public LayerMask interactLayer = ~0; // varsayýlan: her þeyi gör

    [Header("Crosshair")]
    public Image crosshairImage;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;

    private Camera cam;
    private IInteractable currentTarget;

    void Awake()
    {
        // Bu obje Camera deðilse bile güvenli tarafta ol
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        if (crosshairImage != null) crosshairImage.color = normalColor;
    }

    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
        {
            currentTarget.OnInteract();
        }
    }

    void CheckForInteractable()
    {
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer, QueryTriggerInteraction.Collide))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                currentTarget = interactable;
                if (crosshairImage != null) crosshairImage.color = highlightColor;
                return;
            }
        }

        currentTarget = null;
        if (crosshairImage != null) crosshairImage.color = normalColor;
    }

    // Editörde debug çizgisi (isteðe baðlý)
    void OnDrawGizmosSelected()
    {
        if (cam == null) cam = GetComponent<Camera>() ?? Camera.main;
        if (cam == null) return;

        Gizmos.color = Color.yellow;
        Vector3 origin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        Gizmos.DrawLine(origin, origin + cam.transform.forward * interactDistance);
    }
}
