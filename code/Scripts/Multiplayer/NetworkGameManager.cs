using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance; // singleton 
    private List<NetworkSpellCaster> spellCasters = new();

    public int RegisteredPlayerCount => spellCasters.Count;

    [SerializeField] private HealthBar healthBarPrefab;
    [SerializeField] private HealthBar enemyHealthBarPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Instance = this;
    }
    public void RegisterPlayer(NetworkSpellCaster spellCaster)
    {
        spellCasters.Add(spellCaster);

        if (spellCasters.Count == 2)
        {
            AssignOpponents();
        }

        if (IsServer && NetworkMatchManager.Instance != null)
            NetworkMatchManager.Instance.ServerNotifyPlayerCountChanged();
    }

    public NetworkSpellCaster GetOpponent(NetworkSpellCaster caster)
    {
        if (spellCasters.Count < 2)
            return null;
        if (spellCasters[0] == caster)
            return spellCasters[1];
        if (spellCasters[1] == caster)
            return spellCasters[0];
        return null;
    }

    private void AssignOpponents()
    {
        Debug.Log("Assigning opponents");
        spellCasters[0].AssignOpponent(spellCasters[1]);
        spellCasters[1].AssignOpponent(spellCasters[0]);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        base.OnDestroy();
    }

    // Note: this is useful to know when all clients have loaded the same scene (single or additive mode)
    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log($"OnLoadEventCompleted: {sceneName}, {loadSceneMode}, {clientsCompleted.Count}, {clientsTimedOut.Count}");
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;
        if (NetworkMatchManager.Instance != null)
            NetworkMatchManager.Instance.ServerNotifySceneLoadCompleted(sceneName, loadSceneMode, clientsTimedOut);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
