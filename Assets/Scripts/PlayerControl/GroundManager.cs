using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    public Transform cameraTransform;   
    public GameObject groundPrefab;
    public float tileLength = 10f;
    public int tilesOnScreen = 5; 
    public float safeZone = 15f;       

    private float nextSpawnZ = 0f;    
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {

        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {

        if (cameraTransform.position.z > nextSpawnZ - tilesOnScreen * tileLength)
        {
            SpawnTile();
        }


        if (activeTiles.Count > 0)
        {
            GameObject firstTile = activeTiles[0];
            if (cameraTransform.position.z - firstTile.transform.position.z > safeZone)
            {
                Destroy(firstTile);
                activeTiles.RemoveAt(0);
            }
        }
    }

    void SpawnTile()
    {
        Vector3 spawnPos = new Vector3(0f, 0f, nextSpawnZ);
        GameObject go = Instantiate(groundPrefab, spawnPos, Quaternion.identity);
        activeTiles.Add(go);
        nextSpawnZ += tileLength;
    }
}
