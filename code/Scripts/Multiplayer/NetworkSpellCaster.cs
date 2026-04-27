using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkSpellCaster : NetworkBehaviour, IDamageable
{
    // Knows about spawn points (hand, staff tip, enemy, etc.).
    // Instantiates spell prefabs at the correct position.
    // Handles cleanup, particle auto-destroy, pooling, etc.
    // Later: handles mana, cooldowns, targeting.

    [Header("Player stats")]
    public float maxHealth = 100f;
    // Server-authoritative health — synced automatically to all clients
    public NetworkVariable<float> NetworkHealth = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public float maxMana = 100f;
    public NetworkVariable<float> NetworkMana = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>Server-authoritative shield state — drives shield VFX and particle timing on all clients.</summary>
    public NetworkVariable<ShieldNetworkState> NetworkShieldState = new NetworkVariable<ShieldNetworkState>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Mana Restoration")]
    [SerializeField] private float manaRestoreDelay = 1f;
    [SerializeField] private float manaRestoreAmount = 2f;
    private Coroutine restoreCoroutine;

    [Header("Casting")]
    public Transform rightHandPoint;
    public Transform leftHandPoint;
    public Transform staffTip;
    public Transform centerPoint;
    public Transform underLeftHand;
    public Transform feetPoint; // for AOEs or self-buffs
    [SerializeField] private Transform enemyPoint; // where projectiles aim & aoe spells spawn
    //public static event Action OnSpellCast;

    [Header("Visuals")]
    [SerializeField] private GameObject fizzleVFX; // disabled GameObject under player
    [SerializeField] private GameObject outOfManaSmokeVFX; // disabled GameObject under player
    public GameObject leftHandAura;
    public GameObject rightHandAura;
    public CameraEffects cameraEffects;

    // Spell Casting
    public static event Action<NetworkSpellCaster, Spell> OnSpellCast; // <- global broadcast
    public static event Action<NetworkSpellCaster, Spell> OnSpellCastDuringCooldown; // <- global broadcast

    // Death event for MatchManager to subscribe
    public event Action<SpellCaster> OnDeath;
    public Animator characterAnimator;

    // Spell queue to handle multiple casts before animations complete
    private class QueuedSpell
    {
        public Spell spell;
        public int releasesRemaining;
    }
    private Queue<QueuedSpell> spellQueue = new Queue<QueuedSpell>();
    // private Transform currentTarget;

    [Header("HealthBar")]
    [SerializeField] HealthBar healthBar;

    [Header("ManaBar")]
    [SerializeField] ManaBar ManaBar;

    [Header("Hit Feedback")]
    [SerializeField] private float flashDuration = 0.5f; private Renderer[] allRenderers;
    private Color[] originalColors; // Store colors, not materials!

    [Header("Low Health Vignette")]
    [SerializeField] private LowHealthVignetteUI lowHealthVignette;
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30%
    [SerializeField] private float criticalHealthThreshold = 0.15f; // 15%

    [Header("Shield")]
    [SerializeField] private ShieldComponent shield;

    // NOT GOOD TO HAVE SPELLBOOK HERE, AND IN SPELLCONTROLLER, BUT FOR NOW IT'S FINE
    [SerializeField] public SpellBook spellbook;
    private Spell currentSpell;


    public enum SpellCasterState
    {
        Idle,
        Casting,
        Recovering,
    }
    public SpellCasterState spellCasterState = SpellCasterState.Idle;
    private double CastStartTime = 0f;
    private double RecoveryEndTime = 0f;
    private Dictionary<SpellNameEnum, double> spellCooldowns = new Dictionary<SpellNameEnum, double>();
    // client prediction only — new
    private Dictionary<SpellNameEnum, double> clientSpellCooldowns = new Dictionary<SpellNameEnum, double>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Initialize health on server
        if (IsServer)
        {
            NetworkHealth.Value = maxHealth;
            NetworkMana.Value = maxMana;
        }

        // Subscribe to health changes on all clients (for UI, logging, etc.)
        NetworkHealth.OnValueChanged += OnHealthChanged;
        NetworkMana.OnValueChanged += OnManaChanged;
        NetworkShieldState.OnValueChanged += OnShieldNetworkChanged;
        if (shield == null)
            shield = GetComponent<ShieldComponent>();
        shield?.ApplyShieldVisualSync(NetworkShieldState.Value);

        // Initialize health bar UI if assigned
        if (healthBar != null)
        {
            healthBar.InitHealth(maxHealth);
        }

        characterAnimator = GetComponent<Animator>();

        NetworkGameManager.Instance.RegisterPlayer(this);
    }

    public override void OnNetworkDespawn()
    {
        NetworkHealth.OnValueChanged -= OnHealthChanged;
        NetworkMana.OnValueChanged -= OnManaChanged;
        NetworkShieldState.OnValueChanged -= OnShieldNetworkChanged;
        base.OnNetworkDespawn();
    }

    /// <summary>
    /// Called on ALL clients whenever NetworkHealth changes.
    /// Updates health bar UI and logs the change.
    /// </summary>
    private void OnHealthChanged(float previousValue, float newValue)
    {
        Debug.Log($"[{gameObject.name}] Health changed: {previousValue} -> {newValue}");

        // if (healthBar != null)
        // {
        //     healthBar.SetHealth(newValue);
        // }

        if (newValue <= 0f)
        {
            Debug.Log($"[{gameObject.name}] has been defeated!");
        }
    }

    /// <summary>
    /// Called on ALL clients whenever NetworkMana changes.
    /// Updates mana bar UI and logs the change.
    /// </summary>
    private void OnManaChanged(float previousValue, float newValue)
    {
        // Debug.Log($"[{gameObject.name}] Mana changed: {previousValue} -> {newValue}");

        // if (ManaBar != null)
        // {
        //     ManaBar.SetMana(newValue);
        // }

        if (newValue <= 0f)
        {
            Debug.Log($"[{gameObject.name}] has run out of mana!");
        }
    }

    private void OnShieldNetworkChanged(ShieldNetworkState previousValue, ShieldNetworkState newValue)
    {
        if (shield == null)
            shield = GetComponent<ShieldComponent>();
        shield?.ApplyShieldVisualSync(newValue);
    }

    /// <summary>Called on the server when the shield activates or deactivates (e.g. from ShieldComponent).</summary>
    public void SetShieldStateServer(bool active, float duration)
    {
        if (!IsServer) return;
        NetworkShieldState.Value = new ShieldNetworkState { Active = active, Duration = duration };
    }

    /// <summary>
    /// IDamageable implementation — called on the SERVER by NetworkProjectileBase collision.
    /// Applies damage authoritatively and syncs via NetworkVariable.
    /// </summary>
    public void TakeDamage(float amount, Transform source)
    {
        if (!IsServer) return; // Only server modifies health

        if (NetworkMatchManager.Instance != null && !NetworkMatchManager.Instance.IsFightingPhase)
            return;

        NetworkHealth.Value = Mathf.Max(NetworkHealth.Value - amount, 0f);
        Debug.Log($"[Server] {gameObject.name} took {amount} damage from {source?.name}. Health: {NetworkHealth.Value}/{maxHealth}");

        // Notify all clients about the hit for feedback (VFX, sound, etc.)
        OnDamagedClientRpc(amount, NetworkHealth.Value);

        if (NetworkHealth.Value <= 0f && NetworkMatchManager.Instance != null)
        {
            NetworkMatchManager.Instance.ServerNotifyPlayerDefeated(this);
            //characterAnimator.SetTrigger("Die");
            OnDeathClientRpc();
        }
    }

    [ClientRpc]
    private void OnDeathClientRpc()
    {
        // if (IsOwner)
        // {
        //     Debug.Log("[NetworkSpellCaster] OnDeathClientRpc: Local caster, skipping");
        //     return;
        // }
        Debug.Log("[NetworkSpellCaster] OnDeathClientRpc: setting die trigger");
        characterAnimator.SetTrigger("Die");
    }

    /// <summary>
    /// ClientRpc sent to all clients when this player takes damage.
    /// Use this for hit feedback effects (flash, camera shake, etc.)
    /// </summary>
    [ClientRpc]
    private void OnDamagedClientRpc(float damageAmount, float currentHealth)
    {
        int AdditiveLayer = 1;
        Debug.Log($"[Client] {gameObject.name} was hit for {damageAmount}! Health: {currentHealth}/{maxHealth}");
        characterAnimator.CrossFadeInFixedTime(
            "Damaged",
            0.05f,  // short blend
            AdditiveLayer,
            0f  // seek into the animation by network delay amount
        );
    }

    // Called by the client to create a synced effect at its own position.
    public void Cast(SpellNameEnum spellNameEnum)
    {
        Spell spell = spellbook.GetSpellByName(spellNameEnum);

        if (spell == null)
        {
            Debug.LogError($"Spell is null. {spellNameEnum}. Make sure it's added to the spellbook {spellbook.name}.");
            return;
        }

        if (spell.spawnPrefab == null)
        {
            Debug.LogError("Spell spawn prefab is null");
            return;
        }

        if (spellCasterState != SpellCasterState.Idle)
        {
            Debug.LogError("Spell caster is not idle");
            return;
        }

        if (NetworkMatchManager.Instance != null && !NetworkMatchManager.Instance.IsFightingPhase)
            return;

        // Mirror server cooldown check locally
        if (clientSpellCooldowns.TryGetValue(spellNameEnum, out double endTime))
        {
            if (NetworkManager.LocalTime.Time < endTime)
            {
                // play a "on cooldown" feedback instead — shake, sound, UI flash
                OnSpellCastDuringCooldown?.Invoke(this, spell);
                return;
            }
        }

        // let UI subscribers know, (ex. to show cooldown)
        OnSpellCast?.Invoke(this, spell);

        // Record cooldown locally (mirrors what server will do)
        clientSpellCooldowns[spellNameEnum] = NetworkManager.LocalTime.Time + spell.cooldown;

        // SKIP LOCAL ANIMATION FOR DODGE SPELLS
        bool skipLocalAnimation = spell.spellNameEnum == SpellNameEnum.DodgeRight1 || spell.spellNameEnum == SpellNameEnum.DodgeLeft1;
        if (!skipLocalAnimation)
        {
            if (spell.manaCost <= NetworkMana.Value) // check mana locally
                PlayLocalAnimation(spell); // start locally immediately
        }

        TrySpellServerRPC(spellNameEnum, NetworkManager.LocalTime.Time, default);
    }


    private void PlayLocalAnimation(Spell spell)
    {
        string animationName = spell.castTriggerName;

        Debug.Log($"Playing animation {animationName} on client");
        if (characterAnimator == null)
        {
            Debug.LogError("Character animator is null");
            return;
        }
        if (string.IsNullOrEmpty(animationName))
        {
            Debug.LogError("Animation name is null or empty");
            return;
        }

        characterAnimator.CrossFadeInFixedTime(
            animationName,
            0.05f,  // short blend
            -1,
            0f  // seek into the animation by network delay amount
        );
    }


    [ServerRpc]
    private void TrySpellServerRPC(SpellNameEnum spellNameEnum, double requestedStartTime, ServerRpcParams serverRpcParams = default)
    {
        Spell spell = spellbook.GetSpellByName(spellNameEnum);

        if (spell == null)
        {
            Debug.LogError($"Spell is null.");
            return;
        }

        if (NetworkMatchManager.Instance != null && !NetworkMatchManager.Instance.IsFightingPhase)
        {
            var rejectParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId }
                }
            };
            CastRejectedClientRpc(spellNameEnum, rejectParams);
            return;
        }

        if (spell.manaCost > NetworkMana.Value)
        {
            Debug.Log($"Not enough mana for {spell.name}!!!");
            PlayNoManaClientRPC();
            return;
        }

        if (spell.spawnPrefab == null)
        {
            Debug.LogError("Spell spawn prefab is null");
            return;
        }

        if (spellCasterState != SpellCasterState.Idle)
        {
            Debug.LogError("Spell caster is not idle");
            return;
        }

        // GET COOLDOWN END TIME FOR SPELL
        double cooldownEndTime = spellCooldowns.TryGetValue(spellNameEnum, out double endTime) ? endTime : 0; 

        if (cooldownEndTime > NetworkManager.ServerTime.Time)
        {
            Debug.LogError($"{spell.name} is on cooldown. Cooldown ends at {cooldownEndTime}");
            return;
        }

        // SET CURRENT SPELL
        currentSpell = spell;
        // used to time spell spawn to animation & built in recovery
        // also have cooldown per individual spell (NOT IMPLEMENTED YET)
        //CastStartTime = NetworkManager.ServerTime.Time;
        CastStartTime = requestedStartTime;
        RecoveryEndTime = CastStartTime + currentSpell.recoveryDelay;
        spellCooldowns[spellNameEnum] = CastStartTime + currentSpell.cooldown;
        // SET SPELL CASTER STATE TO CASTING
        // update loop will see state and spawn spell
        spellCasterState = SpellCasterState.Casting;

        // PLAY ANIMATION ON CLIENT
        // plays animation immediately, no delay
        PlayAnimationClientRPC(spellNameEnum, requestedStartTime);
        
        // USE MANA for spell immediately (can change to use mana over time later)
        UseMana(spell.manaCost);

        // SPAWN SPELL ON SERVER, SYNCED WITH NETWORK TRANSFORM
        //spell.behavior.Cast(this, spell, enemyPoint);
        // update loop will see state and spawn spell
    }

    private void UseMana(float mana)
    {
        if (!NetworkManager.IsServer)
        {
            Debug.LogError("UseMana can only be called on the server");
            return;
        }
        // characterAnimator.SetTrigger("ManaUsedTrigger");
        Debug.Log($"Used {mana} Mana");
        NetworkMana.Value = Mathf.Max(NetworkMana.Value - mana, 0f);
        Debug.Log($"CURRENT Mana {NetworkMana.Value}");
        Debug.Log($"MAX Mana {maxMana}");
        StartManaRestore();
    }

    [ClientRpc]
    private void CastRejectedClientRpc(SpellNameEnum spellNameEnum, ClientRpcParams clientRpcParams = default)
    {
        clientSpellCooldowns.Remove(spellNameEnum);
        PlayFizzle();
    }

    [ClientRpc]
    private void PlayNoManaClientRPC()
    {
        cameraEffects.Shake();
        PlaySmoke();
    }

    [ClientRpc]
    private void PlayAnimationClientRPC(SpellNameEnum spellNameEnum, double requestedStartTime)
    {
        bool playAnimation = spellNameEnum == SpellNameEnum.DodgeRight1 || spellNameEnum == SpellNameEnum.DodgeLeft1;
        if (IsOwner && !playAnimation) return; // already started locally in Cast(), but didn't start dodge animation

        Spell spell = spellbook.GetSpellByName(spellNameEnum);
        string animationName = spell.castTriggerName;

        Debug.Log($"Playing animation {animationName} on client");

        if (characterAnimator == null)
        {
            Debug.LogError("Character animator is null");
            return;
        }
        if (string.IsNullOrEmpty(animationName))
        {
            Debug.LogError("Animation name is null or empty");
            return;
        }

        float offset = (float)(NetworkManager.ServerTime.Time - requestedStartTime);
        offset = Mathf.Clamp(offset, 0f, spell.GetFirstReleaseTime() - 0.05f);
        Debug.Log($"PlayAnimationClientRPC: offset: {offset}");

        characterAnimator.CrossFadeInFixedTime(
            animationName,
            0.05f,  // short blend
            -1,
            offset  // seek into the animation by network delay amount
        );
    }

    [ClientRpc]
    public void SpawnImpactVFXClientRpc(Vector3 hitPosition, SpellNameEnum spellName)
    {
        Debug.Log($"SpawnImpactVFXClientRpc: spellName={spellName}, hitPosition={hitPosition}");
        Spell spell = spellbook.GetSpellByName(spellName);
        if (spell?.impactVFX != null)
        {
            Debug.Log($"SpawnImpactVFXClientRpc: spellName={spellName}, hitPosition={hitPosition}");
            GameObject i = Instantiate(spell.impactVFX, hitPosition, Quaternion.identity);
            Destroy(i, 1f);
        }
        else
        {
            Debug.LogError($"SpawnImpactVFXClientRpc: spell {spellName} has no impact VFX");
        }
    }

    public void AssignOpponent(NetworkSpellCaster opponent)
    {
        //this.opponent = opponent;
        this.enemyPoint = opponent.transform;
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.opponent = opponent.transform;
        }
    }


    // 🔥 Mini-pool style fizzle feedback
    private void PlaySmoke()
    {
        if (fizzleVFX == null)
        {
            Debug.Log("fizzle effect is null");
            return;
        }
        StartCoroutine(SmokeRoutine());
    }

    private IEnumerator SmokeRoutine()
    {
        outOfManaSmokeVFX.SetActive(true);
        var ps = outOfManaSmokeVFX.GetComponent<ParticleSystem>();
        ps?.Play();

        // Wait for duration or default 1s
        yield return new WaitForSeconds(ps?.main.duration ?? 1f);

        outOfManaSmokeVFX.SetActive(false);
    }

    // 🔥 Mini-pool style fizzle feedback
    public void PlayFizzle()
    {
        if (fizzleVFX == null)
        {
            Debug.Log("fizzle effect is null");
            return;
        }

        StartCoroutine(FizzleRoutine());
    }

    private IEnumerator FizzleRoutine()
    {
        fizzleVFX.SetActive(true);
        var ps = fizzleVFX.GetComponent<ParticleSystem>();
        ps?.Play();

        // Wait for duration or default 1s
        yield return new WaitForSeconds(ps?.main.duration ?? 1f);

        fizzleVFX.SetActive(false);
    }


    // Start or restart the mana restoration process
    private void StartManaRestore()
    {
        if (!NetworkManager.IsServer)
        {
            Debug.LogError("StartManaRestore can only be called on the server");
            return;
        }
        // stop any existing restore first
        StopManaRestore();

        restoreCoroutine = StartCoroutine(RestoreMana());
    }

    // Stop the mana restoration
    private void StopManaRestore()
    {
        if (!NetworkManager.IsServer)
        {
            Debug.LogError("StopManaRestore can only be called on the server");
            return;
        }
        if (restoreCoroutine != null)
        {
            StopCoroutine(restoreCoroutine);
            restoreCoroutine = null;
        }
    }

    // Mana restoration logic - restores mana over time
    private IEnumerator RestoreMana()
    {
        if (!NetworkManager.IsServer)
        {
            Debug.LogError("RestoreMana can only be called on the server");
            yield break;
        }
        // initial delay before starting the restore
        yield return new WaitForSeconds(manaRestoreDelay);

        if (NetworkMana.Value >= maxMana)
        {
            yield break; // already full
        }

        float remaining = maxMana - NetworkMana.Value;

        // keep restoring manaRestoreAmount per second (last step may be less)
        while (remaining > 0f && NetworkMana.Value < maxMana)
        {
            float amountToRestore = Mathf.Min(manaRestoreAmount, remaining, maxMana - NetworkMana.Value);
            NetworkMana.Value = Mathf.Min(NetworkMana.Value + amountToRestore, maxMana);
            //ManaBar.SetMana(NetworkMana.Value); // Taken care of by NetworkMana.OnValueChanged
            remaining -= amountToRestore;
            yield return new WaitForSeconds(1f);
        }
    }




    void Update()
    {
        // FACE ENEMY
        if (enemyPoint != null && enemyPoint.position != null)
        {
            Vector3 dir = enemyPoint.position - transform.position;
            dir.y = 0f;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        // SPELL CASTER STATE MACHINE
        if (!NetworkManager.IsServer) return;
        switch (spellCasterState)
        {
            case SpellCasterState.Idle:
                break;
            case SpellCasterState.Casting:
                if (NetworkManager.Singleton.ServerTime.Time >= CastStartTime + currentSpell.GetFirstReleaseTime())
                {
                    if (currentSpell.behavior != null)
                    {
                        // SPAWN SPELL ON SERVER, SYNCED WITH NETWORK TRANSFORM
                        currentSpell.behavior.Cast(this, currentSpell, enemyPoint);
                    }
                    spellCasterState = SpellCasterState.Recovering;
                }
                break;
            case SpellCasterState.Recovering:
                if (NetworkManager.Singleton.ServerTime.Time >= RecoveryEndTime)
                {
                    spellCasterState = SpellCasterState.Idle;
                    RecoveryEndTime = 0f;
                    CastStartTime = 0f;
                    // currentSpell = null; 
                    // don't reset current spell, it will be set by next cast, current spell on cooldown
                }
                break;
        }

    }

    // FROM SINGLE PLAYER SPELL CASTER
    // // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {
    //     //  initialize health
    //     characterAnimator = GetComponent<Animator>();
    //     currentHealth = maxHealth;
    //     healthBar.InitHealth(currentHealth);

    //     // initialize mana
    //     currentMana = maxMana;
    //     if (ManaBar != null)
    //     {
    //         ManaBar.InitMana(currentMana);
    //     }

    //     // Get all renderers on this character (including children)
    //     allRenderers = GetComponentsInChildren<Renderer>();

    //     // Store original COLORS (not materials!)
    //     if (allRenderers.Length > 0)
    //     {
    //         originalColors = new Color[allRenderers.Length];

    //         for (int i = 0; i < allRenderers.Length; i++)
    //         {
    //             Material mat = allRenderers[i].material; // Creates ONE instance per renderer

    //             // Store the original color
    //             if (mat.HasProperty("_BaseColor"))
    //             {
    //                 originalColors[i] = mat.GetColor("_BaseColor");
    //             }
    //             else if (mat.HasProperty("_Color"))
    //             {
    //                 originalColors[i] = mat.GetColor("_Color");
    //             }
    //         }

    //         Debug.Log($"Stored {originalColors.Length} original colors");
    //     }

    //     // Determine if this is the local player
    //     // SINGLE-PLAYER: Uses Unity tag "Player" to identify the local player
    //     // MULTIPLAYER: Replace CompareTag with network ownership check:
    //     //   - Photon: isLocal = photonView.IsMine;
    //     //   - Mirror: isLocal = isLocalPlayer;
    //     //   - Unity Netcode: isLocal = IsOwner;
    //     bool isLocal = gameObject.CompareTag("Player");

    //     // Register with GameManager (stores LocalPlayer reference)
    //     if (isLocal)
    //     {
    //         if (GameManager.Instance != null)
    //         {
    //             GameManager.Instance.SetLocalPlayer(this);
    //         }
    //         else
    //         {
    //             Debug.LogWarning("[SpellCaster] GameManager.Instance is null! Make sure GameManager exists in the scene.");
    //         }
    //     }

    //     // Register with MatchManager (handles match flow, victory/defeat)
    //     if (MatchManager.Instance != null)
    //     {
    //         MatchManager.Instance.RegisterPlayer(this, isLocal);
    //     }
    //     else
    //     {
    //         Debug.LogWarning("[SpellCaster] MatchManager.Instance is null! Make sure MatchManager exists in the scene.");
    //     }

    //     // Initialize shield
    //     if (shield == null)
    //     {
    //         shield = GetComponent<ShieldComponent>();
    //         if (shield == null)
    //         {
    //             Debug.LogWarning("[SpellCaster] ShieldComponent is null! Make sure ShieldComponent exists in the scene.");
    //         }
    //     }

    //     // Initialize hand auras
    //     leftHandAura.SetActive(false);
    //     rightHandAura.SetActive(false);

    // }

    // public void Cast(Spell spell)
    // {
    //     if (spell.spawnPrefab == null) return;
    //     if (!CanCast(spell) || currentMana < spell.manaCost)
    //     {
    //         Debug.Log($"Not enough mana for {spell.name}!!!");
    //         FindFirstObjectByType<CameraEffects>().Shake();
    //         PlaySmoke();
    //         return;
    //     }

    //     // spell.behavior.Cast(this, spell, enemyPoint);

    //     // characterAnimator.SetTrigger("CastProjectileRight");
    //     // StartCoroutine(CastAfterDelay(0.4f, spell));

    //     // Only enqueue if the spell needs releases
    //     if (spell.releaseCount > 0)
    //     {
    //         spellQueue.Enqueue(new QueuedSpell
    //         {
    //             spell = spell,
    //             releasesRemaining = spell.releaseCount
    //         });
    //     }

    //     Debug.Log($"In spellcaster.cast. Trying to cast {spell.name}. Activating characterAnimator.");
    //     // Trigger Animation Event
    //     if (spell.castTriggerName != null && spell.castTriggerName != "")
    //         characterAnimator.SetTrigger(spell.castTriggerName);
    //     else
    //         characterAnimator.SetTrigger("CastProjectileRight");
    // }

    // // private IEnumerator CastAfterDelay(float delay, Spell spell)
    // // {
    // //     Debug.Log("in castafterdelay");
    // //     yield return new WaitForSeconds(delay);
    // //     Debug.Log("After delay");
    // //     spell.behavior.Cast(this, spell, enemyPoint);
    // // }

    // // Called by Animation Event
    // public void OnReleaseSpell()
    // {
    //     // Check if there's a spell in the queue
    //     if (spellQueue.Count == 0)
    //     {
    //         Debug.LogWarning("OnReleaseSpell called but spell queue is empty!");
    //         return;
    //     }

    //     // Peek the current spell (don't dequeue yet - multi-release spells need multiple calls)
    //     QueuedSpell queued = spellQueue.Peek();
    //     Spell spell = queued.spell;

    //     // Move this to cast() if we want to use mana instantly instead of waiting for the animation to finish
    //     UseMana(spell.manaCost / (float)spell.releaseCount);

    //     Debug.Log($"on spell release from animation event ({spell.spellName}) ----------");
    //     if (spell.behavior != null)
    //     {
    //         spell.behavior.Cast(this, spell, enemyPoint);
    //     }

    //     // Broadcast the event
    //     OnSpellCast?.Invoke(this, spell);

    //     // Decrement releases remaining and dequeue when done
    //     queued.releasesRemaining--;
    //     if (queued.releasesRemaining <= 0)
    //     {
    //         spellQueue.Dequeue();
    //     }
    // }

    // public bool CanCast(Spell spell)
    // {
    //     return currentMana >= spell.manaCost;
    // }

    // public void TakeDamage(float dmg, Transform source)
    // {
    //     if (MatchManager.Instance != null && !MatchManager.Instance.CanFight)
    //     {
    //         Debug.Log($"{name} cannot take damage during pre-round or post-match!");
    //         return;
    //     }

    //     characterAnimator.SetTrigger("DamagedTrigger");

    //     Debug.Log($"DAMAGED BY {dmg} AMOUNT");

    //     // Subtract damage from current health
    //     currentHealth -= dmg;

    //     Debug.Log($"CURRENT HEALTH {currentHealth}");
    //     Debug.Log($"MAX HEALTH {maxHealth}");

    //     // Update health bar
    //     healthBar.SetHealth(currentHealth);

    //     // Trigger low health vignette (only for local player)
    //     if (GameManager.Instance != null && GameManager.Instance.LocalPlayer == this && lowHealthVignette != null)
    //     {
    //         float healthPercent = currentHealth / maxHealth;

    //         if (healthPercent <= criticalHealthThreshold)
    //         {
    //             // Critical health - urgent pulse
    //             lowHealthVignette.PlayUrgentPulse();
    //         }
    //         else if (healthPercent <= lowHealthThreshold)
    //         {
    //             // Low health - normal pulse
    //             lowHealthVignette.PlayLowHealthVignette();
    //         }
    //     }

    //     // Flash on hit
    //     StartCoroutine(FlashOnHit());
    //     //StartCoroutine(HitStop(0.1f));

    //     if (currentHealth <= 0) Die();
    // }

    // private IEnumerator FlashOnHit()
    // {
    //     if (allRenderers == null || allRenderers.Length == 0)
    //     {
    //         Debug.LogWarning("No renderers found on character!");
    //         yield break;
    //     }

    //     Debug.Log($"Flashing {allRenderers.Length} renderers");

    //     // Flash all materials to red
    //     for (int i = 0; i < allRenderers.Length; i++)
    //     {
    //         if (allRenderers[i] != null)
    //         {
    //             Material mat = allRenderers[i].material;

    //             if (mat.HasProperty("_BaseColor"))
    //             {
    //                 mat.SetColor("_BaseColor", Color.red);
    //                 //Debug.Log($"Set {allRenderers[i].name} to red");
    //             }
    //             else if (mat.HasProperty("_Color"))
    //             {
    //                 mat.SetColor("_Color", Color.red);
    //             }
    //         }
    //     }

    //     yield return new WaitForSecondsRealtime(flashDuration);

    //     // Restore original colors
    //     for (int i = 0; i < allRenderers.Length; i++)
    //     {
    //         if (allRenderers[i] != null)
    //         {
    //             Material mat = allRenderers[i].material;

    //             if (mat.HasProperty("_BaseColor"))
    //             {
    //                 mat.SetColor("_BaseColor", originalColors[i]);
    //                 //Debug.Log($"Restored {allRenderers[i].name} to original color: {originalColors[i]}");
    //             }
    //             else if (mat.HasProperty("_Color"))
    //             {
    //                 mat.SetColor("_Color", originalColors[i]);
    //             }
    //         }
    //     }

    //     Debug.Log("Flash complete!");
    // }

    // // private IEnumerator HitStop(float duration = 0.05f)
    // // {
    // //     Time.timeScale = 0f;
    // //     yield return new WaitForSecondsRealtime(duration);
    // //     Time.timeScale = 1f;
    // // }

    // public void UseMana(float mana)
    // {
    //     characterAnimator.SetTrigger("ManaUsedTrigger");
    //     Debug.Log($"Used {mana} Mana");
    //     currentMana -= mana;
    //     Debug.Log($"CURRENT Mana {currentMana}");
    //     Debug.Log($"MAX Mana {maxMana}");
    //     ManaBar.SetMana(currentMana);
    //     StartManaRestore();
    // }

    // private void Die()
    // {
    //     Debug.Log($"{name} died!");
    //     characterAnimator.SetTrigger("Die");
    //     OnDeath?.Invoke(this);
    // }

    // // 🔥 Mini-pool style fizzle feedback
    // public void PlayFizzle()
    // {
    //     if (fizzleVFX == null)
    //     {
    //         Debug.Log("fizzle effect is null");
    //         return;
    //     }

    //     StartCoroutine(FizzleRoutine());
    // }

    // private IEnumerator FizzleRoutine()
    // {
    //     fizzleVFX.SetActive(true);
    //     var ps = fizzleVFX.GetComponent<ParticleSystem>();
    //     ps?.Play();

    //     // Wait for duration or default 1s
    //     yield return new WaitForSeconds(ps?.main.duration ?? 1f);

    //     fizzleVFX.SetActive(false);
    // }

    // // 🔥 Mini-pool style fizzle feedback
    // private void PlaySmoke()
    // {
    //     if (fizzleVFX == null)
    //     {
    //         Debug.Log("fizzle effect is null");
    //         return;
    //     }

    //     StartCoroutine(SmokeRoutine());
    // }

    // private IEnumerator SmokeRoutine()
    // {
    //     outOfManaSmokeVFX.SetActive(true);
    //     var ps = outOfManaSmokeVFX.GetComponent<ParticleSystem>();
    //     ps?.Play();

    //     // Wait for duration or default 1s
    //     yield return new WaitForSeconds(ps?.main.duration ?? 1f);

    //     outOfManaSmokeVFX.SetActive(false);
    // }

    // // Mana restoration logic - restores mana over time
    // private IEnumerator RestoreMana()
    // {
    //     // initial delay before starting the restore
    //     yield return new WaitForSeconds(manaRestoreDelay);

    //     if (currentMana >= maxMana)
    //     {
    //         yield break; // already full
    //     }

    //     float remaining = maxMana - currentMana;

    //     // keep restoring manaRestoreAmount per second (last step may be less)
    //     while (remaining > 0f && currentMana < maxMana)
    //     {
    //         float amountToRestore = Mathf.Min(manaRestoreAmount, remaining, maxMana - currentMana);
    //         currentMana += amountToRestore;
    //         ManaBar.SetMana(currentMana);
    //         remaining -= amountToRestore;
    //         yield return new WaitForSeconds(1f);
    //     }
    // }

    // // Start or restart the mana restoration process
    // public void StartManaRestore()
    // {
    //     // stop any existing restore first
    //     StopManaRestore();

    //     restoreCoroutine = StartCoroutine(RestoreMana());
    // }

    // // Stop the mana restoration
    // public void StopManaRestore()
    // {
    //     if (restoreCoroutine != null)
    //     {
    //         StopCoroutine(restoreCoroutine);
    //         restoreCoroutine = null;
    //     }
    // }

    // public void Heal(float amount, Transform source)
    // {
    //     Debug.Log($"Healed by {amount}");
    //     currentHealth = Mathf.Min(currentHealth + amount, maxHealth); // Cap at max health
    //     Debug.Log($"CURRENT HEALTH {currentHealth}");
    //     Debug.Log($"MAX HEALTH {maxHealth}");
    //     healthBar.SetHealth(currentHealth);

    //     // Stop low health vignette if health is restored above threshold (only for local player)
    //     if (GameManager.Instance != null && GameManager.Instance.LocalPlayer == this && lowHealthVignette != null)
    //     {
    //         float healthPercent = currentHealth / maxHealth;

    //         if (healthPercent > lowHealthThreshold)
    //         {
    //             // Health is back to safe levels - stop the vignette
    //             lowHealthVignette.StopLowHealthVignette();
    //         }
    //     }
    //     // Optional: trigger healing animation
    //     // characterAnimator.SetTrigger("HealTrigger");
    // }


}
