// PlayerNetworkSetup.cs
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkSetup : NetworkBehaviour
{
    public Camera playerCamera;
    public AudioListener audioListener;
    public static event Action OnPlayerSpawned;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"PLAYER SPAWNED | Owner: {OwnerClientId} | IsOwner: {IsOwner}");

        // Explicitly set both directions — never assume the prefab default is correct
        playerCamera.enabled    = IsOwner;
        audioListener.enabled   = IsOwner;

        OnPlayerSpawned?.Invoke();
    }
}