using Steamworks;
using Steamworks.Data;

public enum LobbyStateId
{
    Idle,
    Searching,
    Browsing,
    InLobby,
    StartingMatch
}

public interface ILobbyState
{
    LobbyStateId Id { get; }

    void Enter(LobbyContext ctx);
    void Exit(LobbyContext ctx);

    void OnLobbyEntered(LobbyContext ctx, Lobby lobby);
    void OnMemberJoined(LobbyContext ctx, Lobby lobby, Friend friend);
    void OnMemberLeft(LobbyContext ctx, Lobby lobby, Friend friend);
    void OnMemberDataChanged(LobbyContext ctx, Lobby lobby, Friend friend);
    void OnLobbyDataChanged(LobbyContext ctx, Lobby lobby);
    void OnInviteAccepted(LobbyContext ctx, Lobby lobby, SteamId friendId);
    void OnClientConnected(LobbyContext ctx, ulong clientId);
    void OnClientDisconnected(LobbyContext ctx, ulong clientId);
}

/// <summary>
/// Default no-op base so individual states only override what they need.
/// </summary>
public abstract class LobbyStateBase : ILobbyState
{
    public abstract LobbyStateId Id { get; }

    public virtual void Enter(LobbyContext ctx) { }
    public virtual void Exit(LobbyContext ctx) { }

    public virtual void OnLobbyEntered(LobbyContext ctx, Lobby lobby) { }
    public virtual void OnMemberJoined(LobbyContext ctx, Lobby lobby, Friend friend) { }
    public virtual void OnMemberLeft(LobbyContext ctx, Lobby lobby, Friend friend) { }
    public virtual void OnMemberDataChanged(LobbyContext ctx, Lobby lobby, Friend friend) { }
    public virtual void OnLobbyDataChanged(LobbyContext ctx, Lobby lobby) { }
    public virtual void OnInviteAccepted(LobbyContext ctx, Lobby lobby, SteamId friendId) { }
    public virtual void OnClientConnected(LobbyContext ctx, ulong clientId) { }
    public virtual void OnClientDisconnected(LobbyContext ctx, ulong clientId) { }
}
