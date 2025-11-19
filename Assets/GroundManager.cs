using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    public Transform cameraTransform;   // Kamerayý buraya sürükle
    public GameObject groundPrefab;     // Zemin prefabýný buraya sürükle
    public float tileLength = 10f;      // Bir zemin parçasýnýn uzunluðu (Z ekseni yönünde)
    public int tilesOnScreen = 5;       // Ekranda ayný anda kaç parça olsun
    public float safeZone = 15f;        // Kameranýn ne kadar gerisindekini silelim

    private float nextSpawnZ = 0f;      // Bir sonraki zeminin spawnlanacaðý Z pozisyonu
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        // Baþta ekranda gözükecek kadar zemin spawnla
        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {
        // Kamera ileri gittikçe yeni zemin spawnla
        if (cameraTransform.position.z > nextSpawnZ - tilesOnScreen * tileLength)
        {
            SpawnTile();
        }

        // Kameranýn çok gerisinde kalan ilk zemini sil
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
