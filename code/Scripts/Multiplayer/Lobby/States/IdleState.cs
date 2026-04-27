using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Default panel: Public Match, Private Match, Back to Main Menu.
/// </summary>
public class IdleState : LobbyStateBase
{
    public override LobbyStateId Id => LobbyStateId.Idle;

    public override void Enter(LobbyContext ctx)
    {
        ctx.PublicMatchButton.SetEnabled(true);
        ctx.PrivateMatchButton.SetEnabled(true);
        ctx.BackToMainMenuButton.SetEnabled(true);

        // display all errors
        if (ErrorManager.Instance?.Errors.Count == 0)
        {
            ctx.ErrorMsg.text = "";
        }
        else
        {
            ctx.ErrorMsg.text = string.Join("\n", ErrorManager.Instance.Errors);
        }
        // clear all errors
        ErrorManager.Instance.ClearErrors();

        ctx.PublicMatchButton.clicked += OnPublicMatch;
        ctx.PrivateMatchButton.clicked += OnPrivateMatch;
        ctx.BackToMainMenuButton.clicked += OnBackToMainMenu;

        ctx.ShowPanel(ctx.DefaultPanel);
    }

    public override void Exit(LobbyContext ctx)
    {
        ctx.PublicMatchButton.clicked -= OnPublicMatch;
        ctx.PrivateMatchButton.clicked -= OnPrivateMatch;
        ctx.BackToMainMenuButton.clicked -= OnBackToMainMenu;
    }

    // Invites can arrive even while sitting at the default screen
    public override async void OnInviteAccepted(LobbyContext ctx, Steamworks.Data.Lobby lobby, Steamworks.SteamId friendId)
    {
        Debug.Log($"[Idle] Accepted invite to lobby {lobby.Id}");
        await ctx.NetworkHelper.LeaveAndShutdownAsync();
        var result = await lobby.Join();

        if (result != Steamworks.RoomEnter.Success)
        {
            Debug.LogWarning($"[Idle] Failed to join invited lobby: {result}");
            ctx.StateMachine.TransitionTo(LobbyStateId.Idle);
        }
        // OnLobbyEntered callback handles the rest
    }

    // ── Stored references for unsubscribe ──────────────────────────────────
    // We need a stable reference to the LobbyContext for the click handlers.
    // The orchestrator guarantees a single IdleState instance per context.

    private LobbyContext cachedCtx;

    private void OnPublicMatch()
    {
        cachedCtx.StateMachine.TransitionTo(LobbyStateId.Searching);
    }

    private void OnPrivateMatch()
    {
        cachedCtx.StateMachine.TransitionTo(LobbyStateId.Browsing);
    }

    private void OnBackToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // Override Enter to cache context for button callbacks
    // (Button.clicked is Action, can't pass arguments)
    public void Initialize(LobbyContext ctx)
    {
        cachedCtx = ctx;
    }
}
