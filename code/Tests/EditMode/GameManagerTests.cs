using NUnit.Framework;
using UnityEngine;

public class GameManagerTests
{
    private GameObject gameManagerObject;
    private GameManager gameManager;

    [SetUp]
    public void SetUp()
    {
        // Clear any existing instance
        if (GameManager.Instance != null)
        {
            Object.DestroyImmediate(GameManager.Instance.gameObject);
        }

        // Create new GameManager
        gameManagerObject = new GameObject("GameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up
        if (gameManagerObject != null)
        {
            Object.DestroyImmediate(gameManagerObject);
        }
    }

    [Test]
    public void Awake_CreatesSingletonInstance()
    {
        // Act - Awake is called automatically when component is added

        // Assert
        Assert.IsNotNull(GameManager.Instance);
    }

    [Test]
    public void SetLocalPlayer_UpdatesLocalPlayerReference()
    {
        // Arrange
        var playerObject = new GameObject("Player");
        var spellCaster = playerObject.AddComponent<SpellCaster>();

        // Act
        gameManager.SetLocalPlayer(spellCaster);

        // Assert
        Assert.AreEqual(spellCaster, gameManager.LocalPlayer);

        // Clean up
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void SetLocalPlayer_CanBeCalledMultipleTimes()
    {
        // Arrange
        var playerObject1 = new GameObject("Player1");
        var spellCaster1 = playerObject1.AddComponent<SpellCaster>();

        var playerObject2 = new GameObject("Player2");
        var spellCaster2 = playerObject2.AddComponent<SpellCaster>();

        // Act
        gameManager.SetLocalPlayer(spellCaster1);
        Assert.AreEqual(spellCaster1, gameManager.LocalPlayer);

        gameManager.SetLocalPlayer(spellCaster2);
        Assert.AreEqual(spellCaster2, gameManager.LocalPlayer);

        // Clean up
        Object.DestroyImmediate(playerObject1);
        Object.DestroyImmediate(playerObject2);
    }

    [Test]
    public void LocalPlayer_InitiallyNull()
    {
        // Assert
        Assert.IsNull(gameManager.LocalPlayer);
    }

    [Test]
    public void Singleton_OnlyOneInstanceExists()
    {
        // Arrange
        var secondGameManagerObject = new GameObject("GameManager2");
        var secondGameManager = secondGameManagerObject.AddComponent<GameManager>();

        // Act - second GameManager's Awake should destroy itself

        // Assert
        // The second GameManager should mark itself for destruction
        Assert.AreEqual(gameManager, GameManager.Instance, 
            "The first GameManager should remain as the singleton instance");

        // Clean up
        Object.DestroyImmediate(secondGameManagerObject);
    }

    [Test]
    public void SetLocalPlayer_WithNull_SetsNull()
    {
        // Arrange
        var playerObject = new GameObject("Player");
        var spellCaster = playerObject.AddComponent<SpellCaster>();
        gameManager.SetLocalPlayer(spellCaster);

        // Act
        gameManager.SetLocalPlayer(null);

        // Assert
        Assert.IsNull(gameManager.LocalPlayer);

        // Clean up
        Object.DestroyImmediate(playerObject);
    }
}

