using UnityEngine;
using Unity.Netcode;

public class NetworkSpellController : NetworkBehaviour
{
    [SerializeField] private SpellBook spellbook;
    [SerializeField] private NetworkSpellCaster spellCaster;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

// Disable for builds
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        // get input, if f is pressed, cast the spell
        if (Input.GetKeyDown(KeyCode.P))
        {
            spellCaster.Cast(SpellNameEnum.PurpleBall);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            spellCaster.Cast(SpellNameEnum.GroundIce);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            spellCaster.Cast(SpellNameEnum.AirCombo);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            spellCaster.Cast(SpellNameEnum.DodgeLeft);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            spellCaster.Cast(SpellNameEnum.FlameBall1);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            spellCaster.Cast(SpellNameEnum.Slash);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            spellCaster.Cast(SpellNameEnum.IceStorm);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            spellCaster.Cast(SpellNameEnum.Poison);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            spellCaster.Cast(SpellNameEnum.StarFall);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            spellCaster.Cast(SpellNameEnum.HealLvl1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            spellCaster.Cast(SpellNameEnum.HealLvl2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            spellCaster.Cast(SpellNameEnum.HealLvl3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            spellCaster.Cast(SpellNameEnum.HealLvl4);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            spellCaster.Cast(SpellNameEnum.HolySword);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            spellCaster.Cast(SpellNameEnum.Shield);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            spellCaster.Cast(SpellNameEnum.DodgeLeft1);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            spellCaster.Cast(SpellNameEnum.DodgeRight1);
        }
    }
#endif


}
