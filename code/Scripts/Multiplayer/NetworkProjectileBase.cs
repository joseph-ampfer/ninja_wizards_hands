using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NetworkProjectileBase : NetworkBehaviour
{
    [Header("Stats")]
    public float speed = 20f;
    public float damage = 10f;
    public float lifetime = 5f;
    public bool destroyOnHit = true;

    [Header("References")]
    public GameObject impactVFX;

    private Rigidbody rb;
    private Transform owner;
    private float lifeTimer;

    private NetworkObject networkObject;

    private bool initialized = false;
    private Renderer[] renderers;

    private SpellNameEnum spellName;
    private NetworkSpellCaster ownerCaster;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();
        renderers = GetComponentsInChildren<Renderer>();
        // hide until InitializeClientRpc is called
        //SetVisible(false);
    }



    public void Fire(Vector3 direction, Transform firedBy, SpellNameEnum spellName)
    {
        //transform.rotation = Quaternion.LookRotation(direction);
        owner = firedBy;
        this.spellName = spellName;
        ownerCaster = firedBy.GetComponent<NetworkSpellCaster>();
        lifeTimer = 0f;
        rb.linearVelocity = direction * speed;
        //SetVisible(true);
    }

    [ClientRpc]
    public void InitializeClientRpc(Vector3 spawnPosition, Vector3 direction, double spawnTime)
    {
        if (IsServer) return; // server already initialized via Fire()

        Debug.Log($"InitializeClientRpc: spawnPosition={spawnPosition}, direction={direction}, spawnTime={spawnTime}");

        // float elapsedTime = (float)(NetworkManager.LocalTime.Time - spawnTime);
        // Debug.Log($"elapsedTime={elapsedTime}");
        // transform.position = spawnPosition + direction * speed * elapsedTime;
        transform.position = spawnPosition;
        rb.linearVelocity = direction * speed;
        //SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderers) r.enabled = visible;
    }

    // Helper: am I the server (or is this not even a networked object?)
    private bool CanDestroy()
    {
        // If no network object, this is single player - always OK to destroy
        if (networkObject == null) return true;
        // If networked, only the server/host may destroy
        return NetworkManager.Singleton.IsServer;
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime && CanDestroy())
        {
            if (networkObject != null)
                networkObject.Despawn();
            else
                Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform == owner) return; // don't hit self\

        // Only the server should process hits for networked projectiles
        if (!CanDestroy()) return;

        Debug.Log($"Projectile hit: {other.name} (tag: {other.tag}, layer: {LayerMask.LayerToName(other.gameObject.layer)})");

        // --- SHIELD CHECK ---
        // Use GetComponentInParent to find ShieldComponent on parent objects (e.g., shield VFX is child of player)
        ShieldComponent shield = other.GetComponentInParent<ShieldComponent>();
        if (shield != null)
        {
            Debug.Log($"Found ShieldComponent on: {shield.gameObject.name} (InstanceID: {shield.GetInstanceID()}), IsActive={shield.IsActive}");
            if (shield.IsActive)
            {
                shield.BlockProjectile(this);
                return;
            }
            else
            {
                Debug.Log("Shield found but NOT active - projectile passes through! Make sure the shield gesture activates THIS ShieldComponent.");
            }
        }

        // // --- PARRY CHECK ---
        // ParryComponent parry = other.GetComponent<ParryComponent>();
        // if (parry != null && parry.IsParrying)
        // {
        //     parry.OnSuccessfulParry(this);
        //     ReflectProjectile(parry.transform);
        //     return;
        // }

        // --- NORMAL DAMAGE ---
        // Check the hit object and its parents for IDamageable (NetworkSpellCaster implements it)
        IDamageable target = other.GetComponentInParent<NetworkSpellCaster>();
        if (target != null)
        {
            target.TakeDamage(damage, owner);
            Debug.Log($"[Server] Projectile dealt {damage} damage to {other.name}");
        }

        ImpactAndDestroy();
    }

    private void ReflectProjectile(Transform newTarget)
    {
        // Simple reflection — just reverse direction toward target
        Vector3 dir = (newTarget.position - transform.position).normalized;
        rb.linearVelocity = dir * speed;
        owner = newTarget;
    }

    public void ImpactAndDestroy(bool playImpactVfx = true)
    {
        if (!destroyOnHit) return;

        if (networkObject != null && networkObject.IsSpawned)
        {
            if (playImpactVfx && ownerCaster != null)
                ownerCaster.SpawnImpactVFXClientRpc(transform.position, spellName);
            StartCoroutine(DespawnNextFrame());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator DespawnNextFrame()
    {
        yield return null; // wait one frame — not 100ms
        if (networkObject != null && networkObject.IsSpawned)
            networkObject.Despawn();
    }


    public override void OnNetworkDespawn()
    {
        // intentionally empty — VFX handled by SpawnImpactVFXClientRpc
    }
}
