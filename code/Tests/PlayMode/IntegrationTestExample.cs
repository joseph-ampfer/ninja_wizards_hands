using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Example integration tests showing how to test systems working together in Play Mode
/// These tests use Unity's full lifecycle (Awake, Start, Update) and can test time-based behavior
/// </summary>
public class IntegrationTestExample
{
    [UnityTest]
    public IEnumerator SpellCaster_WhenCasting_ConsumesMana()
    {
        // Arrange
        var casterObject = new GameObject("TestCaster");
        var caster = TestHelpers.CreateTestSpellCaster(casterObject, health: 100f, mana: 100f);
        
        var spell = TestHelpers.CreateTestSpell("Fireball", manaCost: 20, damage: 30);
        var behavior = ScriptableObject.CreateInstance<MockSpellBehavior>();
        spell.behavior = behavior;

        float initialMana = caster.currentMana;

        // Act - Wait for Start() to be called
        yield return null;

        // Manually call UseMana since we can't trigger animation events in tests
        caster.UseMana(spell.manaCost);

        // Assert
        Assert.AreEqual(initialMana - spell.manaCost, caster.currentMana, 
            "Mana should be consumed when spell is cast");

        // Cleanup
        Object.Destroy(casterObject);
        Object.Destroy(spell);
        Object.Destroy(behavior);
        
        yield return null; // Wait for cleanup
    }

    [UnityTest]
    public IEnumerator SpellCaster_WhenHealing_RestoresHealth()
    {
        // Arrange
        var casterObject = new GameObject("TestCaster");
        var caster = TestHelpers.CreateTestSpellCaster(casterObject, health: 100f, mana: 100f);
        
        // Damage the caster first
        caster.currentHealth = 50f;
        float damagedHealth = caster.currentHealth;

        // Act
        yield return null; // Wait one frame for Start()
        
        caster.Heal(30f, null);

        // Assert
        Assert.Greater(caster.currentHealth, damagedHealth, "Health should increase after healing");
        Assert.AreEqual(80f, caster.currentHealth, "Should heal by exact amount");

        // Cleanup
        Object.Destroy(casterObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ShieldComponent_AfterActivation_BecomesActive()
    {
        // Arrange
        var shieldObject = new GameObject("TestShield");
        var shield = shieldObject.AddComponent<ShieldComponent>();
        shield.activeTime = 2f;

        // Act
        yield return null; // Wait for Start()
        
        shield.ActivateShield();

        // Assert
        Assert.IsTrue(shield.IsActive, "Shield should be active immediately after activation");

        // Cleanup
        Object.Destroy(shieldObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Projectile_AfterFiring_HasVelocity()
    {
        // Arrange
        var projectileObject = TestHelpers.CreateTestProjectile(speed: 20f);
        var projectile = projectileObject.GetComponent<ProjectileBase>();
        var rb = projectileObject.GetComponent<Rigidbody>();
        
        var ownerObject = new GameObject("Owner");

        // Act
        yield return null; // Wait for Start()
        
        projectile.Fire(Vector3.forward, ownerObject.transform);
        yield return new WaitForFixedUpdate(); // Wait for physics update

        // Assert
        Assert.Greater(rb.linearVelocity.magnitude, 0f, "Projectile should have velocity after firing");
        Vector3 expectedVelocity = Vector3.forward * projectile.speed;
        Assert.AreEqual(expectedVelocity.magnitude, rb.linearVelocity.magnitude, 0.1f);

        // Cleanup
        Object.Destroy(projectileObject);
        Object.Destroy(ownerObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator GameManager_SetLocalPlayer_UpdatesReference()
    {
        // Arrange
        var gameManagerObject = new GameObject("GameManager");
        var gameManager = gameManagerObject.AddComponent<GameManager>();
        
        var playerObject = new GameObject("Player");
        var player = TestHelpers.CreateTestSpellCaster(playerObject);

        // Act
        yield return null; // Wait for Awake()
        
        gameManager.SetLocalPlayer(player);

        // Assert
        Assert.AreEqual(player, gameManager.LocalPlayer, "Local player should be set correctly");

        // Cleanup
        Object.Destroy(gameManagerObject);
        Object.Destroy(playerObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SpellBook_MatchingSequence_ReturnsCorrectSpell()
    {
        // Arrange
        var spell1 = TestHelpers.CreateTestSpell("Fireball", 10, 20);
        var spell2 = TestHelpers.CreateTestSpell("Ice Blast", 15, 25);
        
        var spellBook = ScriptableObject.CreateInstance<SpellBook>();
        
        var sequence1 = TestHelpers.CreateGestureSequence(
            (GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        );
        
        var sequence2 = TestHelpers.CreateGestureSequence(
            (GestureLabel.Victory, GestureLabel.ThumbsUp)
        );

        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence1, spell = spell1 });
        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence2, spell = spell2 });

        // Act
        yield return null;
        
        bool found = spellBook.TryGetSpell(sequence1, out Spell foundSpell);

        // Assert
        Assert.IsTrue(found, "Should find spell with matching sequence");
        Assert.AreEqual(spell1, foundSpell, "Should return correct spell");

        // Cleanup
        Object.Destroy(spell1);
        Object.Destroy(spell2);
        Object.Destroy(spellBook);
        yield return null;
    }
}

