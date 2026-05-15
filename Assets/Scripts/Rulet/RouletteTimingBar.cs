using System.Collections.Generic;
using UnityEngine;

public class RouletteTimingBar : MonoBehaviour
{
    [System.Serializable]
    public class DifficultyStep
    {
        public string stepName = "Step";

        [Header("Zorluk")]
        public float needleSpeed = 3.2f;

        [Tooltip("Çubuk yeþil noktaya kaç derece yakýnsa baþarýlý saysýn.")]
        public float successAngleTolerance = 10f;

        [Tooltip("Yeþil noktanýn görsel büyüklüðü.")]
        public float targetDotSize = 0.035f;

        [Tooltip("Yeþil noktanýn çýkabileceði alan. 0.5 daha merkezde, 1.0 tüm yarým çemberde.")]
        [Range(0.1f, 1f)]
        public float targetSpawnRange = 0.75f;
    }

    [Header("General")]
    public bool buildVisualOnAwake = true;
    public bool faceCamera = true;
    public Camera targetCamera;

    [Header("Arc Settings")]
    public float radius = 0.22f;
    public float maxAngle = 90f;
    public int arcSegmentCount = 19;

    [Header("Default Gameplay")]
    public float defaultNeedleSpeed = 3.2f;
    public float defaultSuccessAngleTolerance = 10f;
    public float defaultTargetDotSize = 0.035f;
    [Range(0.1f, 1f)] public float defaultTargetSpawnRange = 0.75f;

    [Header("Difficulty By Enemy Hit Count")]
    public DifficultyStep[] difficultySteps =
    {
        new DifficultyStep
        {
            stepName = "First Shot - Easy",
            needleSpeed = 2.7f,
            successAngleTolerance = 13f,
            targetDotSize = 0.045f,
            targetSpawnRange = 0.65f
        },
        new DifficultyStep
        {
            stepName = "Second Shot - Medium",
            needleSpeed = 3.5f,
            successAngleTolerance = 9f,
            targetDotSize = 0.035f,
            targetSpawnRange = 0.8f
        },
        new DifficultyStep
        {
            stepName = "Final Shot - Hard",
            needleSpeed = 4.5f,
            successAngleTolerance = 6.5f,
            targetDotSize = 0.027f,
            targetSpawnRange = 1f
        }
    };

    [Header("Visual Sizes")]
    public float arcSegmentLength = 0.035f;
    public float arcSegmentThickness = 0.008f;
    public float needleThickness = 0.012f;

    [Header("Colors")]
    public Color arcColor = new Color(0.05f, 0.05f, 0.05f, 1f);
    public Color needleColor = Color.white;
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    Transform visualRoot;
    Transform needlePivot;
    Transform needle;
    Transform targetDot;

    readonly List<GameObject> spawnedObjects = new List<GameObject>();

    float currentAngle;
    float targetAngle;
    float timer;
    bool isRunning;
    bool isBuilt;

    float currentNeedleSpeed;
    float currentSuccessAngleTolerance;
    float currentTargetDotSize;
    float currentTargetSpawnRange;

    void Awake()
    {
        currentNeedleSpeed = defaultNeedleSpeed;
        currentSuccessAngleTolerance = defaultSuccessAngleTolerance;
        currentTargetDotSize = defaultTargetDotSize;
        currentTargetSpawnRange = defaultTargetSpawnRange;

        if (buildVisualOnAwake)
            BuildVisual();

        Hide();
    }

    void Update()
    {
        if (!isRunning)
            return;

        timer += Time.deltaTime * currentNeedleSpeed;

        currentAngle = Mathf.Sin(timer) * maxAngle;
        ApplyNeedleAngle(currentAngle);
    }

    void LateUpdate()
    {
        if (!faceCamera || visualRoot == null || !visualRoot.gameObject.activeSelf)
            return;

        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null)
            return;

        Vector3 dir = visualRoot.position - cam.transform.position;
        if (dir.sqrMagnitude > 0.001f)
            visualRoot.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    public void ShowAndRandomizeTarget()
    {
        ShowAndRandomizeTarget(0);
    }

    public void ShowAndRandomizeTarget(int enemySuccessfulHitCount)
    {
        ApplyDifficulty(enemySuccessfulHitCount);

        if (!isBuilt)
            BuildVisual();

        if (visualRoot == null)
            return;

        visualRoot.gameObject.SetActive(true);

        float spawnLimit = maxAngle * currentTargetSpawnRange;
        targetAngle = Random.Range(-spawnLimit, spawnLimit);

        if (targetDot != null)
        {
            targetDot.localPosition = AngleToLocalPosition(targetAngle);
            targetDot.localScale = Vector3.one * currentTargetDotSize;
            SetRendererColor(targetDot, successColor);
        }

        SetRendererColor(needle, needleColor);

        timer = Random.Range(0f, 10f);
        isRunning = true;

    }

