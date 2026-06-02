using System.Collections.Generic;
using UnityEngine;

public class GlassParkourGenerator : MonoBehaviour
{
    [Header("References")]
    public GameObject glassPrefab;
    public Transform startPoint;

    [Tooltip("Tablet alaný / final noktasý. Random final açýksa bu obje otomatik taþýnabilir.")]
    public Transform finalPoint;

    [Header("Parent")]
    public Transform generatedParent;

    [Header("Generation")]
    public bool generateOnStart = true;
    public bool clearBeforeGenerate = true;

    [Tooltip("Start ve final dahil toplam cam sayýsý.")]
    public int platformCount = 12;

    [Tooltip("Final mesafeye göre platform sayýsýný sadece yetersizse artýrýr. Fazlaysa azaltmaz.")]
    public bool autoAdjustPlatformCount = true;

    [Header("Step Rules - Her Cam Bir Öncekine Göre")]
    public float minHorizontalStep = 2f;
    public float maxHorizontalStep = 6f;

    public float minVerticalRise = 0f;
    public float maxVerticalRise = 1.2f;

    [Header("Random Final Point")]
    public bool useRandomFinalPoint = true;

    public float finalDistanceMin = 30f;
    public float finalDistanceMax = 40f;

    public float finalHeightMin = 8f;
    public float finalHeightMax = 14f;

    public bool randomFinalInFullCircle = false;
    public float finalAngleRange = 70f;
    public bool useStartForwardAsMainDirection = true;
    public bool moveFinalPointTransform = true;

    [Header("Random Distribution")]
    [Tooltip("0 = tamamen rastgele daðýlýr, 1 = sürekli finale yönelir. Rastgele görünüm için 0 - 0.15 arasý kullan.")]
    [Range(0f, 1f)]
    public float finalDirectionBias = 0.08f;

    [Tooltip("Her cam için kaç farklý rastgele nokta denensin.")]
    public int randomCandidateTryCount = 150;

    [Tooltip("Camlarýn start noktasýndan aþýrý uzaða kaçmasýný engeller.")]
    public bool limitAroundStart = true;

    [Tooltip("Camlar start noktasýnýn bu yarýçapý dýþýna çok çýkmaz.")]
    public float maxDistanceFromStart = 70f;

    [Tooltip("Eski camlarýn üst üste binmesini azaltýr. 0 yaparsan kapalý olur.")]
    public float minDistanceFromOlderPlatforms = 4f;

    [Header("Map Safety Bounds")]
    [Tooltip("Açýlýrsa final ve camlar belirlediðin X/Z sýnýrlarýnýn dýþýna çýkmaz.")]
    public bool useMapBounds = false;

    public Vector2 mapLimitX = new Vector2(-40f, 40f);
    public Vector2 mapLimitZ = new Vector2(-40f, 40f);

    public bool clampFinalInsideBounds = true;
    public bool clampGeneratedGlassesInsideBounds = true;

    [Header("Obstacle Avoidance")]
    [Tooltip("Açýlýrsa camlar ve iki cam arasýndaki yol obstacleMask içindeki colliderlara çarpmamaya çalýþýr.")]
    public bool avoidObstacles = true;

    [Tooltip("Camlarýn içinden geçmemesi gereken objelerin layerý. Örn: ParkourObstacle.")]
    public LayerMask obstacleMask;

    [Tooltip("Camýn yerleþeceði noktada bu kutu kadar alan boþ mu diye kontrol edilir.")]
    public Vector3 glassCheckHalfExtents = new Vector3(1.2f, 0.6f, 1.2f);

    [Tooltip("Ýki cam arasýndaki çizgide engel var mý kontrol eder.")]
    public bool checkPathBetweenGlasses = true;

    [Tooltip("Ýki cam arasýndaki yol kontrolünün kalýnlýðý.")]
    public float pathCheckRadius = 0.6f;

    [Tooltip("SphereCast baþlangýcýný biraz ileri alýr. Kendi camýna çarpmasýný azaltýr.")]
    public float pathCheckStartPadding = 0.2f;

    [Tooltip("SphereCast bitiþini biraz erken keser. Final noktasýnýn kendi colliderýna takýlmasýný azaltýr.")]
    public float pathCheckEndPadding = 0.8f;

