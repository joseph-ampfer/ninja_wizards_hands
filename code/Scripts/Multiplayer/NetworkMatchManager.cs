using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mediapipe.Unity.Sample.GestureRecognition;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Server-driven multiplayer match flow: load gate, prematch countdown, fighting, end sequence, results and rematch.
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class NetworkMatchManager : NetworkBehaviour
{
    public static NetworkMatchManager Instance { get; private set; }

    public NetworkVariable<NetworkMatchPhase> MatchPhase = new NetworkVariable<NetworkMatchPhase>(
        NetworkMatchPhase.WaitingForPlayers,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("Scene")]
    [Tooltip("If empty, rematch uses the active scene name.")]
    [SerializeField] private string gameSceneName;

    [Header("UI")]
    [SerializeField] private TMP_Text waitingForPlayersText;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text victoryText;
    [SerializeField] private TMP_Text defeatText;
    [SerializeField] private GameObject endMatchButtons;
    [SerializeField] private TMP_Text localRematchStatusText;
    [SerializeField] private TMP_Text opponentRematchStatusText;

    [Header("Camera & VFX")]
    [SerializeField] private CameraEffects cameraEffects;

    [Header("Gesture UI")]
    [SerializeField] private EnemyGestureDisplay enemyGestureDisplay;
    [SerializeField] private GestureUIBuffer gestureUIBuffer;

    [Header("Timing")]
    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private float victoryZoomDuration = 1.5f;
    [SerializeField] private float resultDisplayDelay = 1f;
    [SerializeField] private float showButtonsDelay = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private float gameMusicVolume = 0.3f;

    [Header("Recognition (local client)")]
    [SerializeField] private GestureRecognizerRunner gestureRecognizerRunner;

    private bool _serverAllClientsLoaded;
    private bool _serverMatchEndStarted;
    private bool _matchEndVisualCompleted;
    private bool _rematchLoadStarted;

    public NetworkMatchPhase CurrentPhase => MatchPhase.Value;

    public bool IsFightingPhase => MatchPhase.Value == NetworkMatchPhase.Fighting;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Instance = this;
        MatchPhase.OnValueChanged += OnMatchPhaseChanged;
        if (IsServer)
        {
            MatchPhase.Value = NetworkMatchPhase.WaitingForPlayers;
            _serverMatchEndStarted = false;
            _rematchLoadStarted = false;
        }

        _matchEndVisualCompleted = false;

        ApplyPhaseVisuals(MatchPhase.Value, immediate: true);
        if (!IsServer)
            TryBindGestureRunner();

        ClearRematchMemberDataLocal();
    }

    public override void OnNetworkDespawn()
    {
        MatchPhase.OnValueChanged -= OnMatchPhaseChanged;
        if (Instance == this)
            Instance = null;
        base.OnNetworkDespawn();
    }

    private void OnMatchPhaseChanged(NetworkMatchPhase previous, NetworkMatchPhase next)
    {
        ApplyPhaseVisuals(next, immediate: false);
    }

    private void Update()
    {
        if (MatchPhase.Value != NetworkMatchPhase.Results)
            return;

        if (NetworkManager.Singleton == null)
            return;

        if (!MultiplayerPauseMenu.CurrentLobby.HasValue)
            return;

        var lobby = MultiplayerPauseMenu.CurrentLobby.Value;
        bool iWant = false;
        bool theyWant = false;
        foreach (var member in lobby.Members)
        {
            if (member.Id == SteamClient.SteamId)
                iWant = lobby.GetMemberData(member, "rematch") == "1";
            else if (lobby.GetMemberData(member, "rematch") == "1")
                theyWant = true;
        }

        if (localRematchStatusText != null)
            localRematchStatusText.text = iWant ? "You want a rematch." : "";
        if (opponentRematchStatusText != null)
            opponentRematchStatusText.text = theyWant ? "Opponent wants a rematch." : "";

        if (!NetworkManager.Singleton.IsServer)
            return;

        if (iWant && theyWant && lobby.MemberCount >= 2 && !_rematchLoadStarted)
        {
            _rematchLoadStarted = true;
            string scene = string.IsNullOrEmpty(gameSceneName) ? SceneManager.GetActiveScene().name : gameSceneName;
            DespawnAndReloadScene(scene);
        }
    }

    private void DespawnAndReloadScene(string name)
    {
        if (!IsServer) return;
        // get NetworkManager player objects in a list
        var playerObjects = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList.Where(netOb => netOb.IsPlayerObject).ToList();

        // loop over them and despawn them
        foreach (var netObj in playerObjects)
        {
            if (netObj != null && netObj.IsSpawned)
            {
                Debug.Log("[NetworkMatchManager] Despawning playerObj: " + netObj.name);
                netObj.Despawn();
            }
        }

        // reload scene
        NetworkManager.Singleton.SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    /// <summary>Server: called when Netcode reports all clients finished loading the scene.</summary>
    public void ServerNotifySceneLoadCompleted(string sceneName, LoadSceneMode mode, IReadOnlyList<ulong> clientsTimedOut)
    {
        if (!IsServer)
            return;

        if (clientsTimedOut != null && clientsTimedOut.Count > 0)
        {
            Debug.LogWarning("[NetworkMatchManager] Scene load had timed-out clients.");
            return;
        }

        _serverAllClientsLoaded = true;
        ServerTryLeaveWaitingForPlayers();
    }

    /// <summary>Server: called when NetworkGameManager registers the second player.</summary>
    public void ServerNotifyPlayerCountChanged()
    {
        if (!IsServer)
            return;
        Debug.Log("[NetworkMatchManager] ServerNotifyPlayerCountChanged");
        ServerTryLeaveWaitingForPlayers();
    }

    private void ServerTryLeaveWaitingForPlayers()
    {
        if (!IsServer || MatchPhase.Value != NetworkMatchPhase.WaitingForPlayers)
            return;
        // if (!_serverAllClientsLoaded) // commented out to allow for local testing
        //     return;
        if (NetworkGameManager.Instance == null || NetworkGameManager.Instance.RegisteredPlayerCount < 2)
            return;

        Debug.Log("[NetworkMatchManager] ServerTryLeaveWaitingForPlayers");
        StartCoroutine(ServerPrematchThenFightRoutine());
    }

    private IEnumerator ServerPrematchThenFightRoutine()
    {
        MatchPhase.Value = NetworkMatchPhase.Prematch;
        PlayMusicClientRpc();

        int seconds = Mathf.Max(1, Mathf.RoundToInt(countdownDuration));
        for (int i = seconds; i > 0; i--)
        {
            ShowCountdownClientRpc(i);
            yield return new WaitForSeconds(1f);
        }

        ShowFightClientRpc();
        yield return new WaitForSeconds(0.5f);
        HideCountdownClientRpc();

        MatchPhase.Value = NetworkMatchPhase.Fighting;
    }

    /// <summary>Server: a player's health reached zero during Fighting.</summary>
    public void ServerNotifyPlayerDefeated(NetworkSpellCaster loser)
    {
        if (!IsServer || MatchPhase.Value != NetworkMatchPhase.Fighting)
            return;
        if (_serverMatchEndStarted)
            return;

        var winner = NetworkGameManager.Instance != null ? NetworkGameManager.Instance.GetOpponent(loser) : null;
        if (winner == null)
        {
            Debug.LogWarning("[NetworkMatchManager] Could not resolve winner.");
            return;
        }

        _serverMatchEndStarted = true;
        MatchPhase.Value = NetworkMatchPhase.EndMatch;

        ulong winnerId = winner.NetworkObject.NetworkObjectId;
        ClearGestureBuffersClientRpc();
        MatchEndSequenceClientRpc(winnerId);
        if (IsClient)
            StartCoroutine(EnsureLocalMatchEndVisualsIfNeeded(winnerId));
    }

    /// <summary>Listen-server may not execute ClientRpc on the local peer in some builds; run the sequence once if nothing ran.</summary>
    private IEnumerator EnsureLocalMatchEndVisualsIfNeeded(ulong winnerNetworkObjectId)
    {
        yield return null;
        if (!_matchEndVisualCompleted)
            yield return MatchEndSequenceLocal(winnerNetworkObjectId);
    }

    [ClientRpc]
    private void ClearGestureBuffersClientRpc()
    {
        gestureUIBuffer?.ClearUIBuffer();
        enemyGestureDisplay?.ClearUIBuffer();
    }

    [ClientRpc]
    private void MatchEndSequenceClientRpc(ulong winnerNetworkObjectId)
    {
        StartCoroutine(MatchEndSequenceLocal(winnerNetworkObjectId));
    }

    private IEnumerator MatchEndSequenceLocal(ulong winnerNetworkObjectId)
    {
        if (_matchEndVisualCompleted)
            yield break;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(winnerNetworkObjectId, out NetworkObject winnerNetObj))
        {
            Debug.LogWarning("[NetworkMatchManager] Winner NetworkObject not found for end sequence.");
            yield break;
        }

        _matchEndVisualCompleted = true;

        var winnerCaster = winnerNetObj.GetComponent<NetworkSpellCaster>();
        if (winnerCaster != null && winnerCaster.characterAnimator != null)
            winnerCaster.characterAnimator.SetTrigger("Victory");

        if (cameraEffects != null)
            cameraEffects.ZoomToTarget(winnerNetObj.transform, victoryZoomDuration);

        yield return new WaitForSeconds(resultDisplayDelay);

        bool localWon = winnerNetObj.OwnerClientId == NetworkManager.Singleton.LocalClientId;
        if (victoryText != null)
            victoryText.gameObject.SetActive(localWon);
        if (defeatText != null)
            defeatText.gameObject.SetActive(!localWon);

        if (AudioManager.Instance != null && gameMusic != null)
            AudioManager.Instance.StopMusic();

        yield return new WaitForSeconds(showButtonsDelay);

        if (IsServer)
            MatchPhase.Value = NetworkMatchPhase.Results;

        if (endMatchButtons != null)
            endMatchButtons.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    [ClientRpc]
    private void PlayMusicClientRpc()
    {
        if (AudioManager.Instance != null && gameMusic != null)
            AudioManager.Instance.PlayMusic(gameMusic, true, gameMusicVolume);
    }

    [ClientRpc]
    private void ShowCountdownClientRpc(int value)
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = value.ToString();
        }
    }

    [ClientRpc]
    private void ShowFightClientRpc()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "FIGHT!";
        }
    }

    [ClientRpc]
    private void HideCountdownClientRpc()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void ApplyPhaseVisuals(NetworkMatchPhase phase, bool immediate)
    {
        if (waitingForPlayersText != null)
        {
            bool show = phase == NetworkMatchPhase.WaitingForPlayers;
            waitingForPlayersText.gameObject.SetActive(show);
            if (show && string.IsNullOrEmpty(waitingForPlayersText.text))
                waitingForPlayersText.text = "Waiting for players…";
        }

        if (phase == NetworkMatchPhase.WaitingForPlayers || phase == NetworkMatchPhase.Prematch)
        {
            if (gestureRecognizerRunner == null)
                TryBindGestureRunner();
            gestureRecognizerRunner?.PauseRecognition();
        }
        else if (phase == NetworkMatchPhase.Fighting)
        {
            if (gestureRecognizerRunner == null)
                TryBindGestureRunner();
            gestureRecognizerRunner?.ResumeRecognition();
        }
        else
        {
            gestureRecognizerRunner?.PauseRecognition();
        }

        if (phase != NetworkMatchPhase.Results && endMatchButtons != null)
            endMatchButtons.SetActive(false);

        if (phase == NetworkMatchPhase.Results)
        {
            if (endMatchButtons != null)
                endMatchButtons.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            if (victoryText != null) victoryText.gameObject.SetActive(false);
            if (defeatText != null) defeatText.gameObject.SetActive(false);
        }

        if (immediate && phase != NetworkMatchPhase.Results)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void TryBindGestureRunner()
    {
        if (gestureRecognizerRunner == null)
            gestureRecognizerRunner = FindFirstObjectByType<GestureRecognizerRunner>();
    }

    private static void ClearRematchMemberDataLocal()
    {
        if (!MultiplayerPauseMenu.CurrentLobby.HasValue)
            return;
        try
        {
            MultiplayerPauseMenu.CurrentLobby.Value.SetMemberData("rematch", "0");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[NetworkMatchManager] Could not clear rematch member data: {e.Message}");
        }
    }

    /// <summary>Wire Rematch button (multiplayer).</summary>
    public void OnRematchClicked()
    {
        if (!MultiplayerPauseMenu.CurrentLobby.HasValue)
        {
            Debug.LogWarning("[NetworkMatchManager] No Steam lobby for rematch.");
            return;
        }
        MultiplayerPauseMenu.CurrentLobby.Value.SetMemberData("rematch", "1");
    }

    /// <summary>Wire Leave — leaves lobby and loads menu (local).</summary>
    public void OnLeaveClicked()
    {
        StartCoroutine(LeaveMatchRoutine());
    }

    private IEnumerator LeaveMatchRoutine()
    {
        if (MultiplayerPauseMenu.CurrentLobby.HasValue)
        {
            MultiplayerPauseMenu.CurrentLobby.Value.Leave();
            MultiplayerPauseMenu.CurrentLobby = null;
        }

        if (NetworkManager.Singleton != null &&
            (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
            yield return new WaitWhile(() => NetworkManager.Singleton != null &&
                (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient));
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MultiplayerLobby");
    }
}
