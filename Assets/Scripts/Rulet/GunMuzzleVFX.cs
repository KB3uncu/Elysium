using UnityEngine;
using UnityEngine.VFX;

public class GunMuzzleVFX : MonoBehaviour
{
    public VisualEffect vfx;
    public string playEventName = "OnPlay";

    void Awake()
    {
        if (vfx == null) vfx = GetComponentInChildren<VisualEffect>(true);
    }

    public void PlayOnce()
    {
        Debug.Log($"{name} MUZZLE PLAY");
        if (vfx == null) return;
        vfx.SendEvent("OnPlay");
    }
}
