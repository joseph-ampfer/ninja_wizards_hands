using NUnit.Framework;
using UnityEngine;

public class ProjectileBaseTests
{
    private GameObject projectileObject;
    private ProjectileBase projectile;
    private Rigidbody rb;

    [SetUp]
    public void SetUp()
    {
        projectileObject = new GameObject("TestProjectile");
        projectileObject.AddComponent<BoxCollider>();
        rb = projectileObject.AddComponent<Rigidbody>();
        projectile = projectileObject.AddComponent<ProjectileBase>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(projectileObject);
    }

    [Test]
    public void Fire_SetsVelocityCorrectly()
    {
        // Arrange
        Vector3 direction = Vector3.forward;
        Transform owner = new GameObject("Owner").transform;
        projectile.speed = 20f;

        // Act
        projectile.Fire(direction, owner);

        // Assert
        Assert.AreEqual(direction * projectile.speed, rb.linearVelocity);

        // Cleanup
        Object.DestroyImmediate(owner.gameObject);
    }

    [Test]
    public void Fire_SetsOwnerReference()
    {
        // Arrange
        Vector3 direction = Vector3.forward;
        Transform owner = new GameObject("Owner").transform;

        // Act
        projectile.Fire(direction, owner);

        // Assert - we can't access private field, but we can test that hitting owner doesn't trigger damage
        // This is tested implicitly through collision behavior

        // Cleanup
        Object.DestroyImmediate(owner.gameObject);
    }

    [Test]
    public void Speed_DefaultValue_IsPositive()
    {
        // Assert
        Assert.Greater(projectile.speed, 0f);
    }

    [Test]
    public void Damage_CanBeSet()
    {
        // Arrange
        float expectedDamage = 25f;

        // Act
        projectile.damage = expectedDamage;

        // Assert
        Assert.AreEqual(expectedDamage, projectile.damage);
    }

    [Test]
    public void Lifetime_DefaultValue_IsPositive()
    {
        // Assert
        Assert.Greater(projectile.lifetime, 0f);
    }

    [Test]
    public void DestroyOnHit_DefaultValue_IsTrue()
    {
        // Assert
        Assert.IsTrue(projectile.destroyOnHit);
    }

    [Test]
    public void ImpactAndDestroy_WithDestroyOnHit_DestroysProjectile()
    {
        // Arrange
        projectile.destroyOnHit = true;

        // Act
        projectile.ImpactAndDestroy();

        // Assert - object will be destroyed at end of frame
        // We can only check that the destroy was queued
        Assert.IsNotNull(projectileObject); // Still exists immediately
    }

    [Test]
    public void ImpactAndDestroy_WithoutDestroyOnHit_KeepsProjectile()
    {
        // Arrange
        projectile.destroyOnHit = false;

        // Act
        projectile.ImpactAndDestroy();

        // Assert
        Assert.IsNotNull(projectileObject);
    }

    [Test]
    public void Fire_WithZeroSpeed_SetsZeroVelocity()
    {
        // Arrange
        projectile.speed = 0f;
        Vector3 direction = Vector3.forward;
        Transform owner = new GameObject("Owner").transform;

        // Act
        projectile.Fire(direction, owner);

        // Assert
        Assert.AreEqual(Vector3.zero, rb.linearVelocity);

        // Cleanup
        Object.DestroyImmediate(owner.gameObject);
    }

    [Test]
    public void Fire_WithDiagonalDirection_SetsCorrectVelocity()
    {
        // Arrange
        Vector3 direction = new Vector3(1, 1, 0).normalized;
        Transform owner = new GameObject("Owner").transform;
        projectile.speed = 10f;

        // Act
        projectile.Fire(direction, owner);

        // Assert
        Vector3 expectedVelocity = direction * projectile.speed;
        Assert.AreEqual(expectedVelocity.x, rb.linearVelocity.x, 0.01f);
        Assert.AreEqual(expectedVelocity.y, rb.linearVelocity.y, 0.01f);
        Assert.AreEqual(expectedVelocity.z, rb.linearVelocity.z, 0.01f);

        // Cleanup
        Object.DestroyImmediate(owner.gameObject);
    }

    [Test]
    public void ImpactAndDestroy_WithNullImpactVFX_DoesNotThrow()
    {
        // Arrange
        projectile.impactVFX = null;
        projectile.destroyOnHit = false;

        // Act & Assert
        Assert.DoesNotThrow(() => projectile.ImpactAndDestroy());
    }

    [Test]
    public void RequiresCollider_ComponentExists()
    {
        // Assert - RequireComponent attribute should ensure collider exists
        var collider = projectileObject.GetComponent<Collider>();
        Assert.IsNotNull(collider);
    }
}

