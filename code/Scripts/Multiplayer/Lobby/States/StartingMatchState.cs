using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Transitional state: all players are ready, host loads the game scene.
/// </summary>
public class StartingMatchState : LobbyStateBase
{
    public override LobbyStateId Id => LobbyStateId.StartingMatch;

    private Label statusLabel;

    public override void Enter(LobbyContext ctx)
    {
        statusLabel = ctx.IsPublicMatch ? ctx.PublicStatusLabel : ctx.PrivateStatusLabel;
        statusLabel.text = "All ready! Starting…";

        // Disable UI so players can't mess with buttons while loading
        if (ctx.IsPublicMatch)
        {
            ctx.PublicReadyButton.SetEnabled(false);
            ctx.BackFromPublicButton.SetEnabled(false);
        }
        else
        {
            ctx.PrivateReadyButton.SetEnabled(false);
            ctx.BackFromPrivateButton.SetEnabled(false);
            ctx.InviteFriendButton.SetEnabled(false);
        }

        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("[StartingMatch] Host loading game scene.");
            ctx.NetworkHelper.StartGameScene();
        }
        else
        {
            // Client just waits -- Netcode scene management will load the scene
            Debug.Log("[StartingMatch] Waiting for host to load scene.");
            ctx.BridgeLobbyToPauseMenu();
        }
    }

    public override void OnClientDisconnected(LobbyContext ctx, ulong clientId)
    {
        bool isLocalDisconnect = !NetworkManager.Singleton.IsHost &&
                                  clientId == NetworkManager.Singleton.LocalClientId;

        if (isLocalDisconnect)
        {
            Debug.Log("[StartingMatch] Disconnected while starting.");
            statusLabel.text = "Disconnected.";
            ctx.NetworkHelper.LeaveAndShutdownSync();
            ctx.StateMachine.TransitionTo(LobbyStateId.Idle);
        }
    }
}
