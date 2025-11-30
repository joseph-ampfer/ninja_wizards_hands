using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ManaBarTests
{
    private GameObject manaBarObject;
    private ManaBar manaBar;
    private Slider frontBar;
    private Slider backBar;

    [SetUp]
    public void SetUp()
    {
        // Create mana bar GameObject
        manaBarObject = new GameObject("ManaBar");
        manaBar = manaBarObject.AddComponent<ManaBar>();

        // Create front bar slider
        var frontBarObject = new GameObject("FrontBar");
        frontBarObject.transform.SetParent(manaBarObject.transform);
        frontBar = frontBarObject.AddComponent<Slider>();

        // Create back bar slider
        var backBarObject = new GameObject("BackBar");
        backBarObject.transform.SetParent(manaBarObject.transform);
        backBar = backBarObject.AddComponent<Slider>();

        // Use reflection to set private fields
        var frontBarField = typeof(ManaBar).GetField("frontBar", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        frontBarField.SetValue(manaBar, frontBar);

        var backBarField = typeof(ManaBar).GetField("backBar", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        backBarField.SetValue(manaBar, backBar);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(manaBarObject);
    }

    [Test]
    public void InitMana_SetsMaxValue()
    {
        // Arrange
        float maxMana = 100f;

        // Act
        manaBar.InitMana(maxMana);

        // Assert
        Assert.AreEqual(maxMana, frontBar.maxValue);
        Assert.AreEqual(maxMana, backBar.maxValue);
    }

    [Test]
    public void InitMana_SetsCurrentValue()
    {
        // Arrange
        float maxMana = 100f;

        // Act
        manaBar.InitMana(maxMana);

        // Assert
        Assert.AreEqual(maxMana, frontBar.value);
        Assert.AreEqual(maxMana, backBar.value);
    }

    [Test]
    public void SetMana_UpdatesFrontBarImmediately()
    {
        // Arrange
        manaBar.InitMana(100f);

        // Act
        manaBar.SetMana(75f);

        // Assert
        Assert.AreEqual(75f, frontBar.value);
    }

    [Test]
    public void SetMana_GainingMana_UpdatesBackBarImmediately()
    {
        // Arrange
        manaBar.InitMana(100f);
        manaBar.SetMana(50f);

        // Act
        manaBar.SetMana(80f);

        // Assert
        Assert.AreEqual(80f, backBar.value, "Back bar should update immediately when gaining mana");
    }

    [Test]
    public void SetMana_LosingMana_FrontBarUpdatesFirst()
    {
        // Arrange
        manaBar.InitMana(100f);

        // Act
        manaBar.SetMana(60f);

        // Assert
        Assert.AreEqual(60f, frontBar.value, "Front bar should update immediately when losing mana");
        // Back bar will animate with delay (tested in integration tests)
    }

    [Test]
    public void SetMana_ClampsBelowZero()
    {
        // Arrange
        manaBar.InitMana(100f);

        // Act
        manaBar.SetMana(-10f);

        // Assert
        Assert.AreEqual(0f, frontBar.value, "Mana should be clamped to 0");
    }

    [Test]
    public void SetMana_ClampsAboveMax()
    {
        // Arrange
        manaBar.InitMana(100f);

        // Act
        manaBar.SetMana(150f);

        // Assert
        Assert.AreEqual(100f, frontBar.value, "Mana should be clamped to max value");
    }
}

