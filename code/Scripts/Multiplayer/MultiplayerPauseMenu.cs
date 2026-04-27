using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Steamworks.Data;
using Unity.Netcode;
using GlobalCursor = UnityEngine.Cursor;
using Image = UnityEngine.UIElements.Image;


/// <summary>
/// Multiplayer pause menu.
///
/// Attach to a GameObject in the game scene alongside a UIDocument
/// pointing at PauseMenu.uxml.
///
/// Note: we intentionally do NOT touch Time.timeScale — pausing time
/// corrupts server-side physics and Netcode ticks in a multiplayer match.
/// </summary>
public class MultiplayerPauseMenu : MonoBehaviour
{
     [Header("UI")]
    public UIDocument uiDocument;
 
    [Header("Scenes")]
    public string lobbySceneName = "MainMenu";
 
    [Header("Spellbook")]
    [SerializeField] private GestureIconLibrary iconLibrary;

    // ── UI references ─────────────────────────────────────────────────────
    private VisualElement root;
    private VisualElement pausePanel;
    private VisualElement backdrop;
    private VisualElement spellListContainer;
    private Label spellbookEmptyLabel;
    private Button resumeButton;
    private Button settingsButton;
    private Button leaveButton;
    private VisualElement settingsPanelRoot;
    private SettingsPanelController settingsPanelController;
 
    // ── State ──────────────────────────────────────────────────────────────
    private bool isPaused       = false;
    private bool isLeaving      = false;
    private bool spellbookBuilt = false;
 
    public static Lobby? CurrentLobby;
 
    // ─────────────────────────────────────────────────────────────────────
    #region Unity Lifecycle
 