    [Header("Fragile / Breaking Glass")]
    [Tooltip("Oyuncu üstüne basýnca 3-5 saniye içinde kýrýlacak kýrýk cam prefabý.")]
    public GameObject fragileGlassPrefab;

    [Range(0f, 1f)]
    [Tooltip("0.25 = camlarýn yaklaþýk %25'i kýrýk cam olur.")]
    public float fragileGlassChance = 0.25f;

    [Tooltip("Baþlangýçtaki kaç cam kesinlikle saðlam olsun.")]
    public int safePlatformsAtStart = 2;

    [Tooltip("Sondaki kaç cam kesinlikle saðlam olsun.")]
    public int safePlatformsAtEnd = 2;

    [Tooltip("Kýrýk cam prefabýnda StepBreakGlass yoksa otomatik ekler.")]
    public bool autoAddStepBreakScript = true;

    public float fragileMinBreakDelay = 3f;
    public float fragileMaxBreakDelay = 5f;

    [Header("Fragile Glass Break Force")]
    public float fragileOutwardForce = 1.5f;
    public float fragileUpwardForce = 0.4f;
    public float fragileRandomForce = 1.2f;
    public float fragileTorqueForce = 4f;
    public float fragileDestroyPiecesAfter = 5f;

    [Header("Glass Rotation")]
    public bool faceNextGlass = true;
    public float rotationRandomness = 8f;

    [Header("Glass Scale")]
    public bool randomizeScale = false;
    public Vector3 minScale = Vector3.one;
    public Vector3 maxScale = Vector3.one;

