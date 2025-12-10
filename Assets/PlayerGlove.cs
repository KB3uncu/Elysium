using UnityEngine;

public class PlayerGlove : MonoBehaviour
{
    public bool hasGlove { get; private set; }

    [SerializeField] private GameObject gloveInHand;

    public void EquipGlove()
    {
        hasGlove = true;
        if (gloveInHand != null)
            gloveInHand.SetActive(true);

    }

    public void ConsumeGlove()
    {
        hasGlove = false;
        if (gloveInHand != null)
            gloveInHand.SetActive(false);

        Debug.Log("PlayerGlove: Eldiven kullanýldý yok oldu");
    }
}