    void Start()
    {
        var root = uiDocument.rootVisualElement;
 
        this.root = root;
        pausePanel          = root.Q<VisualElement>("PausePanel");
        backdrop            = root.Q<VisualElement>("Backdrop");
        spellListContainer  = root.Q<VisualElement>("SpellList");
        spellbookEmptyLabel = root.Q<Label>("SpellbookEmpty");
        resumeButton        = root.Q<Button>("ResumeButton");
        settingsButton      = root.Q<Button>("SettingsButton");
        leaveButton         = root.Q<Button>("LeaveButton");
        settingsPanelRoot   = root.Q<VisualElement>("settings-root");
        settingsPanelController = new SettingsPanelController(settingsPanelRoot);

        resumeButton.clicked   += Resume;
        settingsButton.clicked += OnSettingsClicked;
        leaveButton.clicked    += OnLeaveClicked;
 
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnClientStopped += OnClientStopped;
 
        SetPanelVisible(false);
    }
 
    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
        }
    }
 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }
 
    #endregion
 
    // ─────────────────────────────────────────────────────────────────────
    #region Pause / Resume
 
    public void TogglePause() { if (isPaused) Resume(); else Pause(); }
 
    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;
 
        if (!spellbookBuilt) BuildSpellbookUI();
 
        SetPanelVisible(true);
        GlobalCursor.visible   = true;
        GlobalCursor.lockState = CursorLockMode.None;
    }
 
    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;
 
        SetPanelVisible(false);
        GlobalCursor.visible   = false;
        GlobalCursor.lockState = CursorLockMode.Locked;
    }
 
    #endregion
 
    // ─────────────────────────────────────────────────────────────────────
    #region Spellbook UI
 
    private void BuildSpellbookUI()
    {
        spellbookBuilt = true;
        spellListContainer.Clear();
 
        var caster = GetLocalCaster();
 
        if (caster == null || caster.spellbook == null || iconLibrary == null)
        {
            spellbookEmptyLabel.style.display = DisplayStyle.Flex;
            Debug.LogWarning("[PauseMenu] Missing caster, spellBook, or iconLibrary.");
            return;
        }
 
        var entries = caster.spellbook.GetEntries();
        if (entries == null || entries.Count == 0)
        {
            spellbookEmptyLabel.style.display = DisplayStyle.Flex;
            return;
        }
 
        spellbookEmptyLabel.style.display = DisplayStyle.None;
 
        foreach (var entry in entries)
        {
            if (entry.spell == null) continue;
 
            // ── Row ──────────────────────────────────────────────────────
            var row = new VisualElement();
            row.AddToClassList("spell-row");
 
            // Spell name
            var nameLabel = new Label(entry.spell.spellName);
            nameLabel.AddToClassList("spell-name");
            row.Add(nameLabel);
 
            // Gesture icon strip
            var iconStrip = new VisualElement();
            iconStrip.AddToClassList("icon-strip");
 
            if (entry.sequence != null)
            {
                for (int i = 0; i < entry.sequence.Count; i++)
                {
                    // Comma separator between pairs
                    if (i > 0)
                    {
                        var comma = new Label(",");
                        comma.AddToClassList("pair-separator");
                        iconStrip.Add(comma);
                    }
 
                    var pair = entry.sequence[i];
                    iconStrip.Add(MakeGestureIcon(pair.Left, "left"));
                    iconStrip.Add(MakeGestureIcon(pair.Right, "right"));
                }
            }
 
            row.Add(iconStrip);
            spellListContainer.Add(row);
        }
    }
 
    private Image MakeGestureIcon(GestureLabel gesture, string hand)
    {
        var icon = new Image
        {
            sprite = iconLibrary.GetIcon(gesture, hand),
            scaleMode = ScaleMode.ScaleToFit
        };
        icon.AddToClassList("gesture-icon");
        return icon;
    }
 
    private NetworkSpellCaster GetLocalCaster()
    {
        if (NetworkManager.Singleton?.LocalClient?.PlayerObject == null) return null;
        return NetworkManager.Singleton.LocalClient.PlayerObject
                             .GetComponent<NetworkSpellCaster>();
    }
 
    #endregion
 
    // ─────────────────────────────────────────────────────────────────────
    #region Leave Match
 
    private void OnLeaveClicked()
    {
        Debug.Log("OnLeaveClicked");
        if (isLeaving) return;
        isLeaving = true;
        leaveButton.SetEnabled(false);
        leaveButton.text = "Leaving…";
        StartCoroutine(LeaveMatchRoutine());
    }

    public IEnumerator LeaveMatchRoutine()
    {
        Debug.Log("LeaveMatchRoutine");
        if (CurrentLobby.HasValue)
        {
            CurrentLobby.Value.Leave();
            CurrentLobby = null;
        }


        if (NetworkManager.Singleton != null &&
            (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();

            // Wait until Netcode confirms it is fully shut down
            // IsListening goes false once the shutdown completes
            yield return new WaitWhile(() => NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient);
        }

        yield return null; // Wait for the next frame



        GlobalCursor.visible = true;
        GlobalCursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;
        SceneManager.LoadScene(lobbySceneName);
        Debug.Log($"Leaving match, returning to lobby: {lobbySceneName}");

    }
 
    private void OnClientDisconnected(ulong clientId)
    {
        if (isLeaving) return;
 
        bool weAreClient = NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost;
        bool hostLeft = clientId == NetworkManager.ServerClientId;

        ErrorManager.Instance?.AddError("Opponent disconnected");
 
        if (weAreClient && hostLeft)
        {
            Debug.Log("[PauseMenu] Host disconnected — returning to lobby.");
            isLeaving = true;
            StartCoroutine(LeaveMatchRoutine());
            return;
        }
 
        if (NetworkManager.Singleton.IsHost && clientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("[PauseMenu] Opponent disconnected — returning to lobby.");
            isLeaving = true;
            StartCoroutine(LeaveMatchRoutine());
        }
    }

    private void OnClientStopped(bool wasHost)
    {
        if (isLeaving) return;
        ErrorManager.Instance?.AddError("Opponent disconnected");
        Debug.Log("[PauseMenu] Client stopped (host gone) -- returning to lobby.");
        isLeaving = true;
        
        StartCoroutine(LeaveMatchRoutine());
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────
    #region Helpers

    private void OnSettingsClicked()
    {
        Debug.Log("[PauseMenu] Settings — wire up your settings panel.");
        settingsPanelController.LoadSavedValues();
        settingsPanelRoot.style.display = DisplayStyle.Flex;
    }

    private void SetPanelVisible(bool visible)
    {
        pausePanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        backdrop.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
 
    #endregion
}