    [Header("Random Seed")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    [Header("Debug")]
    public bool drawGizmos = true;
    public bool drawMapBoundsGizmo = false;
    public Color gizmoLineColor = Color.yellow;
    public Color gizmoStartColor = Color.cyan;
    public Color gizmoFinalColor = Color.magenta;
    public Color gizmoBoundsColor = Color.green;

    private readonly List<GameObject> spawnedGlasses = new List<GameObject>();
    private readonly List<Vector3> generatedPositions = new List<Vector3>();

    void Start()
    {
        if (generateOnStart)
            GenerateParkour();
    }

    [ContextMenu("Generate Parkour")]
    public void GenerateParkour()
    {
        if (glassPrefab == null)
        {
            Debug.LogWarning("GlassParkourGenerator: Glass Prefab atanmadý.");
            return;
        }

        if (startPoint == null)
        {
            Debug.LogWarning("GlassParkourGenerator: Start Point atanmadý.");
            return;
        }

        if (clearBeforeGenerate)
            ClearGeneratedParkour();

        if (generatedParent == null)
            generatedParent = transform;

        int currentSeed;

        if (useRandomSeed)
            currentSeed = seed;
        else
            currentSeed = System.Environment.TickCount + Random.Range(0, 999999);

        System.Random random = new System.Random(currentSeed);

        Vector3 startPos = startPoint.position;
        Vector3 finalPos = GetFinalPosition(random, startPos);

        if (useMapBounds && clampFinalInsideBounds)
            finalPos = ClampToMapBounds(finalPos);

        int count = Mathf.Max(2, platformCount);

        if (autoAdjustPlatformCount)
            count = GetAdjustedPlatformCount(startPos, finalPos, count);

        ClampFinalHeightToStepRules(ref finalPos, startPos, count);

        if (useMapBounds && clampFinalInsideBounds)
            finalPos = ClampToMapBounds(finalPos);

        if (finalPoint != null && moveFinalPointTransform)
            finalPoint.position = finalPos;

        GenerateRandomWalkPositions(random, startPos, finalPos, count);
        SpawnGlasses(random);
    }

    Vector3 GetFinalPosition(System.Random random, Vector3 startPos)
    {
        if (!useRandomFinalPoint && finalPoint != null)
            return finalPoint.position;

        float distance = RandomRange(random, finalDistanceMin, finalDistanceMax);
        float height = RandomRange(random, finalHeightMin, finalHeightMax);

        Vector3 mainDirection;

        if (useStartForwardAsMainDirection && startPoint != null)
            mainDirection = startPoint.forward;
        else
            mainDirection = transform.forward;

        mainDirection.y = 0f;

        if (mainDirection.sqrMagnitude < 0.01f)
            mainDirection = Vector3.forward;

        mainDirection.Normalize();

        float angle;

        if (randomFinalInFullCircle)
            angle = RandomRange(random, 0f, 360f);
        else
            angle = RandomRange(random, -finalAngleRange, finalAngleRange);

        Vector3 finalDirection = Quaternion.Euler(0f, angle, 0f) * mainDirection;

        Vector3 finalPos = startPos + finalDirection * distance;
        finalPos.y = startPos.y + height;

        return finalPos;
    }

    int GetAdjustedPlatformCount(Vector3 startPos, Vector3 finalPos, int desiredCount)
    {
        float distance = GetFlatDistance(startPos, finalPos);
        float height = Mathf.Max(0f, finalPos.y - startPos.y);

        int minJumpsForDistance = Mathf.CeilToInt(distance / Mathf.Max(0.01f, maxHorizontalStep));
        int minJumpsForHeight = Mathf.CeilToInt(height / Mathf.Max(0.01f, maxVerticalRise));

        int minJumps = Mathf.Max(1, minJumpsForDistance, minJumpsForHeight);
        int desiredJumps = Mathf.Max(1, desiredCount - 1);

        int adjustedJumps = Mathf.Max(desiredJumps, minJumps);
        int adjustedCount = adjustedJumps + 1;

        if (adjustedCount != desiredCount)
        {
            Debug.Log(
                "GlassParkourGenerator: Platform sayýsý ulaþýlabilirlik için artýrýldý. " +
                "Eski: " + desiredCount + " Yeni: " + adjustedCount
            );
        }

        return adjustedCount;
    }

    void ClampFinalHeightToStepRules(ref Vector3 finalPos, Vector3 startPos, int count)
    {
        int jumps = Mathf.Max(1, count - 1);

        float minPossibleHeight = startPos.y + minVerticalRise * jumps;
        float maxPossibleHeight = startPos.y + maxVerticalRise * jumps;

        if (finalPos.y < minPossibleHeight)
            finalPos.y = minPossibleHeight;

        if (finalPos.y > maxPossibleHeight)
            finalPos.y = maxPossibleHeight;
    }

    void GenerateRandomWalkPositions(System.Random random, Vector3 startPos, Vector3 finalPos, int count)
    {
        generatedPositions.Clear();
        generatedPositions.Add(startPos);

        Vector3 previousPos = startPos;

        for (int i = 1; i < count - 1; i++)
        {
            int remainingStepsAfterThis = (count - 1) - i;

            Vector3 bestCandidate = previousPos;
            float bestScore = float.MaxValue;
            bool foundValidCandidate = false;

            float verticalRise = PickVerticalRise(
                random,
                previousPos.y,
                finalPos.y,
                remainingStepsAfterThis
            );

            for (int attempt = 0; attempt < randomCandidateTryCount; attempt++)
            {
                Vector3 direction = GetRandomDirectionWithSmallFinalBias(random, previousPos, finalPos);

                float horizontalStep = RandomRange(random, minHorizontalStep, maxHorizontalStep);

                Vector3 candidate = previousPos + direction * horizontalStep;
                candidate.y = previousPos.y + verticalRise;

                if (useMapBounds && clampGeneratedGlassesInsideBounds)
                    candidate = ClampToMapBounds(candidate);

                if (IsCandidateBlocked(previousPos, candidate, finalPos, remainingStepsAfterThis))
                    continue;

                float score = GetCandidateScore(candidate, startPos, finalPos, remainingStepsAfterThis);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidate;
                    foundValidCandidate = true;
                }

                if (score <= 0.001f)
                    break;
            }

            if (!foundValidCandidate)
            {
                Vector3 toFinal = finalPos - previousPos;
                toFinal.y = 0f;

                if (toFinal.sqrMagnitude > 0.01f)
                {
                    Vector3 directionToFinal = toFinal.normalized;

                    float safeStep = Mathf.Clamp(
                        toFinal.magnitude / Mathf.Max(1, remainingStepsAfterThis + 1),
                        minHorizontalStep,
                        maxHorizontalStep
                    );

                    Vector3 safeCandidate = previousPos + directionToFinal * safeStep;
                    safeCandidate.y = previousPos.y + verticalRise;

                    if (useMapBounds && clampGeneratedGlassesInsideBounds)
                        safeCandidate = ClampToMapBounds(safeCandidate);

                    if (!IsCandidateBlocked(previousPos, safeCandidate, finalPos, remainingStepsAfterThis))
                    {
                        float safeScore = GetCandidateScore(safeCandidate, startPos, finalPos, remainingStepsAfterThis);

                        if (safeScore < bestScore)
                        {
                            bestCandidate = safeCandidate;
                            bestScore = safeScore;
                            foundValidCandidate = true;
                        }
                    }
                }
            }

            if (!foundValidCandidate)
            {
                Vector3 emergencyCandidate;

                if (TryFindEmergencyCandidate(
                    random,
                    previousPos,
                    startPos,
                    finalPos,
                    verticalRise,
                    remainingStepsAfterThis,
                    out emergencyCandidate))
                {
                    bestCandidate = emergencyCandidate;
                    foundValidCandidate = true;
                }
            }

            if (!foundValidCandidate)
            {
                Debug.LogWarning(
                    "GlassParkourGenerator: " + (i + 1) +
                    ". cam için engelsiz nokta bulunamadý. " +
                    "Obstacle collider çok büyük olabilir veya final noktasý engelin arkasýnda kalýyor olabilir."
                );

                Vector3 toFinal = finalPos - previousPos;
                toFinal.y = 0f;

                if (toFinal.sqrMagnitude > 0.01f)
                {
                    Vector3 directionToFinal = toFinal.normalized;
                    float fallbackStep = Mathf.Clamp(
                        toFinal.magnitude / Mathf.Max(1, remainingStepsAfterThis + 1),
                        minHorizontalStep,
                        maxHorizontalStep
                    );

                    bestCandidate = previousPos + directionToFinal * fallbackStep;
                    bestCandidate.y = previousPos.y + verticalRise;

                    if (useMapBounds && clampGeneratedGlassesInsideBounds)
                        bestCandidate = ClampToMapBounds(bestCandidate);
                }
            }

            generatedPositions.Add(bestCandidate);
            previousPos = bestCandidate;
        }

        generatedPositions.Add(finalPos);
    }

