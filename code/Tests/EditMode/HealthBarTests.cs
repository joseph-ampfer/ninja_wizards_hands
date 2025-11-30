using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarTests
{
    private GameObject healthBarObject;
    private HealthBar healthBar;
    private Slider frontBar;
    private Slider backBar;

    [SetUp]
    public void SetUp()
    {
        // Create health bar GameObject
        healthBarObject = new GameObject("HealthBar");
        healthBar = healthBarObject.AddComponent<HealthBar>();

        // Create front bar slider
        var frontBarObject = new GameObject("FrontBar");
        frontBarObject.transform.SetParent(healthBarObject.transform);
        frontBar = frontBarObject.AddComponent<Slider>();

        // Create back bar slider
        var backBarObject = new GameObject("BackBar");
        backBarObject.transform.SetParent(healthBarObject.transform);
        backBar = backBarObject.AddComponent<Slider>();

        // Use reflection to set private fields
        var frontBarField = typeof(HealthBar).GetField("frontBar", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        frontBarField.SetValue(healthBar, frontBar);

        var backBarField = typeof(HealthBar).GetField("backBar", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        backBarField.SetValue(healthBar, backBar);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(healthBarObject);
    }

    [Test]
    public void InitHealth_SetsMaxValue()
    {
        // Arrange
        float maxHealth = 100f;

        // Act
        healthBar.InitHealth(maxHealth);

        // Assert
        Assert.AreEqual(maxHealth, frontBar.maxValue);
        Assert.AreEqual(maxHealth, backBar.maxValue);
    }

    [Test]
    public void InitHealth_SetsCurrentValue()
    {
        // Arrange
        float maxHealth = 100f;

        // Act
        healthBar.InitHealth(maxHealth);

        // Assert
        Assert.AreEqual(maxHealth, frontBar.value);
        Assert.AreEqual(maxHealth, backBar.value);
    }

    [Test]
    public void InitHealth_WithZero_SetsZeroValues()
    {
        // Arrange
        float maxHealth = 0f;

        // Act
        healthBar.InitHealth(maxHealth);

        // Assert
        Assert.AreEqual(0f, frontBar.maxValue);
        Assert.AreEqual(0f, frontBar.value);
    }

    [Test]
    public void SetHealth_UpdatesFrontBarImmediately()
    {
        // Arrange
        healthBar.InitHealth(100f);

        // Act
        healthBar.SetHealth(75f);

        // Assert
        Assert.AreEqual(75f, frontBar.value);
    }

    [Test]
    public void SetHealth_GainingHealth_UpdatesBackBarImmediately()
    {
        // Arrange
        healthBar.InitHealth(100f);
        healthBar.SetHealth(50f);

        // Act
        healthBar.SetHealth(75f);

        // Assert
        Assert.AreEqual(75f, backBar.value, "Back bar should update immediately when gaining health");
    }

    [Test]
    public void SetHealth_LosingHealth_FrontBarUpdatesFirst()
    {
        // Arrange
        healthBar.InitHealth(100f);

        // Act
        healthBar.SetHealth(60f);

        // Assert
        Assert.AreEqual(60f, frontBar.value, "Front bar should update immediately when losing health");
        // Back bar will animate with delay (tested in integration tests)
    }

    [Test]
    public void SetHealth_WithNegativeValue_HandlesGracefully()
    {
        // Arrange
        healthBar.InitHealth(100f);

        // Act & Assert - should not throw
        Assert.DoesNotThrow(() => healthBar.SetHealth(-10f));
    }

    [Test]
    public void SetHealth_AboveMaxValue_HandlesGracefully()
    {
        // Arrange
        healthBar.InitHealth(100f);

        // Act & Assert
        Assert.DoesNotThrow(() => healthBar.SetHealth(150f));
    }
}

