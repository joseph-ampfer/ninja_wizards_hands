using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Helper utilities for creating test objects and mock data
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a basic spell for testing
    /// </summary>
    public static Spell CreateTestSpell(string name = "TestSpell", int manaCost = 10, int damage = 20)
    {
        var spell = ScriptableObject.CreateInstance<Spell>();
        spell.name = name;
        spell.spellName = name;
        spell.manaCost = manaCost;
        spell.damage = damage;
        spell.lifetime = 2f;
        return spell;
    }

    /// <summary>
    /// Creates a SpellBook with test entries
    /// </summary>
    public static SpellBook CreateTestSpellBook(params Spell[] spells)
    {
        var spellBook = ScriptableObject.CreateInstance<SpellBook>();
        
        foreach (var spell in spells)
        {
            var gesture = new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist);
            var entry = new SpellBook.Entry
            {
                sequence = new List<GesturePair> { gesture },
                spell = spell
            };
            spellBook.entries.Add(entry);
        }

        return spellBook;
    }

    /// <summary>
    /// Creates a SpellCaster with basic configuration
    /// </summary>
    public static SpellCaster CreateTestSpellCaster(GameObject gameObject, float health = 100f, float mana = 100f)
    {
        var caster = gameObject.AddComponent<SpellCaster>();
        caster.maxHealth = health;
        caster.currentHealth = health;
        caster.maxMana = mana;
        caster.currentMana = mana;

        // Add required transform points
        var rightHand = new GameObject("RightHand");
        rightHand.transform.SetParent(gameObject.transform);
        caster.rightHandPoint = rightHand.transform;

        var leftHand = new GameObject("LeftHand");
        leftHand.transform.SetParent(gameObject.transform);
        caster.leftHandPoint = leftHand.transform;

        return caster;
    }

    /// <summary>
    /// Creates a projectile GameObject with required components
    /// </summary>
    public static GameObject CreateTestProjectile(float speed = 20f, float damage = 10f)
    {
        var projectile = new GameObject("TestProjectile");
        projectile.AddComponent<BoxCollider>();
        projectile.AddComponent<Rigidbody>();
        var projectileBase = projectile.AddComponent<ProjectileBase>();
        projectileBase.speed = speed;
        projectileBase.damage = damage;
        return projectile;
    }

    /// <summary>
    /// Creates a target with TargetPoints component
    /// </summary>
    public static GameObject CreateTestTarget(Vector3 position = default)
    {
        var target = new GameObject("TestTarget");
        target.transform.position = position;
        
        var targetPoints = target.AddComponent<TargetPoints>();
        
        var chest = new GameObject("Chest");
        chest.transform.SetParent(target.transform);
        chest.transform.localPosition = Vector3.up;
        targetPoints.chest = chest.transform;

        var feet = new GameObject("Feet");
        feet.transform.SetParent(target.transform);
        feet.transform.localPosition = Vector3.zero;
        targetPoints.feet = feet.transform;

        return target;
    }

    /// <summary>
    /// Creates a gesture sequence from gesture labels
    /// </summary>
    public static List<GesturePair> CreateGestureSequence(params (GestureLabel left, GestureLabel right)[] gestures)
    {
        var sequence = new List<GesturePair>();
        foreach (var (left, right) in gestures)
        {
            sequence.Add(new GesturePair(left, right));
        }
        return sequence;
    }
}

/// <summary>
/// Mock IDamageable for testing damage interactions
/// </summary>
public class MockDamageable : MonoBehaviour, IDamageable
{
    public float LastDamageReceived { get; private set; }
    public Transform LastDamageSource { get; private set; }
    public int DamageCallCount { get; private set; }

    public void TakeDamage(float amount, Transform source)
    {
        LastDamageReceived = amount;
        LastDamageSource = source;
        DamageCallCount++;
    }

    public void Reset()
    {
        LastDamageReceived = 0f;
        LastDamageSource = null;
        DamageCallCount = 0;
    }
}

/// <summary>
/// Mock SpellBehavior for testing
/// </summary>
public class MockSpellBehavior : SpellBehavior
{
    public bool WasCastCalled { get; private set; }
    public SpellCaster LastCaster { get; private set; }
    public Spell LastSpell { get; private set; }
    public Transform LastTarget { get; private set; }

    public override void Cast(SpellCaster caster, Spell spell, Transform target = null)
    {
        WasCastCalled = true;
        LastCaster = caster;
        LastSpell = spell;
        LastTarget = target;
    }

    public void Reset()
    {
        WasCastCalled = false;
        LastCaster = null;
        LastSpell = null;
        LastTarget = null;
    }
}

