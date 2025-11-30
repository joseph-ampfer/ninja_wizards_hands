using NUnit.Framework;
using UnityEngine;

public class ProjectileSpellBehaviorTests
{
    private ProjectileSpellBehavior behavior;
    private SpellCaster caster;
    private Spell spell;
    private GameObject casterObject;
    private GameObject targetObject;
    private GameObject projectilePrefab;

    [SetUp]
    public void SetUp()
    {
        // Create behavior
        behavior = ScriptableObject.CreateInstance<ProjectileSpellBehavior>();

        // Create caster
        casterObject = new GameObject("Caster");
        caster = casterObject.AddComponent<SpellCaster>();
        
        // Create fire points
        var rightHand = new GameObject("RightHand");
        rightHand.transform.SetParent(casterObject.transform);
        caster.rightHandPoint = rightHand.transform;

        var staffTip = new GameObject("StaffTip");
        staffTip.transform.SetParent(casterObject.transform);
        caster.staffTip = staffTip.transform;

        // Create projectile prefab
        projectilePrefab = new GameObject("ProjectilePrefab");
        projectilePrefab.AddComponent<BoxCollider>();
        projectilePrefab.AddComponent<Rigidbody>();
        projectilePrefab.AddComponent<ProjectileBase>();

        // Create spell
        spell = ScriptableObject.CreateInstance<Spell>();
        spell.spawnPrefab = projectilePrefab;
        spell.damage = 25;

        // Create target with TargetPoints
        targetObject = new GameObject("Target");
        var targetPoints = targetObject.AddComponent<TargetPoints>();
        
        var chest = new GameObject("Chest");
        chest.transform.SetParent(targetObject.transform);
        chest.transform.position = new Vector3(0, 1, 5);
        targetPoints.chest = chest.transform;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(behavior);
        Object.DestroyImmediate(spell);
        Object.DestroyImmediate(casterObject);
        Object.DestroyImmediate(targetObject);
        Object.DestroyImmediate(projectilePrefab);
        
        // Clean up any instantiated projectiles
        var instantiatedProjectiles = Object.FindObjectsOfType<ProjectileBase>();
        foreach (var proj in instantiatedProjectiles)
        {
            Object.DestroyImmediate(proj.gameObject);
        }
    }

    [Test]
    public void Cast_WithStaffTip_UsesStaffTipAsFirePoint()
    {
        // Arrange
        caster.staffTip.position = new Vector3(1, 2, 3);

        // Act
        behavior.Cast(caster, spell, targetObject.transform);

        // Assert - check that a projectile was spawned
        var spawnedProjectile = Object.FindFirstObjectByType<ProjectileBase>();
        Assert.IsNotNull(spawnedProjectile);
        
        // The projectile should be near the staff tip position
        float distance = Vector3.Distance(spawnedProjectile.transform.position, caster.staffTip.position);
        Assert.Less(distance, 0.1f);
    }

    [Test]
    public void Cast_WithoutStaffTip_UsesRightHandPoint()
    {
        // Arrange
        caster.staffTip = null;
        caster.rightHandPoint.position = new Vector3(2, 1, 1);

        // Act
        behavior.Cast(caster, spell, targetObject.transform);

        // Assert
        var spawnedProjectile = Object.FindFirstObjectByType<ProjectileBase>();
        Assert.IsNotNull(spawnedProjectile);
        
        float distance = Vector3.Distance(spawnedProjectile.transform.position, caster.rightHandPoint.position);
        Assert.Less(distance, 0.1f);
    }

    [Test]
    public void Cast_SetsDamageFromSpell()
    {
        // Arrange
        spell.damage = 42;

        // Act
        behavior.Cast(caster, spell, targetObject.transform);

        // Assert
        var spawnedProjectile = Object.FindFirstObjectByType<ProjectileBase>();
        Assert.IsNotNull(spawnedProjectile);
        Assert.AreEqual(42, spawnedProjectile.damage);
    }

    [Test]
    public void Cast_WithMissingTargetPoints_LogsWarning()
    {
        // Arrange
        var targetWithoutPoints = new GameObject("BadTarget");

        // Act - should log warning and return early
        behavior.Cast(caster, spell, targetWithoutPoints.transform);

        // Assert - no projectile should be spawned
        var spawnedProjectile = Object.FindFirstObjectByType<ProjectileBase>();
        Assert.IsNull(spawnedProjectile);

        // Cleanup
        Object.DestroyImmediate(targetWithoutPoints);
    }

    [Test]
    public void Cast_InstantiatesProjectile()
    {
        // Act
        behavior.Cast(caster, spell, targetObject.transform);

        // Assert
        var spawnedProjectile = Object.FindFirstObjectByType<ProjectileBase>();
        Assert.IsNotNull(spawnedProjectile);
    }

    [Test]
    public void Cast_ProjectileAimsAtTarget()
    {
        // Arrange
        caster.staffTip.position = Vector3.zero;
        targetObject.GetComponent<TargetPoints>().chest.position = new Vector3(0, 0, 10);

        // Act
        behavior.Cast(caster, spell, targetObject.transform);

        // Assert
        var spawnedProjectile = Object.FindFirstObjectByType<ProjectileBase>();
        Assert.IsNotNull(spawnedProjectile);
        
        // Check projectile is looking roughly toward target (forward should be close to Vector3.forward)
        float forwardDot = Vector3.Dot(spawnedProjectile.transform.forward, Vector3.forward);
        Assert.Greater(forwardDot, 0.9f, "Projectile should be facing toward target");
    }

    [Test]
    public void Cast_FiresProjectile()
    {
        // Act
        behavior.Cast(caster, spell, targetObject.transform);

        // Assert
        var spawnedProjectile = Object.FindFirstObjectByType<ProjectileBase>();
        Assert.IsNotNull(spawnedProjectile);
        
        // After Fire is called, the projectile should have velocity
        var rb = spawnedProjectile.GetComponent<Rigidbody>();
        Assert.IsNotNull(rb);
        Assert.Greater(rb.linearVelocity.magnitude, 0f, "Projectile should have velocity after firing");
    }
}

