using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public float moveSpeed = 5f;    // Ýlerleme hýzý


    void Update()
    {
        Vector3 dir = transform.forward;
        dir.y = 0f;              // Yukarý-aþaðý kýsmý iptal et
        dir.Normalize();         // Uzunluðu 1 yap

        transform.position += dir * moveSpeed * Time.deltaTime;
    }
}
