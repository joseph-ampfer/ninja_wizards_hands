using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class NetworkSweepingDamageFront : NetworkBehaviour
{
    public float speed = 10f;
    public float lifeTime = 1.5f;

    private NetworkSpellCaster ownerCaster;
    private Spell spell;
    private float life;
    private bool isInitialized = false;
    private HashSet<Collider> hitAlready = new();

    private NetworkObject networkObject;

    void Awake()
    {
        //networkObject = GetComponent<NetworkObject>();
    }

    public void Init(NetworkSpellCaster caster, Spell spellData)
    {
        ownerCaster = caster;
        spell = spellData;
        isInitialized = true;
        networkObject = GetComponentInParent<NetworkObject>();
    }

    void Update()
    {
        if (!isInitialized) return; // Don't move until initialized

        transform.position += transform.forward * speed * Time.deltaTime;
        life += Time.deltaTime;
        if (life > lifeTime)
            networkObject.Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        Debug.Log("SweepingDamageFront hit: " + other.name);

        // ignore self and duplicate hits
        if (ownerCaster && other.transform == ownerCaster.transform) return;
        if (hitAlready.Contains(other)) return;

        hitAlready.Add(other);

        // --- SHIELD CHECK ---
        ShieldComponent shield = other.GetComponentInParent<ShieldComponent>();
        if (shield != null && shield.IsActive)
        {
            Debug.Log("Shield blocked the spell");
            return;
        }


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

        // --- NORMAL DAMAGE ---
        IDamageable target = other.GetComponent<NetworkSpellCaster>();
        if (target != null)
        {
            target.TakeDamage(spell.damage, ownerCaster.transform);
            ImpactAndDestroy(spell);
        }


    }

    public void ImpactAndDestroy(Spell spell)
    {
        // GameObject impactVFX = spell.impactVFX;
        // if (impactVFX)
        // {
        //     GameObject impact = Instantiate(impactVFX, transform.position, Quaternion.identity);
        //     Destroy(impact, 1);
        // }
        ownerCaster.SpawnImpactVFXClientRpc(transform.position, spell.spellNameEnum);
        if (spell.destroyVFXOnHit)
            // Destroy(transform.root.gameObject);  // Kill entire VFX prefab
            networkObject.Despawn();
        else
            Destroy(gameObject);  // Just kill the hitbox, VFX continues

 
            
        Debug.Log("SweepingDamageFront destroyed at time " + Time.time);
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
