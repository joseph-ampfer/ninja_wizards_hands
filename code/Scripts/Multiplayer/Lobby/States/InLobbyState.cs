using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Unified lobby panel for both public and private matches.
/// Uses ctx.IsPublicMatch to bind to the correct UI elements.
/// </summary>
public class InLobbyState : LobbyStateBase
{
    public override LobbyStateId Id => LobbyStateId.InLobby;

    private LobbyContext cachedCtx;

    // Resolved per-enter based on IsPublicMatch
    private VisualElement memberList;
    private Button readyButton;
    private Label statusLabel;
    private Button backButton;

    public void Initialize(LobbyContext ctx) => cachedCtx = ctx;

    public override void Enter(LobbyContext ctx)
    {
        if (ctx.IsPublicMatch)
        {
            memberList = ctx.PublicMemberList;
            readyButton = ctx.PublicReadyButton;
            statusLabel = ctx.PublicStatusLabel;
            backButton = ctx.BackFromPublicButton;
            ctx.ShowPanel(ctx.PublicLobbyPanel);
        }
        else
        {
            memberList = ctx.PrivateMemberList;
            readyButton = ctx.PrivateReadyButton;
            statusLabel = ctx.PrivateStatusLabel;
            backButton = ctx.BackFromPrivateButton;

            bool isHost = ctx.CurrentLobby.HasValue &&
                          ctx.CurrentLobby.Value.Owner.Id == SteamClient.SteamId;
            ctx.InviteFriendButton.style.display = isHost ? DisplayStyle.Flex : DisplayStyle.None;
            ctx.InviteFriendButton.clicked += OnInviteFriend;

            ctx.ShowPanel(ctx.PrivateLobbyPanel);
        }

        ctx.LocalReady = false;
        readyButton.text = "READY UP";
        readyButton.EnableInClassList("ready-active", false);
        readyButton.SetEnabled(true);
        backButton.SetEnabled(true);

        readyButton.clicked += OnReadyClicked;
        backButton.clicked += OnBackClicked;

        ctx.RefreshMemberList(memberList, showHostTag: !ctx.IsPublicMatch);

        if (!IsLocalHost(ctx))
            statusLabel.text = "Connected to lobby. Ready up when set!";
    }

    public override void Exit(LobbyContext ctx)
    {
        readyButton.clicked -= OnReadyClicked;
        backButton.clicked -= OnBackClicked;

        if (!ctx.IsPublicMatch)
            ctx.InviteFriendButton.clicked -= OnInviteFriend;
    }

    public override void OnMemberJoined(LobbyContext ctx, Lobby lobby, Friend friend)
    {
        Debug.Log($"[InLobby] {friend.Name} joined.");
        ctx.RefreshMemberList(memberList, showHostTag: !ctx.IsPublicMatch);

        if (IsLocalHost(ctx))
            statusLabel.text = $"{friend.Name} joined!";
    }

    public override void OnMemberLeft(LobbyContext ctx, Lobby lobby, Friend friend)
    {
        Debug.Log($"[InLobby] {friend.Name} left.");
        ctx.RefreshMemberList(memberList, showHostTag: !ctx.IsPublicMatch);

        if (IsLocalHost(ctx))
        {
            statusLabel.text = $"{friend.Name} left.";
            TryCheckAllReady(ctx);
        }
    }

    public override void OnMemberDataChanged(LobbyContext ctx, Lobby lobby, Friend friend)
    {
        Debug.Log($"[InLobby] {friend.Name} updated data.");
        ctx.RefreshMemberList(memberList, showHostTag: !ctx.IsPublicMatch);
        TryCheckAllReady(ctx);
    }

    /// <summary>
    /// Client receives "in_progress" -- the host is loading the game scene via Netcode,
    /// so the client doesn't need to do anything explicit here.
    /// </summary>
    public override void OnLobbyDataChanged(LobbyContext ctx, Lobby lobby)
    {
        if (lobby.GetData("state") == "in_progress")
        {
            Debug.Log("[InLobby] Lobby state is in_progress.");
            statusLabel.text = "Starting match…";
            ctx.BridgeLobbyToPauseMenu();
            ctx.StateMachine.TransitionTo(LobbyStateId.StartingMatch);  // <-- add this to prevent teardown
        }
    }

    public override async void OnInviteAccepted(LobbyContext ctx, Lobby lobby, SteamId friendId)
    {
        Debug.Log($"[InLobby] Accepted invite while in lobby. Leaving current lobby first.");
        await ctx.NetworkHelper.LeaveAndShutdownAsync();
        var result = await lobby.Join();
        if (result != RoomEnter.Success)
        {
            Debug.LogWarning($"[InLobby] Failed to join invited lobby: {result}");
            ctx.StateMachine.TransitionTo(LobbyStateId.Idle);
        }
        // OnLobbyEntered handles the rest
    }

    public override void OnClientDisconnected(LobbyContext ctx, ulong clientId)
    {
        bool isLocalDisconnect = !NetworkManager.Singleton.IsHost &&
                                  clientId == NetworkManager.Singleton.LocalClientId;

        if (isLocalDisconnect)
        {
            ErrorManager.Instance.AddError("Disconnected from host.");
            
            Debug.Log("[InLobby] Disconnected from host.");
            statusLabel.text = "Disconnected from host.";
            ctx.NetworkHelper.LeaveAndShutdownSync();
            ctx.StateMachine.TransitionTo(LobbyStateId.Idle);
        }
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void OnReadyClicked()
    {
        var ctx = cachedCtx;
        if (!ctx.CurrentLobby.HasValue) return;

        ctx.LocalReady = !ctx.LocalReady;
        ctx.CurrentLobby.Value.SetMemberData("ready", ctx.LocalReady ? "1" : "0");

        readyButton.text = ctx.LocalReady ? "✓ READY" : "READY UP";
        readyButton.EnableInClassList("ready-active", ctx.LocalReady);
    }

    private void OnBackClicked()
    {
        var ctx = cachedCtx;
        ctx.NetworkHelper.LeaveAndShutdownSync();

        if (ctx.IsPublicMatch)
            ctx.StateMachine.TransitionTo(LobbyStateId.Idle);
        else
            ctx.StateMachine.TransitionTo(LobbyStateId.Browsing);
    }

    private void OnInviteFriend()
    {
        var ctx = cachedCtx;
        if (ctx.CurrentLobby.HasValue)
            SteamFriends.OpenGameInviteOverlay(ctx.CurrentLobby.Value.Id);
        else
            Debug.Log("[InLobby] No lobby to invite to.");
    }

    private void TryCheckAllReady(LobbyContext ctx)
    {
        if (!ctx.CurrentLobby.HasValue) return;
        if (!NetworkManager.Singleton.IsHost) return;
        if (ctx.CurrentLobby.Value.MemberCount < 2) return;

        foreach (var member in ctx.CurrentLobby.Value.Members)
        {
            if (ctx.CurrentLobby.Value.GetMemberData(member, "ready") != "1")
                return;
        }

        Debug.Log("[InLobby] All players ready!");
        ctx.StateMachine.TransitionTo(LobbyStateId.StartingMatch);
    }

    private static bool IsLocalHost(LobbyContext ctx)
    {
        return ctx.CurrentLobby.HasValue &&
               ctx.CurrentLobby.Value.Owner.Id == SteamClient.SteamId;
    }
}
