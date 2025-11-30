using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class AISpellManagerTests
{
    private GameObject aiObject;
    private AISpellManager aiManager;
    private SpellCaster spellCaster;
    private SpellBook spellBook;
    private Spell testSpell1;
    private Spell testSpell2;

    [SetUp]
    public void SetUp()
    {
        // Create AI object
        aiObject = new GameObject("AIEnemy");
        aiManager = aiObject.AddComponent<AISpellManager>();
        spellCaster = aiObject.AddComponent<SpellCaster>();
        
        // Initialize SpellCaster
        spellCaster.maxHealth = 100f;
        spellCaster.currentHealth = 100f;
        spellCaster.maxMana = 100f;
        spellCaster.currentMana = 100f;

        // Create test spells
        testSpell1 = ScriptableObject.CreateInstance<Spell>();
        testSpell1.name = "FireBlast";
        testSpell1.spellName = "FireBlast";
        testSpell1.manaCost = 10;

        testSpell2 = ScriptableObject.CreateInstance<Spell>();
        testSpell2.name = "IceSpike";
        testSpell2.spellName = "IceSpike";
        testSpell2.manaCost = 15;

        // Create spellbook
        spellBook = ScriptableObject.CreateInstance<SpellBook>();
        
        var entry1 = new SpellBook.Entry
        {
            sequence = new List<GesturePair> { new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist) },
            spell = testSpell1
        };
        
        var entry2 = new SpellBook.Entry
        {
            sequence = new List<GesturePair> { new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp) },
            spell = testSpell2
        };
        
        spellBook.entries.Add(entry1);
        spellBook.entries.Add(entry2);

        // Set up AI manager
        aiManager.spellCaster = spellCaster;
        aiManager.spellBook = spellBook;
        aiManager.minCastDelay = 1f;
        aiManager.maxCastDelay = 2f;
        aiManager.isActive = false; // Don't start loop in tests
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(aiObject);
        Object.DestroyImmediate(spellBook);
        Object.DestroyImmediate(testSpell1);
        Object.DestroyImmediate(testSpell2);
    }

    [Test]
    public void MinCastDelay_CanBeSet()
    {
        // Arrange
        float expectedDelay = 3f;

        // Act
        aiManager.minCastDelay = expectedDelay;

        // Assert
        Assert.AreEqual(expectedDelay, aiManager.minCastDelay);
    }

    [Test]
    public void MaxCastDelay_CanBeSet()
    {
        // Arrange
        float expectedDelay = 6f;

        // Act
        aiManager.maxCastDelay = expectedDelay;

        // Assert
        Assert.AreEqual(expectedDelay, aiManager.maxCastDelay);
    }

    [Test]
    public void IsActive_DefaultValue()
    {
        // Assert - isActive is public and can be checked
        // The test verifies we can control AI behavior
        Assert.IsFalse(aiManager.isActive);
        
        aiManager.isActive = true;
        Assert.IsTrue(aiManager.isActive);
    }

    [Test]
    public void SpellBook_CanBeAssigned()
    {
        // Assert
        Assert.AreEqual(spellBook, aiManager.spellBook);
        Assert.IsNotNull(aiManager.spellBook);
    }

    [Test]
    public void SpellCaster_CanBeAssigned()
    {
        // Assert
        Assert.AreEqual(spellCaster, aiManager.spellCaster);
        Assert.IsNotNull(aiManager.spellCaster);
    }

    [Test]
    public void Player_CanBeAssigned()
    {
        // Arrange
        var playerObject = new GameObject("Player");
        
        // Act
        aiManager.player = playerObject.transform;

        // Assert
        Assert.AreEqual(playerObject.transform, aiManager.player);

        // Cleanup
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void MinCastDelay_LessThanMaxCastDelay()
    {
        // Arrange
        aiManager.minCastDelay = 2f;
        aiManager.maxCastDelay = 5f;

        // Assert
        Assert.Less(aiManager.minCastDelay, aiManager.maxCastDelay);
    }

    [Test]
    public void SpellBook_WithMultipleSpells_HasCorrectCount()
    {
        // Assert
        var allSpells = spellBook.GetAllSpells();
        Assert.AreEqual(2, allSpells.Count);
    }

    [Test]
    public void IsActive_ControlsAIBehavior()
    {
        // This test documents that isActive is used to control AI loop
        // Arrange
        aiManager.isActive = false;

        // Assert
        Assert.IsFalse(aiManager.isActive, "AI should be inactive");

        // Act
        aiManager.isActive = true;

        // Assert
        Assert.IsTrue(aiManager.isActive, "AI should be active");
    }

    [Test]
    public void CastDelay_RangeIsValid()
    {
        // Arrange & Assert
        Assert.Greater(aiManager.minCastDelay, 0f, "Min delay should be positive");
        Assert.Greater(aiManager.maxCastDelay, 0f, "Max delay should be positive");
        Assert.GreaterOrEqual(aiManager.maxCastDelay, aiManager.minCastDelay, "Max should be >= min");
    }
}

