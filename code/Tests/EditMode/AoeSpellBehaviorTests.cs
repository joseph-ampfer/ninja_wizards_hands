using NUnit.Framework;
using UnityEngine;

public class AoeSpellBehaviorTests
{
    private AoeSpellBehavior behavior;
    private SpellCaster caster;
    private Spell spell;
    private GameObject casterObject;
    private GameObject targetObject;
    private GameObject vfxPrefab;

    [SetUp]
    public void SetUp()
    {
        // Create behavior
        behavior = ScriptableObject.CreateInstance<AoeSpellBehavior>();
        behavior.radius = 5f;
        behavior.heightOffset = 0.1f;

        // Create VFX prefab
        vfxPrefab = new GameObject("VFXPrefab");

        // Create spell
        spell = ScriptableObject.CreateInstance<Spell>();
        spell.spawnPrefab = vfxPrefab;
        spell.damage = 30;
        spell.lifetime = 2f;

        // Create caster
        casterObject = new GameObject("Caster");
        caster = casterObject.AddComponent<SpellCaster>();
        caster.maxHealth = 100f;
        caster.currentHealth = 100f;
        caster.maxMana = 100f;
        caster.currentMana = 100f;

        var centerPoint = new GameObject("CenterPoint");
        centerPoint.transform.SetParent(casterObject.transform);
        caster.centerPoint = centerPoint.transform;

        // Create target with TargetPoints
        targetObject = new GameObject("Target");
        var targetPoints = targetObject.AddComponent<TargetPoints>();
        
        var feet = new GameObject("Feet");
        feet.transform.SetParent(targetObject.transform);
        feet.transform.position = new Vector3(0, 0, 5);
        targetPoints.feet = feet.transform;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(behavior);
        Object.DestroyImmediate(spell);
        Object.DestroyImmediate(casterObject);
        Object.DestroyImmediate(targetObject);
        Object.DestroyImmediate(vfxPrefab);
        
        // Clean up any instantiated VFX
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj.name.Contains("VFXPrefab") && obj != vfxPrefab)
            {
                Object.DestroyImmediate(obj);
            }
        }
    }

    [Test]
    public void Radius_CanBeSet()
    {
        // Arrange
        float expectedRadius = 10f;

        // Act
        behavior.radius = expectedRadius;

        // Assert
        Assert.AreEqual(expectedRadius, behavior.radius);
    }

    [Test]
    public void HeightOffset_CanBeSet()
    {
        // Arrange
        float expectedOffset = 0.5f;

        // Act
        behavior.heightOffset = expectedOffset;

        // Assert
        Assert.AreEqual(expectedOffset, behavior.heightOffset);
    }

    [Test]
    public void Cast_InstantiatesVFX()
    {
        // Act
        behavior.Cast(caster, spell, targetObject.transform);

        // Assert - check that VFX was spawned
        var spawnedVFX = GameObject.Find("VFXPrefab(Clone)");
        Assert.IsNotNull(spawnedVFX);
    }

    [Test]
    public void Cast_VFXSpawnsAtTargetFeet()
    {
        // Arrange
        var targetFeet = targetObject.GetComponent<TargetPoints>().feet;
        targetFeet.position = new Vector3(5, 0, 10);

        // Act
        behavior.Cast(caster, spell, targetObject.transform);

        // Assert
        var spawnedVFX = GameObject.Find("VFXPrefab(Clone)");
        Assert.IsNotNull(spawnedVFX);
        
        Vector3 expectedPosition = targetFeet.position + Vector3.up * behavior.heightOffset;
        float distance = Vector3.Distance(spawnedVFX.transform.position, expectedPosition);
        Assert.Less(distance, 0.1f);
    }

    [Test]
    public void Cast_WithMissingTargetPoints_LogsWarning()
    {
        // Arrange
        var badTarget = new GameObject("BadTarget");

        // Act
        behavior.Cast(caster, spell, badTarget.transform);

        // Assert - should not spawn VFX
        var spawnedVFX = GameObject.Find("VFXPrefab(Clone)");
        Assert.IsNull(spawnedVFX);

        // Cleanup
        Object.DestroyImmediate(badTarget);
    }

    [Test]
    public void Cast_WithNullSpawnPrefab_DoesNotThrow()
    {
        // Arrange
        spell.spawnPrefab = null;

        // Act & Assert
        Assert.DoesNotThrow(() => behavior.Cast(caster, spell, targetObject.transform));
    }

    [Test]
    public void Cast_AppendsDamageToIDamageableTargets()
    {
        // Arrange - Create a damageable target within radius
        var damageableObject = new GameObject("Damageable");
        damageableObject.transform.position = targetObject.GetComponent<TargetPoints>().feet.position;
        var damageable = damageableObject.AddComponent<SpellCaster>();
        damageable.maxHealth = 100f;
        damageable.currentHealth = 100f;
        damageable.maxMana = 100f;
        damageable.currentMana = 100f;
        
        // Add collider so Physics.OverlapSphere can find it
        var collider = damageableObject.AddComponent<SphereCollider>();

        float initialHealth = damageable.currentHealth;

        // Act
        behavior.Cast(caster, spell, targetObject.transform);

        // Assert
        Assert.Less(damageable.currentHealth, initialHealth, "Target should have taken damage");

        // Cleanup
        Object.DestroyImmediate(damageableObject);
    }

    [Test]
    public void Cast_AppliesCorrectDamageAmount()
    {
        // Arrange
        var damageableObject = new GameObject("Damageable");
        damageableObject.transform.position = targetObject.GetComponent<TargetPoints>().feet.position;
        var damageable = damageableObject.AddComponent<SpellCaster>();
        damageable.maxHealth = 100f;
        damageable.currentHealth = 100f;
        damageable.maxMana = 100f;
        damageable.currentMana = 100f;
        
        var collider = damageableObject.AddComponent<SphereCollider>();
        
        spell.damage = 25;

        // Act
        behavior.Cast(caster, spell, targetObject.transform);

        // Assert
        Assert.AreEqual(75f, damageable.currentHealth, "Should have taken exactly 25 damage");

        // Cleanup
        Object.DestroyImmediate(damageableObject);
    }
}

