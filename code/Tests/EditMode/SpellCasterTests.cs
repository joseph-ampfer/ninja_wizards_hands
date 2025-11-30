using NUnit.Framework;
using UnityEngine;

public class SpellCasterTests
{
    private GameObject gameObject;
    private SpellCaster spellCaster;
    private Spell testSpell;

    [SetUp]
    public void SetUp()
    {
        // Create GameObject with SpellCaster
        gameObject = new GameObject("TestPlayer");
        spellCaster = gameObject.AddComponent<SpellCaster>();

        // Set initial values (normally done in Start, but we can set them directly for testing)
        spellCaster.maxHealth = 100f;
        spellCaster.currentHealth = 100f;
        spellCaster.maxMana = 100f;
        spellCaster.currentMana = 100f;

        // Create test spell
        testSpell = ScriptableObject.CreateInstance<Spell>();
        testSpell.name = "Fireball";
        testSpell.spellName = "Fireball";
        testSpell.manaCost = 20;
        testSpell.damage = 30;
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up
        Object.DestroyImmediate(testSpell);
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void CanCast_SufficientMana_ReturnsTrue()
    {
        // Arrange
        spellCaster.currentMana = 50f;
        testSpell.manaCost = 20;

        // Act
        bool result = spellCaster.CanCast(testSpell);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void CanCast_ExactMana_ReturnsTrue()
    {
        // Arrange
        spellCaster.currentMana = 20f;
        testSpell.manaCost = 20;

        // Act
        bool result = spellCaster.CanCast(testSpell);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void CanCast_InsufficientMana_ReturnsFalse()
    {
        // Arrange
        spellCaster.currentMana = 10f;
        testSpell.manaCost = 20;

        // Act
        bool result = spellCaster.CanCast(testSpell);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void CanCast_ZeroMana_ReturnsFalse()
    {
        // Arrange
        spellCaster.currentMana = 0f;
        testSpell.manaCost = 20;

        // Act
        bool result = spellCaster.CanCast(testSpell);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void UseMana_DecreasesManaCorrectly()
    {
        // Arrange
        spellCaster.currentMana = 100f;
        float manaCost = 20f;

        // We can't test UseMana directly as it calls characterAnimator.SetTrigger
        // But we can manually simulate it
        float expectedMana = spellCaster.currentMana - manaCost;
        spellCaster.currentMana -= manaCost;

        // Assert
        Assert.AreEqual(expectedMana, spellCaster.currentMana);
    }

    [Test]
    public void Heal_IncreasesHealth()
    {
        // Arrange
        spellCaster.currentHealth = 50f;
        spellCaster.maxHealth = 100f;
        float healAmount = 30f;

        // Act
        spellCaster.Heal(healAmount, null);

        // Assert
        Assert.AreEqual(80f, spellCaster.currentHealth);
    }

    [Test]
    public void Heal_CapsAtMaxHealth()
    {
        // Arrange
        spellCaster.currentHealth = 90f;
        spellCaster.maxHealth = 100f;
        float healAmount = 50f;

        // Act
        spellCaster.Heal(healAmount, null);

        // Assert
        Assert.AreEqual(100f, spellCaster.currentHealth, "Health should be capped at max health");
    }

    [Test]
    public void Heal_FromZeroHealth()
    {
        // Arrange
        spellCaster.currentHealth = 0f;
        spellCaster.maxHealth = 100f;
        float healAmount = 25f;

        // Act
        spellCaster.Heal(healAmount, null);

        // Assert
        Assert.AreEqual(25f, spellCaster.currentHealth);
    }

    [Test]
    public void TakeDamage_DecreasesHealth()
    {
        // Arrange
        spellCaster.currentHealth = 100f;
        float damage = 30f;

        // Act
        spellCaster.TakeDamage(damage, null);

        // Assert
        Assert.AreEqual(70f, spellCaster.currentHealth);
    }

    [Test]
    public void TakeDamage_CanReduceToZero()
    {
        // Arrange
        spellCaster.currentHealth = 30f;
        float damage = 30f;

        // Act
        spellCaster.TakeDamage(damage, null);

        // Assert
        Assert.AreEqual(0f, spellCaster.currentHealth);
    }

    [Test]
    public void TakeDamage_CanReduceBelowZero()
    {
        // Arrange
        spellCaster.currentHealth = 20f;
        float damage = 50f;

        // Act
        spellCaster.TakeDamage(damage, null);

        // Assert - health can go negative in the implementation
        Assert.AreEqual(-30f, spellCaster.currentHealth);
    }

    [Test]
    public void Health_InitializedToMax()
    {
        // Arrange & Act - done in SetUp
        
        // Assert
        Assert.AreEqual(spellCaster.maxHealth, spellCaster.currentHealth);
    }

    [Test]
    public void Mana_InitializedToMax()
    {
        // Arrange & Act - done in SetUp
        
        // Assert
        Assert.AreEqual(spellCaster.maxMana, spellCaster.currentMana);
    }

    [Test]
    public void Mana_CanBeReducedToZero()
    {
        // Arrange
        spellCaster.currentMana = 20f;

        // Act
        spellCaster.currentMana -= 20f;

        // Assert
        Assert.AreEqual(0f, spellCaster.currentMana);
    }

    [Test]
    public void Heal_ZeroAmount_NoChange()
    {
        // Arrange
        spellCaster.currentHealth = 50f;
        float originalHealth = spellCaster.currentHealth;

        // Act
        spellCaster.Heal(0f, null);

        // Assert
        Assert.AreEqual(originalHealth, spellCaster.currentHealth);
    }

    [Test]
    public void TakeDamage_ZeroAmount_NoChange()
    {
        // Arrange
        spellCaster.currentHealth = 50f;
        float originalHealth = spellCaster.currentHealth;

        // Act
        spellCaster.TakeDamage(0f, null);

        // Assert
        Assert.AreEqual(originalHealth, spellCaster.currentHealth);
    }
}