    bool TryFindEmergencyCandidate(
        System.Random random,
        Vector3 previousPos,
        Vector3 startPos,
        Vector3 finalPos,
        float verticalRise,
        int remainingStepsAfterThis,
        out Vector3 result)
    {
        result = previousPos;

        Vector3 toFinal = finalPos - previousPos;
        toFinal.y = 0f;

        Vector3 baseDirection;

        if (toFinal.sqrMagnitude > 0.01f)
            baseDirection = toFinal.normalized;
        else
            baseDirection = Vector3.forward;

        float bestScore = float.MaxValue;
        bool found = false;

        int emergencyTryCount = Mathf.Max(80, randomCandidateTryCount);

        for (int i = 0; i < emergencyTryCount; i++)
        {
            float sideAngle = RandomRange(random, -120f, 120f);
            Vector3 direction = Quaternion.Euler(0f, sideAngle, 0f) * baseDirection;

            float step = RandomRange(random, minHorizontalStep, maxHorizontalStep);

            Vector3 candidate = previousPos + direction.normalized * step;
            candidate.y = previousPos.y + verticalRise;

            if (useMapBounds && clampGeneratedGlassesInsideBounds)
                candidate = ClampToMapBounds(candidate);

            if (IsCandidateBlocked(previousPos, candidate, finalPos, remainingStepsAfterThis))
                continue;

            float score = GetCandidateScore(candidate, startPos, finalPos, remainingStepsAfterThis);

            if (score < bestScore)
            {
                bestScore = score;
                result = candidate;
                found = true;
            }
        }

        return found;
    }

    bool IsCandidateBlocked(Vector3 previousPos, Vector3 candidate, Vector3 finalPos, int remainingStepsAfterThis)
    {
        if (!avoidObstacles)
            return false;

        if (obstacleMask.value == 0)
            return false;

        if (IsPositionBlocked(candidate))
            return true;

        if (checkPathBetweenGlasses && IsPathBlocked(previousPos, candidate))
            return true;

        if (remainingStepsAfterThis == 1 && checkPathBetweenGlasses && IsPathBlocked(candidate, finalPos))
            return true;

        return false;
    }

