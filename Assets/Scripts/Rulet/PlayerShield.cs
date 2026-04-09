using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [Header("Shield Visuals")]
    public GameObject handHealthy;
    public GameObject handDamaged;
    public GameObject tableHealthy;
    public GameObject tableDamaged;

    [Header("Shield State")]
    public bool hasShield = false;
    public int usesLeft = 0;
    public bool isDamaged = false;
    public bool isInHand = false;

    // Bu round enemy ateþ etmeden önce oyuncu kullanmak isterse true olacak
    public bool willUseThisShot = false;

    void Start()
    {
        UpdateVisuals();
    }

    public void GiveShield()
    {
        hasShield = true;
        usesLeft = 2;
        isDamaged = false;
        willUseThisShot = false;
        isInHand = false;

        UpdateVisuals();
        Debug.Log("Shield spawned on table.");
    }
    public void PickUpShield()
    {
        if (!hasShield || usesLeft <= 0) return;

        isInHand = true;
        UpdateVisuals();

        Debug.Log("Shield picked up. Now shield is in hand.");
    }

    public bool CanOfferShield()
    {
        return hasShield && usesLeft > 0;
    }

    public void ChooseToUseShield(bool useShield)
    {
        if (!CanOfferShield()) return;

        willUseThisShot = useShield;
        Debug.Log("Shield choice: " + useShield);
    }

    public bool ConsumeShieldAgainstShot(bool bulletWasReal)
    {
        if (!hasShield || usesLeft <= 0 || !willUseThisShot)
            return false;

        int oldUses = usesLeft;
        usesLeft--;

        bool blocked = false;

        if (bulletWasReal)
        {
            blocked = true;

            // Ýlk kullaným ve gerçek mermi geldiyse hasarlýya geç
            if (oldUses == 2)
            {
                isDamaged = true;
            }
        }

        // Kullaným hakký biterse kalkan yok olur
        if (usesLeft <= 0)
        {
            hasShield = false;
            isDamaged = false;
            isInHand = false ;
        }

        willUseThisShot = false;
        UpdateVisuals();

        Debug.Log($"Shield used. BulletReal:{bulletWasReal} | Blocked:{blocked} | UsesLeft:{usesLeft}");
        return blocked;
    }

    public void PutShieldBackToTable()
    {
        if (!hasShield || usesLeft <= 0)
        {
            isInHand = false;
            UpdateVisuals();
            return;
        }

        isInHand = false;
        willUseThisShot = false;
        UpdateVisuals();

        Debug.Log("Shield returned to table.");
    }

    public void ClearDecision()
    {
        willUseThisShot = false;
    }

    public void UpdateVisuals()
    {
        if (handHealthy != null) handHealthy.SetActive(false);
        if (handDamaged != null) handDamaged.SetActive(false);
        if (tableHealthy != null) tableHealthy.SetActive(false);
        if (tableDamaged != null) tableDamaged.SetActive(false);

        if (!hasShield || usesLeft <= 0)
            return;

        if (isInHand)
        {
            // Elde göster
            if (isDamaged)
            {
                if (handDamaged != null) handDamaged.SetActive(true);
            }
            else
            {
                if (handHealthy != null) handHealthy.SetActive(true);
            }
        }
        else
        {
            // Masada göster
            if (isDamaged)
            {
                if (tableDamaged != null) tableDamaged.SetActive(true);
            }
            else
            {
                if (tableHealthy != null) tableHealthy.SetActive(true);
            }
        }
    }
}