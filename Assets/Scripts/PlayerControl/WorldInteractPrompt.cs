using UnityEngine;

public class WorldInteractPrompt : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public Transform visualRoot;

    [Header("Floating")]
    public float floatAmount = 0.08f;
    public float floatSpeed = 3f;

    [Header("Rotation")]
    public bool faceCamera = true;

    [Header("Fade")]
    public float fadeInDuration = 0.25f;
    public float fadeOutDuration = 0.35f;
    public float hideDelay = 1f;

    Camera cam;

    Vector3 basePosition;
    float alpha;
    float hideTimer;

    bool isActive;
    bool wantsVisible;

    void Awake()
    {
        cam = Camera.main;

        if (visualRoot == null)
            visualRoot = transform;

        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>(true);

        HideInstant();
    }

    void LateUpdate()
    {
        if (!isActive)
            return;

        if (cam == null)
            cam = Camera.main;

        if (wantsVisible)
        {
            hideTimer = hideDelay;
            alpha = Mathf.MoveTowards(alpha, 1f, Time.deltaTime / fadeInDuration);
        }
        else
        {
            hideTimer -= Time.deltaTime;

            if (hideTimer <= 0f)
                alpha = Mathf.MoveTowards(alpha, 0f, Time.deltaTime / fadeOutDuration);
        }

        if (canvasGroup != null)
            canvasGroup.alpha = alpha;

        if (!wantsVisible && hideTimer <= 0f && alpha <= 0.01f)
        {
            HideInstant();
            return;
        }

        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = basePosition + Vector3.up * yOffset;

        if (faceCamera && cam != null)
        {
            Vector3 directionToCamera = cam.transform.position - transform.position;

            if (directionToCamera.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
    }

    public void ShowAt(Vector3 position)
    {
        basePosition = position;

        if (visualRoot != null && !visualRoot.gameObject.activeSelf)
            visualRoot.gameObject.SetActive(true);

        isActive = true;
        wantsVisible = true;
    }

    public void Hide()
    {
        wantsVisible = false;
    }

    public void HideInstant()
    {
        isActive = false;
        wantsVisible = false;
        alpha = 0f;
        hideTimer = 0f;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (visualRoot != null)
            visualRoot.gameObject.SetActive(false);
    }
}