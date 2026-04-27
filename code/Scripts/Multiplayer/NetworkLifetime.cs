using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Attach to any NetworkObject prefab to auto-despawn after a set duration.
/// Only the server runs the timer; Despawn(true) replicates destruction to all clients.
/// </summary>
public class NetworkLifetime : NetworkBehaviour
{
    [Tooltip("Set at runtime via Initialize() — or bake a default here.")]
    public float lifetime = 3f;

    public void Initialize(float duration)
    {
        lifetime = duration;
    }

    public override void OnNetworkSpawn()
    {
        // Only the server owns the despawn decision
        if (IsServer)
        {
            StartCoroutine(DespawnAfterLifetime());
        }
    }

    private System.Collections.IEnumerator DespawnAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            // true = also destroy the GameObject on all clients
            NetworkObject.Despawn(true);
        }
    }
}