    public bool StopAndCheck()
    {
        if (!isRunning)
            return false;

        isRunning = false;

        float difference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));
        bool success = difference <= currentSuccessAngleTolerance;

        SetRendererColor(needle, success ? successColor : failColor);

        return success;
    }

    public void Hide()
    {
        isRunning = false;

        if (visualRoot != null)
            visualRoot.gameObject.SetActive(false);
    }

    void ApplyDifficulty(int enemySuccessfulHitCount)
    {
        if (difficultySteps == null || difficultySteps.Length == 0)
        {
            currentNeedleSpeed = defaultNeedleSpeed;
            currentSuccessAngleTolerance = defaultSuccessAngleTolerance;
            currentTargetDotSize = defaultTargetDotSize;
            currentTargetSpawnRange = defaultTargetSpawnRange;
            return;
        }

        int index = Mathf.Clamp(enemySuccessfulHitCount, 0, difficultySteps.Length - 1);
        DifficultyStep step = difficultySteps[index];

        currentNeedleSpeed = Mathf.Max(0.1f, step.needleSpeed);
        currentSuccessAngleTolerance = Mathf.Max(0.1f, step.successAngleTolerance);
        currentTargetDotSize = Mathf.Max(0.001f, step.targetDotSize);
        currentTargetSpawnRange = Mathf.Clamp(step.targetSpawnRange, 0.1f, 1f);
    }

    void BuildVisual()
    {
        ClearBuiltVisual();

        GameObject rootObj = new GameObject("TimingBarVisual");
        rootObj.transform.SetParent(transform);
        rootObj.transform.localPosition = Vector3.zero;
        rootObj.transform.localRotation = Quaternion.identity;
        rootObj.transform.localScale = Vector3.one;
        rootObj.layer = gameObject.layer;

        visualRoot = rootObj.transform;

        BuildArc();
        BuildTargetDot();
        BuildNeedle();

        isBuilt = true;
    }

    void BuildArc()
    {
        if (visualRoot == null)
            return;

        int count = Mathf.Max(3, arcSegmentCount);

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0f : (float)i / (count - 1);
            float angle = Mathf.Lerp(-maxAngle, maxAngle, t);

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = "ArcSegment";
            segment.layer = gameObject.layer;

            DestroyCollider(segment);

            segment.transform.SetParent(visualRoot);
            segment.transform.localPosition = AngleToLocalPosition(angle);
            segment.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            segment.transform.localScale = new Vector3(arcSegmentLength, arcSegmentThickness, arcSegmentThickness);

            SetRendererColor(segment.transform, arcColor);
            spawnedObjects.Add(segment);
        }
    }

    void BuildTargetDot()
    {
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.name = "GreenTargetDot";
        dot.layer = gameObject.layer;

        DestroyCollider(dot);

        dot.transform.SetParent(visualRoot);
        dot.transform.localPosition = AngleToLocalPosition(0f);
        dot.transform.localRotation = Quaternion.identity;
        dot.transform.localScale = Vector3.one * defaultTargetDotSize;

        SetRendererColor(dot.transform, successColor);

        targetDot = dot.transform;
        spawnedObjects.Add(dot);
    }

    void BuildNeedle()
    {
        GameObject pivotObj = new GameObject("NeedlePivot");
        pivotObj.layer = gameObject.layer;

        pivotObj.transform.SetParent(visualRoot);
        pivotObj.transform.localPosition = Vector3.zero;
        pivotObj.transform.localRotation = Quaternion.identity;
        pivotObj.transform.localScale = Vector3.one;

        needlePivot = pivotObj.transform;
        spawnedObjects.Add(pivotObj);

        GameObject needleObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        needleObj.name = "Needle";
        needleObj.layer = gameObject.layer;

        DestroyCollider(needleObj);

        needleObj.transform.SetParent(needlePivot);
        needleObj.transform.localPosition = new Vector3(0f, radius * 0.5f, -0.01f);
        needleObj.transform.localRotation = Quaternion.identity;
        needleObj.transform.localScale = new Vector3(needleThickness, radius, needleThickness);

        SetRendererColor(needleObj.transform, needleColor);

        needle = needleObj.transform;
        spawnedObjects.Add(needleObj);
    }

    Vector3 AngleToLocalPosition(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;

        float x = Mathf.Sin(rad) * radius;
        float y = Mathf.Cos(rad) * radius;

        return new Vector3(x, y, 0f);
    }

    void ApplyNeedleAngle(float angle)
    {
        if (needlePivot == null)
            return;

        needlePivot.localRotation = Quaternion.Euler(0f, 0f, -angle);
    }

    void SetRendererColor(Transform target, Color color)
    {
        if (target == null)
            return;

        Renderer r = target.GetComponent<Renderer>();
        if (r == null)
            return;

        r.material.color = color;
    }

    void DestroyCollider(GameObject obj)
    {
        Collider col = obj.GetComponent<Collider>();
        if (col != null)
            Destroy(col);
    }

    void ClearBuiltVisual()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
                Destroy(spawnedObjects[i]);
        }

        spawnedObjects.Clear();

        if (visualRoot != null)
            Destroy(visualRoot.gameObject);

        visualRoot = null;
        needlePivot = null;
        needle = null;
        targetDot = null;
        isBuilt = false;
    }
}