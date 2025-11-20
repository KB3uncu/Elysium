using UnityEngine;

public class TreadmillFloor : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private Renderer rend;
    private float offset;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        offset += scrollSpeed * Time.deltaTime;
        rend.material.mainTextureOffset = new Vector2(offset, 0);
    }
}
