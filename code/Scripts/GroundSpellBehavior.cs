using UnityEngine;

[CreateAssetMenu(fileName = "GroundSpellBehavior", menuName = "Spells/GroundSpellBehavior")]
public class GroundSpellBehavior : SpellBehavior
{
    public float radius = 5f;
    public float heightOffset = 0.1f;
  

    public override void Cast(SpellCaster caster, Spell spell, Transform target)
    {

        // Spawn visual effect and auto-cleanup
        if (spell.spawnPrefab == null)
        {
            Debug.Log("forgot the spell prefab");
            return;
        }

        var vfx = Instantiate(spell.spawnPrefab, caster.underLeftHand.position, caster.transform.rotation);
        var front = vfx.GetComponentInChildren<SweepingDamageFront>();

        if (front) front.Init(caster, spell);
        else Debug.Log("didn't get SweepingDamageFront");


        Destroy(vfx, spell.lifetime);

        // // Apply instant damage
        // Collider[] hits = Physics.OverlapSphere(center, radius);
        // foreach (var h in hits)
        // {
        //     var dmg = h.GetComponent<IDamageable>();
        //     if (dmg != null)
        //         dmg.TakeDamage(spell.damage, caster.transform);
        // }
    }
}
