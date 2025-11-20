using UnityEngine;

public class EngelSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnInterval = 2f;

    private BoxCollider spawnArea; 

    void Start()
    {

        spawnArea = GetComponent<BoxCollider>();


        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnInterval);
    }

    void SpawnObstacle()
    {
        float halfZ = spawnArea.size.z / 2f;

        float randomZ = Random.Range(-halfZ, halfZ);


        Vector3 localPos = new Vector3(0f, 0f, randomZ);
        Vector3 worldPos = transform.TransformPoint(localPos);


        Instantiate(obstaclePrefab, worldPos, Quaternion.identity);
    }
}