    bool IsPositionBlocked(Vector3 position)
    {
        if (!avoidObstacles)
            return false;

        if (obstacleMask.value == 0)
            return false;

        return Physics.CheckBox(
            position,
            glassCheckHalfExtents,
            Quaternion.identity,
            obstacleMask,
            QueryTriggerInteraction.Collide
        );
    }

    bool IsPathBlocked(Vector3 from, Vector3 to)
    {
        if (!avoidObstacles)
            return false;

        if (obstacleMask.value == 0)
            return false;

        Vector3 direction = to - from;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
            return false;

        direction.Normalize();

        float startPadding = Mathf.Clamp(pathCheckStartPadding, 0f, distance * 0.45f);
        float endPadding = Mathf.Clamp(pathCheckEndPadding, 0f, distance * 0.45f);

        float castDistance = distance - startPadding - endPadding;

        if (castDistance <= 0.01f)
            return false;

        Vector3 castStart = from + direction * startPadding;

        return Physics.SphereCast(
            castStart,
            pathCheckRadius,
            direction,
            out RaycastHit hit,
            castDistance,
            obstacleMask,
            QueryTriggerInteraction.Collide
        );
    }

    Vector3 GetRandomDirectionWithSmallFinalBias(System.Random random, Vector3 currentPos, Vector3 finalPos)
    {
        float randomAngle = RandomRange(random, 0f, 360f);
        Vector3 randomDir = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        Vector3 toFinal = finalPos - currentPos;
        toFinal.y = 0f;

        if (toFinal.sqrMagnitude < 0.01f)
            return randomDir.normalized;

        Vector3 finalDir = toFinal.normalized;

        Vector3 mixedDir = (randomDir * (1f - finalDirectionBias)) + (finalDir * finalDirectionBias);

        if (mixedDir.sqrMagnitude < 0.01f)
            return randomDir.normalized;

        return mixedDir.normalized;
    }

    float GetCandidateScore(Vector3 candidate, Vector3 startPos, Vector3 finalPos, int remainingSteps)
    {
        float score = 0f;

        float remainingDistanceToFinal = GetFlatDistance(candidate, finalPos);
        float maxReachableDistance = maxHorizontalStep * remainingSteps;

        if (remainingDistanceToFinal > maxReachableDistance)
            score += (remainingDistanceToFinal - maxReachableDistance) * 10f;

        float remainingHeight = finalPos.y - candidate.y;
        float maxReachableHeight = maxVerticalRise * remainingSteps;
        float minReachableHeight = minVerticalRise * remainingSteps;

        if (remainingHeight < minReachableHeight)
            score += (minReachableHeight - remainingHeight) * 10f;

        if (remainingHeight > maxReachableHeight)
            score += (remainingHeight - maxReachableHeight) * 10f;

        if (limitAroundStart)
        {
            float distanceFromStart = GetFlatDistance(candidate, startPos);

            if (distanceFromStart > maxDistanceFromStart)
                score += (distanceFromStart - maxDistanceFromStart) * 4f;
        }

        if (minDistanceFromOlderPlatforms > 0f)
        {
            for (int i = 0; i < generatedPositions.Count - 1; i++)
            {
                float d = GetFlatDistance(candidate, generatedPositions[i]);

                if (d < minDistanceFromOlderPlatforms)
                    score += (minDistanceFromOlderPlatforms - d) * 2f;
            }
        }

        return score;
    }

    float PickVerticalRise(System.Random random, float currentY, float finalY, int remainingStepsAfterThis)
    {
        float remainingHeight = Mathf.Max(0f, finalY - currentY);

        float minFutureHeight = minVerticalRise * remainingStepsAfterThis;
        float maxFutureHeight = maxVerticalRise * remainingStepsAfterThis;

        float allowedMin = Mathf.Max(minVerticalRise, remainingHeight - maxFutureHeight);
        float allowedMax = Mathf.Min(maxVerticalRise, remainingHeight - minFutureHeight);

        if (allowedMin > allowedMax)
        {
            return Mathf.Clamp(
                remainingHeight / Mathf.Max(1, remainingStepsAfterThis + 1),
                minVerticalRise,
                maxVerticalRise
            );
        }

        return RandomRange(random, allowedMin, allowedMax);
    }

