using NUnit.Framework;
using UnityEngine;

public class HealSpellBehaviorTests
{
    private HealSpellBehavior behavior;
    private SpellCaster caster;
    private Spell spell;
    private GameObject casterObject;
    private GameObject healVFXPrefab;

    [SetUp]
    public void SetUp()
    {
        // Create behavior
        behavior = ScriptableObject.CreateInstance<HealSpellBehavior>();
        behavior.heightOffset = 0.1f;

        // Create heal VFX prefab
        healVFXPrefab = new GameObject("HealVFXPrefab");

        // Create spell
        spell = ScriptableObject.CreateInstance<Spell>();
        spell.spawnPrefab = healVFXPrefab;
        spell.damage = 20; // Using damage field as heal amount
        spell.lifetime = 2f;

        // Create caster
        casterObject = new GameObject("Caster");
        caster = casterObject.AddComponent<SpellCaster>();
        caster.maxHealth = 100f;
        caster.currentHealth = 50f; // Damaged
        caster.maxMana = 100f;
        caster.currentMana = 100f;

        var feetPoint = new GameObject("FeetPoint");
        feetPoint.transform.SetParent(casterObject.transform);
        feetPoint.transform.position = new Vector3(0, 0, 0);
        caster.feetPoint = feetPoint.transform;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(behavior);
        Object.DestroyImmediate(spell);
        Object.DestroyImmediate(casterObject);
        Object.DestroyImmediate(healVFXPrefab);
        
        // Clean up any instantiated VFX
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("HealVFXPrefab") && obj != healVFXPrefab)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    [Test]
    public void Cast_HealsCaster()
    {
        // Arrange
        float initialHealth = caster.currentHealth;
        Assert.AreEqual(50f, initialHealth);

        // Act
        behavior.Cast(caster, spell, null);

        // Assert
        Assert.Greater(caster.currentHealth, initialHealth, "Health should increase after heal");
        Assert.AreEqual(70f, caster.currentHealth, "Should heal by spell.damage amount");
    }

    [Test]
    public void Cast_UsesSpellDamageAsHealAmount()
    {
        // Arrange
        caster.currentHealth = 30f;
        spell.damage = 25;

        // Act
        behavior.Cast(caster, spell, null);

        // Assert
        Assert.AreEqual(55f, caster.currentHealth);
    }

    [Test]
    public void Cast_InstantiatesVFX()
    {
        // Act
        behavior.Cast(caster, spell, null);

        // Assert
        var spawnedVFX = GameObject.Find("HealVFXPrefab(Clone)");
        Assert.IsNotNull(spawnedVFX);
    }

    [Test]
    public void Cast_VFXSpawnsAtFeetWithOffset()
    {
        // Arrange
        caster.feetPoint.position = new Vector3(5, 0, 10);

        // Act
        behavior.Cast(caster, spell, null);

        // Assert
        var spawnedVFX = GameObject.Find("HealVFXPrefab(Clone)");
        Assert.IsNotNull(spawnedVFX);
        
        Vector3 expectedPosition = caster.feetPoint.position + Vector3.up * behavior.heightOffset;
        float distance = Vector3.Distance(spawnedVFX.transform.position, expectedPosition);
        Assert.Less(distance, 0.1f);
    }

    [Test]
    public void Cast_WithoutFeetPoint_UsesTransformPosition()
    {
        // Arrange
        caster.feetPoint = null;
        casterObject.transform.position = new Vector3(3, 2, 1);

        // Act
        behavior.Cast(caster, spell, null);

        // Assert
        var spawnedVFX = GameObject.Find("HealVFXPrefab(Clone)");
        Assert.IsNotNull(spawnedVFX);
        
        // VFX should spawn near caster position
        float distance = Vector3.Distance(spawnedVFX.transform.position, casterObject.transform.position);
        Assert.Less(distance, 1f);
    }

    [Test]
    public void Cast_WithNullVFXPrefab_StillHeals()
    {
        // Arrange
        spell.spawnPrefab = null;
        caster.currentHealth = 40f;

        // Act
        behavior.Cast(caster, spell, null);

        // Assert
        Assert.AreEqual(60f, caster.currentHealth, "Should still heal even without VFX");
    }

    [Test]
    public void Cast_IgnoresTargetParameter()
    {
        // Arrange
        var target = new GameObject("Target");
        float initialHealth = caster.currentHealth;

        // Act
        behavior.Cast(caster, spell, target.transform);

        // Assert - caster should be healed, not target
        Assert.Greater(caster.currentHealth, initialHealth);

        // Cleanup
        Object.DestroyImmediate(target);
    }
}

