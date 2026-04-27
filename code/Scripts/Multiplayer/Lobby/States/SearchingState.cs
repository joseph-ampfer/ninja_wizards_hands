using System.Threading;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

/// <summary>
/// Public match flow: search for an open lobby, join it, or create one and wait.
/// Stays on the SearchingPanel until a match is found or the user cancels.
/// </summary>
public class SearchingState : LobbyStateBase
{
    public override LobbyStateId Id => LobbyStateId.Searching;

    private LobbyContext cachedCtx;
    private CancellationTokenSource searchCts;

    public void Initialize(LobbyContext ctx) => cachedCtx = ctx;

    public override void Enter(LobbyContext ctx)
    {
        ctx.CancelSearchButton.SetEnabled(true);
        ctx.CancelSearchButton.clicked += OnCancelSearch;

        ctx.SearchingStatusLabel.text = "Searching for a match…";
        ctx.ShowPanel(ctx.SearchingPanel);

        searchCts?.Cancel();
        searchCts?.Dispose();
        searchCts = new CancellationTokenSource();

        _ = RunSearchAsync(ctx, searchCts.Token);
    }

    public override void Exit(LobbyContext ctx)
    {
        ctx.CancelSearchButton.clicked -= OnCancelSearch;
        searchCts?.Cancel();
        searchCts?.Dispose();
        searchCts = null;
    }

    /// <summary>
    /// When another player joins our hosted public lobby, transition to InLobby.
    /// </summary>
    public override void OnMemberJoined(LobbyContext ctx, Lobby lobby, Friend friend)
    {
        if (lobby.MemberCount >= 2)
        {
            Debug.Log($"[Searching] {friend.Name} joined our public lobby. Moving to InLobby.");
            ctx.IsPublicMatch = true;
            ctx.StateMachine.TransitionTo(LobbyStateId.InLobby);
        }
    }

    public override async void OnInviteAccepted(LobbyContext ctx, Lobby lobby, SteamId friendId)
    {
        Debug.Log($"[Searching] Accepted invite, leaving search.");
        await ctx.NetworkHelper.LeaveAndShutdownAsync();
        var result = await lobby.Join();
        if (result != RoomEnter.Success)
        {
            Debug.LogWarning($"[Searching] Failed to join invited lobby: {result}");
            ctx.StateMachine.TransitionTo(LobbyStateId.Idle);
        }
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void OnCancelSearch()
    {
        searchCts?.Cancel();
        cachedCtx.NetworkHelper.LeaveAndShutdownSync();
        cachedCtx.StateMachine.TransitionTo(LobbyStateId.Idle);
    }

    private async Task RunSearchAsync(LobbyContext ctx, CancellationToken ct)
    {
        ctx.SearchingStatusLabel.text = "Searching…";

        Lobby[] lobbies = await SteamMatchmaking.LobbyList
            .WithMaxResults(20)
            .WithKeyValue("mode", "1v1")
            .WithKeyValue("state", "waiting")
            .WithKeyValue("matchType", "public")
            .WithSlotsAvailable(1)
            .RequestAsync();

        // After every await, bail if cancelled. OnCancelSearch already handled
        // cleanup and state transition, so we must NOT double-transition.
        if (ct.IsCancellationRequested) return;

        if (lobbies != null && lobbies.Length > 0)
        {
            ctx.SearchingStatusLabel.text = "Joining…";
            var joinResult = await lobbies[0].Join();

            if (ct.IsCancellationRequested) return;

            if (joinResult != RoomEnter.Success)
            {
                ctx.SearchingStatusLabel.text = $"Failed to join: {joinResult}";
                await Task.Delay(1500);
                if (!ct.IsCancellationRequested)
                    ctx.StateMachine.TransitionTo(LobbyStateId.Idle);
            }
            return;
        }

        if (ct.IsCancellationRequested) return;

        ctx.SearchingStatusLabel.text = "Creating lobby…";
        var created = await ctx.NetworkHelper.CreateAndHostLobbyAsync(2, isPublic: true);

        // If cancelled during create, OnCancelSearch already cleaned up.
        if (ct.IsCancellationRequested) return;

        if (!created.HasValue)
        {
            ctx.SearchingStatusLabel.text = "Failed to create lobby.";
            await Task.Delay(1500);
            if (!ct.IsCancellationRequested)
                ctx.StateMachine.TransitionTo(LobbyStateId.Idle);
            return;
        }

        ctx.SearchingStatusLabel.text = "Waiting for opponent…";
        Debug.Log($"[Searching] Created public lobby {created.Value.Id}, waiting for opponent.");
    }
}
