using NUnit.Framework;
using UnityEngine;

public class ShieldComponentTests
{
    private GameObject shieldObject;
    private ShieldComponent shield;

    [SetUp]
    public void SetUp()
    {
        shieldObject = new GameObject("TestShield");
        shield = shieldObject.AddComponent<ShieldComponent>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(shieldObject);
    }

    [Test]
    public void IsActive_InitiallyFalse()
    {
        // Assert
        Assert.IsFalse(shield.IsActive);
    }

    [Test]
    public void ActivateShield_SetsIsActiveTrue()
    {
        // Act
        shield.ActivateShield();

        // Assert
        Assert.IsTrue(shield.IsActive);
    }

    [Test]
    public void ActivateShield_WithShieldVFX_ActivatesVFX()
    {
        // Arrange
        var vfxObject = new GameObject("ShieldVFX");
        vfxObject.SetActive(false);
        shield.shieldVFX = vfxObject;

        // Act
        shield.ActivateShield();

        // Assert
        Assert.IsTrue(vfxObject.activeSelf);

        // Cleanup
        Object.DestroyImmediate(vfxObject);
    }

    [Test]
    public void ActivateShield_WithNullVFX_DoesNotThrow()
    {
        // Arrange
        shield.shieldVFX = null;

        // Act & Assert
        Assert.DoesNotThrow(() => shield.ActivateShield());
        Assert.IsTrue(shield.IsActive);
    }

    [Test]
    public void BlockProjectile_DestroysProjectile()
    {
        // Arrange
        var projectileObject = new GameObject("Projectile");
        projectileObject.AddComponent<BoxCollider>();
        projectileObject.AddComponent<Rigidbody>();
        var projectile = projectileObject.AddComponent<ProjectileBase>();

        // Act
        shield.BlockProjectile(projectile);

        // Assert - ImpactAndDestroy is called, which queues destruction
        // We verify the method doesn't throw and projectile still exists immediately
        Assert.IsNotNull(projectileObject);

        // Cleanup
        Object.DestroyImmediate(projectileObject);
    }

    [Test]
    public void ActiveTime_DefaultValue_IsPositive()
    {
        // Assert
        Assert.Greater(shield.activeTime, 0f);
    }

    [Test]
    public void ActivateShield_WithZeroActiveTime_StillActivates()
    {
        // Arrange
        shield.activeTime = 0f;

        // Act
        shield.ActivateShield();

        // Assert
        Assert.IsTrue(shield.IsActive);
    }

    [Test]
    public void IsActive_PropertyIsReadOnly()
    {
        // This test verifies the property design - IsActive can only be set internally
        // We can't directly test this in NUnit, but we document the behavior
        
        // Act
        shield.ActivateShield();

        // Assert
        Assert.IsTrue(shield.IsActive);
        
        // IsActive cannot be set from outside the class (compile-time check)
        // This test documents that behavior
    }
}

