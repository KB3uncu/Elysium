using UnityEngine;

public class ShockwaveEffect : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 20f;    // Bize doðru gelme hýzý
    public float growSpeed = 5f;     // Büyüme hýzý
    public float lifeTime = 1f;      // Kaç saniye sonra yok olsun?

    private Material _mat;
    private Color _startColor;

    void Start()
    {
        Destroy(gameObject, lifeTime);

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            _mat = rend.material;
            if (_mat.HasProperty("_Color")) // Shader'ýnda Color özelliði varsa
            {
                _startColor = _mat.color;
            }
        }
    }

    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        transform.localScale += Vector3.one * growSpeed * Time.deltaTime;

        // duruma göre zamanla silikleþme
        if (_mat != null)
        {
            float alpha = Mathf.Lerp(_startColor.a, 0f, Time.deltaTime * (lifeTime * 2));
            Color newColor = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);
            _mat.color = newColor;
        }
    }
}