using UnityEngine;

public class AnimatorLocomotion : MonoBehaviour
{
    public Animator animator;
    public CharacterController controller;
    public float maxSpeed = 6f;

    void Update()
    {
        if (!animator || !controller) return;

        float speed01 = Mathf.Clamp01(controller.velocity.magnitude / maxSpeed);
        animator.SetFloat("Speed", speed01);
    }
}