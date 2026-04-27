using Steamworks.Data;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Shared mutable state passed to every lobby state.
/// Holds the current lobby, UI panel references, and helper objects.
/// </summary>
public class LobbyContext
{
    // ── Core references ────────────────────────────────────────────────────
    public LobbyStateMachine StateMachine;
    public LobbyNetworkHelper NetworkHelper;
    public string GameSceneName;

    // ── Lobby state ────────────────────────────────────────────────────────
    public Lobby? CurrentLobby;
    public bool LocalReady;
    public bool IsPublicMatch;

    // ── Panels ─────────────────────────────────────────────────────────────
    public VisualElement DefaultPanel;
    public VisualElement SearchingPanel;
    public VisualElement BrowsePanel;
    public VisualElement PrivateLobbyPanel;
    public VisualElement PublicLobbyPanel;

    // ── Default Panel ──────────────────────────────────────────────────────
    public Button PublicMatchButton;
    public Button PrivateMatchButton;
    public Button BackToMainMenuButton;
    public Label ErrorMsg;

    // ── Searching Panel ────────────────────────────────────────────────────
    public Label SearchingStatusLabel;
    public Button CancelSearchButton;

    // ── Browse Panel ───────────────────────────────────────────────────────
    public Button RefreshLobbiesButton;
    public ScrollView LobbyScrollView;
    public Label BrowseStatusLabel;
    public Button BackFromBrowseButton;
    public Button HostGameButton;

    // ── Private Lobby Panel ────────────────────────────────────────────────
    public Label PrivateLobbyCodeLabel;
    public VisualElement PrivateMemberList;
    public Button InviteFriendButton;
    public Button PrivateReadyButton;
    public Label PrivateStatusLabel;
    public Button BackFromPrivateButton;

    // ── Public Lobby Panel ─────────────────────────────────────────────────
    public VisualElement PublicMemberList;
    public Button PublicReadyButton;
    public Label PublicStatusLabel;
    public Button BackFromPublicButton;

    // ── Audio ──────────────────────────────────────────────────────────────
    public AudioSource AudioSource;
    public AudioClip HoverSound;

    /// <summary>Matchmaking version tag to filter incompatible lobbies.</summary>
    public string MatchmakingVersion => Application.version;

    // ── Panel visibility ───────────────────────────────────────────────────

    public void ShowPanel(VisualElement panelToShow)
    {
        DefaultPanel.style.display      = panelToShow == DefaultPanel      ? DisplayStyle.Flex : DisplayStyle.None;
        SearchingPanel.style.display    = panelToShow == SearchingPanel    ? DisplayStyle.Flex : DisplayStyle.None;
        BrowsePanel.style.display       = panelToShow == BrowsePanel       ? DisplayStyle.Flex : DisplayStyle.None;
        PrivateLobbyPanel.style.display = panelToShow == PrivateLobbyPanel ? DisplayStyle.Flex : DisplayStyle.None;
        PublicLobbyPanel.style.display  = panelToShow == PublicLobbyPanel  ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>
    /// Apply standard metadata to a newly created lobby.
    /// </summary>
    public void ApplyLobbyMetadata(Lobby lobby, bool isPublic)
    {
        lobby.SetData("ownerName", Steamworks.SteamClient.Name);
        lobby.SetData("mode", "1v1");
        lobby.SetData("state", "waiting");
        lobby.SetData("version", MatchmakingVersion);
        lobby.SetData("matchType", isPublic ? "public" : "private");
    }

    /// <summary>
    /// Bridge the lobby reference to the in-game pause menu before scene transition.
    /// </summary>
    public void BridgeLobbyToPauseMenu()
    {
        MultiplayerPauseMenu.CurrentLobby = CurrentLobby;
    }

    /// <summary>
    /// Refresh the member list UI in the appropriate panel.
    /// </summary>
    public void RefreshMemberList(VisualElement container, bool showHostTag)
    {
        if (container == null || !CurrentLobby.HasValue) return;
        container.Clear();

        foreach (var member in CurrentLobby.Value.Members)
        {
            bool isReady = CurrentLobby.Value.GetMemberData(member, "ready") == "1";
            bool isOwner = member.Id == CurrentLobby.Value.Owner.Id;
            string readyTag = isReady ? "✓" : "…";
            string hostTag = (showHostTag && isOwner) ? " [Host]" : "";

            var row = new Label($"{readyTag}  {member.Name}{hostTag}");
            row.AddToClassList("member-row");
            row.EnableInClassList("member-ready", isReady);
            container.Add(row);
        }
    }
}
