using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class LobbyStateMachine
{
    private readonly Dictionary<LobbyStateId, ILobbyState> states = new();
    private readonly LobbyContext ctx;

    public ILobbyState Current { get; private set; }

    public LobbyStateMachine(LobbyContext ctx)
    {
        this.ctx = ctx;
    }

    public void Register(ILobbyState state)
    {
        states[state.Id] = state;
    }

    public void TransitionTo(LobbyStateId id)
    {
        if (!states.TryGetValue(id, out var next))
        {
            Debug.LogError($"[LobbyFSM] No state registered for {id}");
            return;
        }

        Debug.Log($"[LobbyFSM] {Current?.Id.ToString() ?? "null"} -> {id}");
        Current?.Exit(ctx);
        Current = next;
        Current.Enter(ctx);
    }

    // Relay Steam/Netcode events to the current state.
    public void OnLobbyEntered(Lobby lobby) => Current?.OnLobbyEntered(ctx, lobby);
    public void OnMemberJoined(Lobby lobby, Friend friend) => Current?.OnMemberJoined(ctx, lobby, friend);
    public void OnMemberLeft(Lobby lobby, Friend friend) => Current?.OnMemberLeft(ctx, lobby, friend);
    public void OnMemberDataChanged(Lobby lobby, Friend friend) => Current?.OnMemberDataChanged(ctx, lobby, friend);
    public void OnLobbyDataChanged(Lobby lobby) => Current?.OnLobbyDataChanged(ctx, lobby);
    public void OnInviteAccepted(Lobby lobby, SteamId friendId) => Current?.OnInviteAccepted(ctx, lobby, friendId);
    public void OnClientConnected(ulong clientId) => Current?.OnClientConnected(ctx, clientId);
    public void OnClientDisconnected(ulong clientId) => Current?.OnClientDisconnected(ctx, clientId);
}
