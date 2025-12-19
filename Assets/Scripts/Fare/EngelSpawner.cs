using UnityEngine;

public class EngelSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnInterval = 2f;

    public float minDistance = 1.5f;

    private BoxCollider spawnArea;
    private float lastZ = 9999f;

    void Start()
    {

        spawnArea = GetComponent<BoxCollider>();
     
        
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnInterval);
    }

    void SpawnObstacle()
    {
        float halfZ = spawnArea.size.z / 2f;
        float randomZ;

        
        int safety = 0;

        do
        {
            randomZ = Random.Range(-halfZ, halfZ);
            safety++;
            if (safety > 15)break;
        }
        while (Mathf.Abs(randomZ - lastZ) < minDistance);

        lastZ = randomZ;

        Vector3 localPos = new Vector3(0f, 0f, randomZ);
        Vector3 worldPos = transform.TransformPoint(localPos);


        Instantiate(obstaclePrefab, worldPos, Quaternion.identity);
    }
}
