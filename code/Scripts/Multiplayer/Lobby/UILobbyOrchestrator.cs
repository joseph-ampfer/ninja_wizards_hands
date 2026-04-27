using UnityEngine;
using UnityEngine.UIElements;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;

/// <summary>
/// Thin MonoBehaviour that wires UI elements, subscribes to Steam/Netcode callbacks,
/// and delegates all logic to the LobbyStateMachine.
/// 
/// Drop-in replacement for the old UILobbyEvents — same serialized fields,
/// same GameObject slot.
/// </summary>
public class UILobbyOrchestrator : MonoBehaviour
{
    public UIDocument uIDocument;

    [Header("Scene Settings")]
    public string gameSceneName = "JoeyNetworking";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;

    private LobbyContext ctx;
    private LobbyStateMachine fsm;

    // ─────────────────────────────────────────────────────────────────────
    #region Unity Lifecycle

    void Start()
    {
        var root = uIDocument.rootVisualElement;

        // Build context with all UI references
        ctx = new LobbyContext
        {
            GameSceneName = gameSceneName,

            // Panels
            DefaultPanel      = root.Q<VisualElement>("DefaultPanel"),
            SearchingPanel    = root.Q<VisualElement>("SearchingPanel"),
            BrowsePanel       = root.Q<VisualElement>("BrowsePanel"),
            PrivateLobbyPanel = root.Q<VisualElement>("PrivateLobbyPanel"),
            PublicLobbyPanel  = root.Q<VisualElement>("PublicLobbyPanel"),

            // Default Panel
            PublicMatchButton    = root.Q<Button>("PublicMatch"),
            PrivateMatchButton   = root.Q<Button>("PrivateMatch"),
            BackToMainMenuButton = root.Q<Button>("BackToMainMenu"),
            ErrorMsg             = root.Q<Label>("ErrorMsg"),

            // Searching Panel
            SearchingStatusLabel = root.Q<Label>("SearchingStatusLabel"),
            CancelSearchButton   = root.Q<Button>("CancelSearch"),

            // Browse Panel
            RefreshLobbiesButton = root.Q<Button>("RefreshLobbies"),
            LobbyScrollView      = root.Q<ScrollView>("LobbyScrollView"),
            BrowseStatusLabel    = root.Q<Label>("BrowseStatusLabel"),
            BackFromBrowseButton = root.Q<Button>("BackFromBrowse"),
            HostGameButton       = root.Q<Button>("HostGame"),

            // Private Lobby Panel
            PrivateLobbyCodeLabel = root.Q<Label>("PrivateLobbyCode"),
            PrivateMemberList     = root.Q<VisualElement>("PrivateMemberList"),
            InviteFriendButton    = root.Q<Button>("InviteFriend"),
            PrivateReadyButton    = root.Q<Button>("PrivateReadyButton"),
            PrivateStatusLabel    = root.Q<Label>("PrivateStatusLabel"),
            BackFromPrivateButton = root.Q<Button>("BackFromPrivate"),

            // Public Lobby Panel
            PublicMemberList     = root.Q<VisualElement>("PublicMemberList"),
            PublicReadyButton    = root.Q<Button>("PublicReadyButton"),
            PublicStatusLabel    = root.Q<Label>("PublicStatusLabel"),
            BackFromPublicButton = root.Q<Button>("BackFromPublic"),

            // Audio
            AudioSource = audioSource,
            HoverSound  = hoverSound,
        };

        // Network helper
        ctx.NetworkHelper = new LobbyNetworkHelper(ctx);

        // State machine
        fsm = new LobbyStateMachine(ctx);
        ctx.StateMachine = fsm;

        // Create and register states
        var idle = new IdleState();
        idle.Initialize(ctx);
        fsm.Register(idle);

        var searching = new SearchingState();
        searching.Initialize(ctx);
        fsm.Register(searching);

        var browsing = new BrowsingState();
        browsing.Initialize(ctx);
        fsm.Register(browsing);

        var inLobby = new InLobbyState();
        inLobby.Initialize(ctx);
        fsm.Register(inLobby);

        fsm.Register(new StartingMatchState());

        // Hover sound on all buttons
        root.Query<Button>().ForEach(btn =>
            btn.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (audioSource != null && hoverSound != null)
                    audioSource.PlayOneShot(hoverSound);
            }));

        // Subscribe to Steam callbacks
        SteamMatchmaking.OnLobbyEntered           += OnSteamLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined      += OnSteamMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave       += OnSteamMemberLeft;
        SteamMatchmaking.OnLobbyMemberDataChanged += OnSteamMemberDataChanged;
        SteamMatchmaking.OnLobbyDataChanged       += OnSteamLobbyDataChanged;
        SteamMatchmaking.OnLobbyInvite            += OnSteamLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested     += OnSteamJoinRequested;

        // Subscribe to Netcode callbacks
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback  += OnNetClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnNetClientDisconnected;
        }

        if (!SteamClient.IsValid)
        {
            Debug.LogWarning("[LobbyOrchestrator] Steam is not initialized!");
            ErrorManager.Instance.AddError("Steam must be running for multiplayer");
        }
        
        // Start in Idle
        fsm.TransitionTo(LobbyStateId.Idle);
    }

    void OnDestroy()
    {
        SteamMatchmaking.OnLobbyEntered           -= OnSteamLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined      -= OnSteamMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave       -= OnSteamMemberLeft;
        SteamMatchmaking.OnLobbyMemberDataChanged -= OnSteamMemberDataChanged;
        SteamMatchmaking.OnLobbyDataChanged       -= OnSteamLobbyDataChanged;
        SteamMatchmaking.OnLobbyInvite            -= OnSteamLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested     -= OnSteamJoinRequested;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback  -= OnNetClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnNetClientDisconnected;
        }

        // Only tear down if we're NOT transitioning into the game scene
        if (fsm?.Current?.Id != LobbyStateId.StartingMatch)
        {
            Debug.Log("Tearing down network manager");
            ctx?.NetworkHelper?.LeaveAndShutdownSync();
        }
        else
        {
            Debug.Log("Not tearing down network manager, we're transitioning into the game scene");
        }
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────
    #region Steam Callbacks -> State Machine

    private void OnSteamLobbyEntered(Lobby lobby)
    {
        ctx.CurrentLobby = lobby;
        ctx.LocalReady = false;

        Debug.Log($"[LobbyOrchestrator] Entered lobby {lobby.Id}");

        // If we are the host who just created this lobby, we're already in the
        // right state (Searching or InLobby). The host started networking in
        // CreateAndHostLobbyAsync, so just relay the event.
        if (NetworkManager.Singleton.IsHost && lobby.Owner.Id == SteamClient.SteamId)
        {
            fsm.OnLobbyEntered(lobby);
            return;
        }

        // We are joining someone else's lobby -- start as client
        bool isPublic = lobby.GetData("matchType") == "public";
        ctx.IsPublicMatch = isPublic;

        ctx.NetworkHelper.StartClientForLobby(lobby);

        // Transition to InLobby (the state's Enter will pick the right panel)
        if (fsm.Current?.Id != LobbyStateId.InLobby)
            fsm.TransitionTo(LobbyStateId.InLobby);
        else
            fsm.OnLobbyEntered(lobby);
    }

    private void OnSteamMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"[LobbyOrchestrator] {friend.Name} joined lobby.");
        fsm.OnMemberJoined(lobby, friend);
    }

    private void OnSteamMemberLeft(Lobby lobby, Friend friend)
    {
        Debug.Log($"[LobbyOrchestrator] {friend.Name} left lobby.");
        fsm.OnMemberLeft(lobby, friend);
    }

    private void OnSteamMemberDataChanged(Lobby lobby, Friend friend)
    {
        fsm.OnMemberDataChanged(lobby, friend);
    }

    private void OnSteamLobbyDataChanged(Lobby lobby)
    {
        fsm.OnLobbyDataChanged(lobby);
    }

    private void OnSteamLobbyInvite(Friend friend, Lobby lobby)
    {
        Debug.Log($"[LobbyOrchestrator] Invite from {friend.Name} to lobby {lobby.Id}");
    }

    private void OnSteamJoinRequested(Lobby lobby, SteamId friendId)
    {
        Debug.Log($"[LobbyOrchestrator] Join requested for lobby {lobby.Id}");
        fsm.OnInviteAccepted(lobby, friendId);
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────
    #region Netcode Callbacks -> State Machine

    private void OnNetClientConnected(ulong clientId)
    {
        fsm.OnClientConnected(clientId);
    }

    private void OnNetClientDisconnected(ulong clientId)
    {
        Debug.Log($"[LobbyOrchestrator] Client disconnected: {clientId}");
        fsm.OnClientDisconnected(clientId);
    }

    #endregion
}
