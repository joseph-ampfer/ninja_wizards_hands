using UnityEngine;

public class ShieldComponent : MonoBehaviour
{
    public bool IsActive { get; private set; }
    public float activeTime = 4f;
    public GameObject shieldVFX;

    public void ActivateShield()
    {
        IsActive = true;
        Debug.Log("in ActivateShield");

        if (shieldVFX) shieldVFX.SetActive(true);
        else Debug.Log("no public GameObject shieldVFX");
        
        Invoke(nameof(DeactivateShield), activeTime);
    }

    private void DeactivateShield()
    {
        IsActive = false;
        if (shieldVFX) shieldVFX.SetActive(false);
    }

    public void BlockProjectile(ProjectileBase projectile)
    {
        // Optional: play block effect, reduce shield HP, etc.
        projectile.ImpactAndDestroy();
    }


}
