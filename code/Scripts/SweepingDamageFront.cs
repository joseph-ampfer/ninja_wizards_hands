using UnityEngine;
using System.Collections.Generic;

public class SweepingDamageFront : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 1.5f;

    private SpellCaster owner;
    private Spell spell;
    private float life;
    private HashSet<Collider> hitAlready = new();

    public void Init(SpellCaster caster, Spell spellData)
    {
        owner = caster;
        spell = spellData;
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        life += Time.deltaTime;
        if (life > lifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ignore self and duplicate hits
        if (owner && other.transform == owner.transform) return;
        if (hitAlready.Contains(other)) return;

        hitAlready.Add(other);

        // --- SHIELD CHECK ---
        ShieldComponent shield = other.GetComponent<ShieldComponent>();
        if (shield != null && shield.IsActive)
        {
            //shield.BlockProjectile(this);
            Debug.Log("Shield blocked the spell");
            return;
        }

        var dmg = other.GetComponent<IDamageable>();
        if (dmg == null) return;

        // // handle shield/parry logic first
        // var shield = other.GetComponent<Shield>();
        // if (shield && shield.IsActive)
        // {
        //     shield.Block(spell, owner);
        //     return;
        // }

        // var parry = other.GetComponent<Parry>();
        // if (parry && parry.CanReflect(spell))
        // {
        //     parry.Reflect(spell, owner);
        //     return;
        // }

        // apply damage
        dmg.TakeDamage(spell.damage, owner.transform);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);

        // Draw velocity vector in Scene view
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * speed * 0.1f);
    }
     

}
