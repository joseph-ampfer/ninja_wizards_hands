using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class HudBinder : MonoBehaviour
{
    [SerializeField] private HealthBar leftBar;
    [SerializeField] private HealthBar rightBar;
    [SerializeField] private ManaBar leftManaBar;
    [SerializeField] private ManaBar rightManaBar;

    private NetworkSpellCaster localPlayer;
    private NetworkSpellCaster enemyPlayer;


    private void OnEnable()
    {
        PlayerNetworkSetup.OnPlayerSpawned += TryBind;
    }

    private void OnDisable()
    {
        PlayerNetworkSetup.OnPlayerSpawned -= TryBind;
    }

    private void Start()
    {
        TryBind(); // in case both already spawned
    }

    public void TryBind()
    {
        Debug.Log("Trying to bind HUD");
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient) return;

        // Find all spawned network players
        var players = FindObjectsByType<NetworkSpellCaster>(FindObjectsSortMode.None);

        localPlayer = players.FirstOrDefault(p => p.IsOwner);
        enemyPlayer = players.FirstOrDefault(p => !p.IsOwner);

        if (localPlayer == null)
        {
            Debug.LogError("No local player found");
            return; // not ready yet (spawn order / scene load)
        }

        if (enemyPlayer == null)
        {
            Debug.LogError("No enemy player found");
            return; // not ready yet (spawn order / scene load)
        }

        // bind bars
        leftBar.BindTo(localPlayer);
        leftManaBar.BindTo(localPlayer);
        rightBar.BindTo(enemyPlayer);
        rightManaBar.BindTo(enemyPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
