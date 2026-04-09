using UnityEngine;

public class MouseFinishTrigger : MonoBehaviour
{
    public MinigameResultManager resultManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fare"))
        {
            resultManager.Win();
        }
    }
}