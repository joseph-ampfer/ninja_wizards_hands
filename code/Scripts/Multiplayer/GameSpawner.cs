using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class GameSpawner : NetworkBehaviour
{
    public Transform[] spawnPoints;
    public GameObject sapphiPrefab;
    public GameObject picoPrefab;

    private List<ulong> clientIds = new();


    void Start()
    {
        Debug.Log("GameSpawner started");
        NetworkObject networkObject = GetComponentInParent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.Log("networkObject is null");
            return;
        }
        if (!NetworkManager.Singleton.IsServer) {
            Debug.Log("GameSpawner not server, not spawning");
            return;
        };
        if (!networkObject.IsSpawned)
        {
            Debug.Log("GameSpawner networkObject not spawned, spawning");
            networkObject.Spawn();
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("GameSpawner spawned");
        base.OnNetworkSpawn();

        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log("GameSpawner spawning player for client: " + id);
            TrySpawnPlayerForClient(id);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        base.OnNetworkDespawn();
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log("Gamespawner client connected: " + clientId);
        TrySpawnPlayerForClient(clientId);
    }

    private void TrySpawnPlayerForClient(ulong clientId)
    {
        if (!IsServer) {
            Debug.Log("not server, not spawning");
            return;
        }

        if (clientIds.Count >= 2)
        {
            Debug.Log("already 2 clientIds in match, not spawning");
            return;
        }

        if (clientIds.Contains(clientId))
        {
            Debug.Log("client with same id connected, not spawning");
            return;
        }

        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId))
        {
            Debug.Log("client not connected, not spawning");
            return;
        }

        var sorted = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        sorted.Sort();
        int index = sorted.IndexOf(clientId);
        if (index < 0) return;

        Transform spawn = spawnPoints[index % spawnPoints.Length];
        GameObject prefab = index == 0 ? sapphiPrefab : picoPrefab;

        GameObject player = Instantiate(prefab, spawn.position, spawn.rotation);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        clientIds.Add(clientId);
    }
}