    void SpawnGlasses(System.Random random)
    {
        for (int i = 0; i < generatedPositions.Count; i++)
        {
            Vector3 pos = generatedPositions[i];

            Quaternion rot = Quaternion.identity;

            if (faceNextGlass)
            {
                Vector3 lookDirection;

                if (i < generatedPositions.Count - 1)
                    lookDirection = generatedPositions[i + 1] - pos;
                else
                    lookDirection = pos - generatedPositions[i - 1];

                lookDirection.y = 0f;

                if (lookDirection.sqrMagnitude > 0.01f)
                    rot = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }

            float randomY = RandomRange(random, -rotationRandomness, rotationRandomness);
            rot *= Quaternion.Euler(0f, randomY, 0f);

            bool canBeFragile =
                fragileGlassPrefab != null &&
                i >= safePlatformsAtStart &&
                i < generatedPositions.Count - safePlatformsAtEnd;

            bool spawnFragile =
                canBeFragile &&
                RandomRange(random, 0f, 1f) <= fragileGlassChance;

            GameObject selectedPrefab = spawnFragile ? fragileGlassPrefab : glassPrefab;

            GameObject glass = Instantiate(selectedPrefab, pos, rot, generatedParent);

            if (spawnFragile)
                glass.name = "Generated_FragileGlass_" + (i + 1);
            else
                glass.name = "Generated_Glass_" + (i + 1);

            if (spawnFragile && autoAddStepBreakScript)
            {
                StepBreakGlass stepBreakGlass = glass.GetComponent<StepBreakGlass>();

                if (stepBreakGlass == null)
                    stepBreakGlass = glass.AddComponent<StepBreakGlass>();

                stepBreakGlass.minBreakDelay = fragileMinBreakDelay;
                stepBreakGlass.maxBreakDelay = fragileMaxBreakDelay;

                stepBreakGlass.outwardForce = fragileOutwardForce;
                stepBreakGlass.upwardForce = fragileUpwardForce;
                stepBreakGlass.randomForce = fragileRandomForce;
                stepBreakGlass.torqueForce = fragileTorqueForce;
                stepBreakGlass.destroyPiecesAfter = fragileDestroyPiecesAfter;
            }

            if (randomizeScale)
            {
                float sx = RandomRange(random, minScale.x, maxScale.x);
                float sy = RandomRange(random, minScale.y, maxScale.y);
                float sz = RandomRange(random, minScale.z, maxScale.z);

                glass.transform.localScale = new Vector3(sx, sy, sz);
            }

            spawnedGlasses.Add(glass);
        }
    }

    float GetFlatDistance(Vector3 a, Vector3 b)
    {
        Vector3 flatA = new Vector3(a.x, 0f, a.z);
        Vector3 flatB = new Vector3(b.x, 0f, b.z);

        return Vector3.Distance(flatA, flatB);
    }

