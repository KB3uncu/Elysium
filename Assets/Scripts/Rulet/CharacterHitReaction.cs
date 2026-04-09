using System.Collections;
using UnityEngine;

public class CharacterHitReaction : MonoBehaviour
{
    public Animator animator;
    public float standUpDelay = 1.5f;

    Coroutine routine;

    public void PlayFallAndStandUp()
    {
        if (animator == null) return;

        if (routine != null) StopCoroutine(routine);

        animator.ResetTrigger("StandUp");
        animator.SetTrigger("Fall");

        routine = StartCoroutine(StandUpRoutine());
    }

    IEnumerator StandUpRoutine()
    {
        yield return new WaitForSeconds(standUpDelay);

        animator.ResetTrigger("Fall");
        animator.SetTrigger("StandUp");
        routine = null;
    }
}
