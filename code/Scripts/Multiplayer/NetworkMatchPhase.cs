/// <summary>
/// Server-authoritative multiplayer match phases (synced via NetworkMatchManager).
/// </summary>
public enum NetworkMatchPhase : byte
{
    WaitingForPlayers = 0,
    Prematch = 1,
    Fighting = 2,
    EndMatch = 3,
    Results = 4
}
