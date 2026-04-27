using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using Netcode.Transports.Facepunch;
using UnityEngine;

/// <summary>
/// Encapsulates all NetworkManager + FacepunchTransport lifecycle operations.
/// Every method properly awaits shutdown before proceeding -- no Thread.Sleep.
/// </summary>
public class LobbyNetworkHelper
{
    private readonly LobbyContext ctx;

    public LobbyNetworkHelper(LobbyContext ctx)
    {
        this.ctx = ctx;
    }

    // ── Lobby creation ─────────────────────────────────────────────────────

    /// <summary>
    /// Create a Steam lobby, apply metadata, and start as host.
    /// Returns the created lobby, or null on failure.
    /// </summary>
    public async Task<Lobby?> CreateAndHostLobbyAsync(int maxPlayers, bool isPublic)
    {
        Lobby? created = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
        if (!created.HasValue)
        {
            Debug.LogWarning("[LobbyNet] Failed to create Steam lobby.");
            return null;
        }

        created.Value.SetPublic();
        ctx.ApplyLobbyMetadata(created.Value, isPublic);
        ctx.CurrentLobby = created.Value;
        ctx.IsPublicMatch = isPublic;

        NetworkManager.Singleton.StartHost();
        Debug.Log($"[LobbyNet] Hosted lobby {created.Value.Id} (public={isPublic})");

        return created;
    }

    // ── Join ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Shut down any existing network session, then join the given lobby as client.
    /// Sets FacepunchTransport target to the lobby owner.
    /// </summary>
    public async Task<RoomEnter> JoinLobbyAsync(Lobby lobby)
    {
        await ShutdownIfActiveAsync();

        var result = await lobby.Join();
        if (result != RoomEnter.Success)
        {
            Debug.LogWarning($"[LobbyNet] Join failed: {result}");
            return result;
        }

        // OnLobbyEntered fires from the Steam callback, which sets ctx.CurrentLobby.
        // StartClient is called there via the state machine.
        return result;
    }

    /// <summary>
    /// Called after OnLobbyEntered confirms we are a joiner (not the host).
    /// Wires the transport and starts the Netcode client.
    /// </summary>
    public void StartClientForLobby(Lobby lobby)
    {
        var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
        transport.targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
        Debug.Log($"[LobbyNet] Started client targeting {lobby.Owner.Name} ({lobby.Owner.Id})");
    }

    // ── Leave ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Leave the current Steam lobby and shut down Netcode. Awaits full shutdown.
    /// </summary>
    public async Task LeaveAndShutdownAsync()
    {
        if (ctx.CurrentLobby.HasValue)
        {
            ctx.CurrentLobby.Value.Leave();
            Debug.Log("[LobbyNet] Left Steam lobby.");
            ctx.CurrentLobby = null;
        }

        ctx.LocalReady = false;
        await ShutdownIfActiveAsync();
    }

    /// <summary>
    /// Synchronous leave for use in Exit() or OnDestroy where you can't await.
    /// </summary>
    public void LeaveAndShutdownSync()
    {
        if (ctx.CurrentLobby.HasValue)
        {
            ctx.CurrentLobby.Value.Leave();
            ctx.CurrentLobby = null;
        }

        ctx.LocalReady = false;

        if (NetworkManager.Singleton != null &&
            (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    // ── Scene load ─────────────────────────────────────────────────────────

    /// <summary>
    /// Host-only: mark lobby as in_progress and load the game scene via Netcode.
    /// </summary>
    public void StartGameScene()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        if (ctx.CurrentLobby.HasValue)
            ctx.CurrentLobby.Value.SetData("state", "in_progress");

        ctx.BridgeLobbyToPauseMenu();
        NetworkManager.Singleton.SceneManager.LoadScene(
            ctx.GameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);

        Debug.Log($"[LobbyNet] Loading game scene: {ctx.GameSceneName}");
    }

    // ── Internal ───────────────────────────────────────────────────────────

    private async Task ShutdownIfActiveAsync()
    {
        if (NetworkManager.Singleton == null) return;
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient) return;

        NetworkManager.Singleton.Shutdown();
        Debug.Log("[LobbyNet] Shutting down NetworkManager…");

        // Yield frames until Netcode confirms shutdown (no Thread.Sleep)
        while (NetworkManager.Singleton != null &&
               (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
        {
            await Task.Yield();
        }

        // One extra yield to let any final cleanup run
        await Task.Yield();
        Debug.Log("[LobbyNet] NetworkManager shutdown complete.");
    }
}
