using NUnit.Framework;
using UnityEngine;

public class ParryComponentTests
{
    private GameObject parryObject;
    private ParryComponent parry;

    [SetUp]
    public void SetUp()
    {
        parryObject = new GameObject("TestParry");
        parry = parryObject.AddComponent<ParryComponent>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(parryObject);
    }

    [Test]
    public void IsParrying_InitiallyFalse()
    {
        // Assert
        Assert.IsFalse(parry.IsParrying);
    }

    [Test]
    public void StartParry_SetsIsParryingTrue()
    {
        // Act
        parry.StartParry();

        // Assert
        Assert.IsTrue(parry.IsParrying);
    }

    [Test]
    public void ParryWindow_DefaultValue_IsPositive()
    {
        // Assert
        Assert.Greater(parry.parryWindow, 0f);
    }

    [Test]
    public void ManaRestoreAmount_DefaultValue_IsPositive()
    {
        // Assert
        Assert.Greater(parry.manaRestoreAmount, 0);
    }

    [Test]
    public void OnSuccessfulParry_DoesNotThrow()
    {
        // Arrange
        var projectileObject = new GameObject("Projectile");
        projectileObject.AddComponent<BoxCollider>();
        projectileObject.AddComponent<Rigidbody>();
        var projectile = projectileObject.AddComponent<ProjectileBase>();

        // Act & Assert
        Assert.DoesNotThrow(() => parry.OnSuccessfulParry(projectile));

        // Cleanup
        Object.DestroyImmediate(projectileObject);
    }

    [Test]
    public void StartParry_CanBeCalledMultipleTimes()
    {
        // Act
        parry.StartParry();
        Assert.IsTrue(parry.IsParrying);

        parry.StartParry();
        Assert.IsTrue(parry.IsParrying);

        // Assert - no exceptions thrown
    }
}