    Vector3 ClampToMapBounds(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, mapLimitX.x, mapLimitX.y);
        pos.z = Mathf.Clamp(pos.z, mapLimitZ.x, mapLimitZ.y);
        return pos;
    }

    float RandomRange(System.Random random, float min, float max)
    {
        if (max < min)
        {
            float temp = min;
            min = max;
            max = temp;
        }

        return Mathf.Lerp(min, max, (float)random.NextDouble());
    }

    [ContextMenu("Clear Generated Parkour")]
    public void ClearGeneratedParkour()
    {
        for (int i = spawnedGlasses.Count - 1; i >= 0; i--)
        {
            if (spawnedGlasses[i] != null)
            {
                if (Application.isPlaying)
                    Destroy(spawnedGlasses[i]);
                else
                    DestroyImmediate(spawnedGlasses[i]);
            }
        }

        spawnedGlasses.Clear();
        generatedPositions.Clear();

        if (generatedParent != null)
        {
            for (int i = generatedParent.childCount - 1; i >= 0; i--)
            {
                Transform child = generatedParent.GetChild(i);

                bool isGeneratedGlass =
                    child.name.StartsWith("Generated_Glass_") ||
                    child.name.StartsWith("Generated_FragileGlass_");

                if (isGeneratedGlass)
                {
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    void OnValidate()
    {
        platformCount = Mathf.Max(2, platformCount);

        minHorizontalStep = Mathf.Max(0.1f, minHorizontalStep);
        maxHorizontalStep = Mathf.Max(minHorizontalStep, maxHorizontalStep);

        minVerticalRise = Mathf.Max(0f, minVerticalRise);
        maxVerticalRise = Mathf.Max(minVerticalRise, maxVerticalRise);

        finalDistanceMin = Mathf.Max(0f, finalDistanceMin);
        finalDistanceMax = Mathf.Max(finalDistanceMin, finalDistanceMax);

        finalHeightMin = Mathf.Max(0f, finalHeightMin);
        finalHeightMax = Mathf.Max(finalHeightMin, finalHeightMax);

        finalAngleRange = Mathf.Clamp(finalAngleRange, 0f, 180f);

        randomCandidateTryCount = Mathf.Max(1, randomCandidateTryCount);
        maxDistanceFromStart = Mathf.Max(1f, maxDistanceFromStart);
        minDistanceFromOlderPlatforms = Mathf.Max(0f, minDistanceFromOlderPlatforms);

        safePlatformsAtStart = Mathf.Max(0, safePlatformsAtStart);
        safePlatformsAtEnd = Mathf.Max(0, safePlatformsAtEnd);

        fragileMinBreakDelay = Mathf.Max(0f, fragileMinBreakDelay);
        fragileMaxBreakDelay = Mathf.Max(fragileMinBreakDelay, fragileMaxBreakDelay);

        fragileDestroyPiecesAfter = Mathf.Max(0.1f, fragileDestroyPiecesAfter);

        if (mapLimitX.y < mapLimitX.x)
        {
            float temp = mapLimitX.x;
            mapLimitX.x = mapLimitX.y;
            mapLimitX.y = temp;
        }

        if (mapLimitZ.y < mapLimitZ.x)
        {
            float temp = mapLimitZ.x;
            mapLimitZ.x = mapLimitZ.y;
            mapLimitZ.y = temp;
        }

        glassCheckHalfExtents.x = Mathf.Max(0.05f, glassCheckHalfExtents.x);
        glassCheckHalfExtents.y = Mathf.Max(0.05f, glassCheckHalfExtents.y);
        glassCheckHalfExtents.z = Mathf.Max(0.05f, glassCheckHalfExtents.z);

        pathCheckRadius = Mathf.Max(0.05f, pathCheckRadius);
        pathCheckStartPadding = Mathf.Max(0f, pathCheckStartPadding);
        pathCheckEndPadding = Mathf.Max(0f, pathCheckEndPadding);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        if (startPoint != null)
        {
            Gizmos.color = gizmoStartColor;
            Gizmos.DrawSphere(startPoint.position, 0.35f);
        }

        if (finalPoint != null)
        {
            Gizmos.color = gizmoFinalColor;
            Gizmos.DrawSphere(finalPoint.position, 0.35f);
        }

        if (drawMapBoundsGizmo)
        {
            Gizmos.color = gizmoBoundsColor;

            float centerX = (mapLimitX.x + mapLimitX.y) * 0.5f;
            float centerZ = (mapLimitZ.x + mapLimitZ.y) * 0.5f;

            float sizeX = Mathf.Abs(mapLimitX.y - mapLimitX.x);
            float sizeZ = Mathf.Abs(mapLimitZ.y - mapLimitZ.x);

            float y = startPoint != null ? startPoint.position.y : transform.position.y;

            Gizmos.DrawWireCube(
                new Vector3(centerX, y, centerZ),
                new Vector3(sizeX, 1f, sizeZ)
            );
        }

        if (generatedPositions == null || generatedPositions.Count < 2)
            return;

        Gizmos.color = gizmoLineColor;

        for (int i = 0; i < generatedPositions.Count - 1; i++)
        {
            Gizmos.DrawLine(generatedPositions[i], generatedPositions[i + 1]);
            Gizmos.DrawSphere(generatedPositions[i], 0.15f);
        }

        Gizmos.DrawSphere(generatedPositions[generatedPositions.Count - 1], 0.15f);
    }
}