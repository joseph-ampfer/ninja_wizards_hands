using NUnit.Framework;
using UnityEngine;

public class ShieldSpellBehaviorTests
{
    private ShieldSpellBehavior behavior;
    private SpellCaster caster;
    private Spell spell;
    private GameObject casterObject;

    [SetUp]
    public void SetUp()
    {
        // Create behavior
        behavior = ScriptableObject.CreateInstance<ShieldSpellBehavior>();

        // Create spell
        spell = ScriptableObject.CreateInstance<Spell>();
        spell.spellName = "Shield";

        // Create caster with shield component
        casterObject = new GameObject("Caster");
        caster = casterObject.AddComponent<SpellCaster>();
        caster.maxHealth = 100f;
        caster.currentHealth = 100f;
        caster.maxMana = 100f;
        caster.currentMana = 100f;
        
        casterObject.AddComponent<ShieldComponent>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(behavior);
        Object.DestroyImmediate(spell);
        Object.DestroyImmediate(casterObject);
    }

    [Test]
    public void Cast_ActivatesShieldComponent()
    {
        // Arrange
        var shield = caster.GetComponent<ShieldComponent>();
        Assert.IsFalse(shield.IsActive, "Shield should start inactive");

        // Act
        behavior.Cast(caster, spell, null);

        // Assert
        Assert.IsTrue(shield.IsActive, "Shield should be active after cast");
    }

    [Test]
    public void Cast_WithoutShieldComponent_DoesNotThrow()
    {
        // Arrange
        var casterWithoutShield = new GameObject("CasterNoShield");
        var casterComp = casterWithoutShield.AddComponent<SpellCaster>();
        casterComp.maxHealth = 100f;
        casterComp.currentHealth = 100f;
        casterComp.maxMana = 100f;
        casterComp.currentMana = 100f;

        // Act & Assert
        Assert.DoesNotThrow(() => behavior.Cast(casterComp, spell, null));

        // Cleanup
        Object.DestroyImmediate(casterWithoutShield);
    }

    [Test]
    public void Cast_IgnoresTargetParameter()
    {
        // Arrange
        var target = new GameObject("Target");
        var shield = caster.GetComponent<ShieldComponent>();

        // Act
        behavior.Cast(caster, spell, target.transform);

        // Assert - shield should still activate even with target provided
        Assert.IsTrue(shield.IsActive);

        // Cleanup
        Object.DestroyImmediate(target);
    }

    [Test]
    public void Cast_WorksWithNullTarget()
    {
        // Arrange
        var shield = caster.GetComponent<ShieldComponent>();

        // Act & Assert
        Assert.DoesNotThrow(() => behavior.Cast(caster, spell, null));
        Assert.IsTrue(shield.IsActive);
    }
}

