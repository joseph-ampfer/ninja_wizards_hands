using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AISpellManager : MonoBehaviour
{
    [Header("References")]
    public SpellCaster spellCaster;
    public SpellBook spellBook;   // so it knows what spells exist
    public Transform player;      // target reference to aim at

    [Header("AI Settings")]
    public float minCastDelay = 2f;
    public float maxCastDelay = 5f;
    public bool isActive = true;

    private List<Spell> spells;

    private void Start()
    {
        Debug.Log("starting AISpellManager");
        if (!spellCaster) spellCaster = GetComponent<SpellCaster>();
        //if (!player) player = GameObject.FindWithTag("Player").transform;
        spells = spellBook.GetAllSpells();

        StartCoroutine(AILoop());
    }

    private IEnumerator AILoop()
    {
        while (isActive && spellBook)
        {
            yield return new WaitForSeconds(Random.Range(minCastDelay, maxCastDelay));

            // pick a spell from the spellbook
            Spell spell = PickRandomSpell();
            //spell = spellBook.GetSpellByName("Ground Ice");

            if (spell != null && spellCaster.CanCast(spell))
            {
                Debug.Log($"trying to cast {spell.name} from AISpellManager");
                // maybe aim at player
                //AimAtPlayer();
                spellCaster.Cast(spell);
            }
        }
    }

    private Spell PickRandomSpell()
    {
        if (spells == null || spells.Count == 0) return null;
        return spells[Random.Range(0, spells.Count)];
    }

    // private void AimAtPlayer()
    // {
    //     if (player == null) return;
    //     Vector3 dir = (player.position - spellCaster.transform.position).normalized;
    //     spellCaster.transform.forward = dir;
    // }
}
