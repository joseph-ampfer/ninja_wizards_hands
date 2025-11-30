using UnityEngine;


public enum TargetAnchor
{
    EnemyFeet,
    EnemyChest,
    EnemyHead,
    GroundCenter
}

public enum TargetMode { None, Enemy, Point, Self }

[CreateAssetMenu(fileName = "New Spell", menuName = "Spells/Spell")]
public class Spell : ScriptableObject
{
    [Header("Basic Info")]
    public string spellName;
    public int manaCost;
    public int damage;
    // public TargetAnchor targetAnchor = TargetAnchor.EnemyChest;


    [Header("Visuals and Prefab")]
    public GameObject spawnPrefab;   // the thing that appears in the world e.g. fireball projectile, lightening AOE, ghost AI
    public GameObject castVFX;       // optional (e.g., burst at the hand)
    public GameObject impactVFX;     // optional (on hit or finish)
    public float lifetime = 5f;  // before it's destroyed// MAYBE REMOVE FROM HERE


    [Header("Which behavior to use when casting this spell")]
    public SpellBehavior behavior;
    public string castTriggerName; // Trigger animation

}