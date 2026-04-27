using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Browse panel: list private lobbies, join one, or host your own.
/// </summary>
public class BrowsingState : LobbyStateBase
{
    public override LobbyStateId Id => LobbyStateId.Browsing;

    private LobbyContext cachedCtx;

    public void Initialize(LobbyContext ctx) => cachedCtx = ctx;

    public override void Enter(LobbyContext ctx)
    {
        ctx.RefreshLobbiesButton.SetEnabled(true);
        ctx.HostGameButton.SetEnabled(true);
        ctx.BackFromBrowseButton.SetEnabled(true);

        ctx.RefreshLobbiesButton.clicked += OnRefresh;
        ctx.HostGameButton.clicked += OnHostGame;
        ctx.BackFromBrowseButton.clicked += OnBack;

        ctx.ShowPanel(ctx.BrowsePanel);
        _ = RefreshLobbyListAsync(ctx);
    }

    public override void Exit(LobbyContext ctx)
    {
        ctx.RefreshLobbiesButton.clicked -= OnRefresh;
        ctx.HostGameButton.clicked -= OnHostGame;
        ctx.BackFromBrowseButton.clicked -= OnBack;
    }

    public override async void OnInviteAccepted(LobbyContext ctx, Lobby lobby, SteamId friendId)
    {
        Debug.Log($"[Browsing] Accepted invite to lobby {lobby.Id}");
        await ctx.NetworkHelper.LeaveAndShutdownAsync();
        var result = await lobby.Join();
        if (result != RoomEnter.Success)
        {
            Debug.LogWarning($"[Browsing] Failed to join invited lobby: {result}");
            ctx.StateMachine.TransitionTo(LobbyStateId.Idle);
        }
    }

    // ── Button handlers ────────────────────────────────────────────────────

    private void OnRefresh() => _ = RefreshLobbyListAsync(cachedCtx);

    private async void OnHostGame()
    {
        var ctx = cachedCtx;
        ctx.HostGameButton.SetEnabled(false);

        var created = await ctx.NetworkHelper.CreateAndHostLobbyAsync(2, isPublic: false);

        if (!created.HasValue)
        {
            ctx.PrivateStatusLabel.text = "Failed to create lobby.";
            ctx.HostGameButton.SetEnabled(true);
            return;
        }

        ctx.IsPublicMatch = false;
        ctx.PrivateLobbyCodeLabel.text = $"Lobby: {created.Value.Id}";
        ctx.PrivateStatusLabel.text = "Waiting for players…";
        ctx.InviteFriendButton.style.display = DisplayStyle.Flex;
        ctx.RefreshMemberList(ctx.PrivateMemberList, showHostTag: true);

        ctx.StateMachine.TransitionTo(LobbyStateId.InLobby);
    }

    private void OnBack()
    {
        cachedCtx.NetworkHelper.LeaveAndShutdownSync();
        cachedCtx.StateMachine.TransitionTo(LobbyStateId.Idle);
    }

    // ── Lobby list ─────────────────────────────────────────────────────────

    private async Task RefreshLobbyListAsync(LobbyContext ctx)
    {
        ctx.BrowseStatusLabel.text = "Searching...";
        ctx.RefreshLobbiesButton.SetEnabled(false);
        ctx.LobbyScrollView.Clear();

        Lobby[] lobbies = await SteamMatchmaking.LobbyList
            .WithMaxResults(20)
            .WithKeyValue("mode", "1v1")
            .WithKeyValue("state", "waiting")
            .WithKeyValue("matchType", "private")
            .WithSlotsAvailable(1)
            .RequestAsync();

        ctx.RefreshLobbiesButton.SetEnabled(true);

        if (lobbies == null || lobbies.Length == 0)
        {
            ctx.BrowseStatusLabel.text = "No open lobbies found.";
            return;
        }

        ctx.BrowseStatusLabel.text = $"{lobbies.Length} lobby/lobbies found.";

        foreach (var lobby in lobbies)
        {
            var capturedLobby = lobby;

            var row = new VisualElement();
            row.AddToClassList("lobby-row");

            SteamFriends.RequestUserInformation(lobby.Owner.Id, true);
            Debug.Log($"Owner name: {lobby.Owner.Name}");

            //string ownerName = string.IsNullOrWhiteSpace(lobby.Owner.Name) ? "Loading..." : lobby.Owner.Name;
            string ownerName = lobby.GetData("ownerName");
            if (string.IsNullOrWhiteSpace(ownerName))
                ownerName = lobby.Owner.Name;
            if (string.IsNullOrWhiteSpace(ownerName))
                ownerName = "Unknown";

            int memberCount = lobby.MemberCount;
            int maxMembers = lobby.MaxMembers;

            var label = new Label($"{ownerName}'s game  [{memberCount}/{maxMembers}]");
            label.AddToClassList("lobby-label");

            var joinBtn = new Button(() => JoinFromBrowse(capturedLobby)) { text = "Join" };
            joinBtn.AddToClassList("lobby-join-btn");

            row.Add(label);
            row.Add(joinBtn);
            ctx.LobbyScrollView.Add(row);
        }
    }

    private async void JoinFromBrowse(Lobby lobby)
    {
        var ctx = cachedCtx;
        ctx.BrowseStatusLabel.text = "Joining...";

        var result = await lobby.Join();
        if (result != RoomEnter.Success)
        {
            ctx.BrowseStatusLabel.text = $"Failed to join: {result}";
        }
        // OnLobbyEntered handles the panel switch via state machine
    }
}
