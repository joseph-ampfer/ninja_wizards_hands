using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpellBookTests
{
    private SpellBook spellBook;
    private Spell testSpell1;
    private Spell testSpell2;

    [SetUp]
    public void SetUp()
    {
        // Create test SpellBook
        spellBook = ScriptableObject.CreateInstance<SpellBook>();

        // Create test spells
        testSpell1 = ScriptableObject.CreateInstance<Spell>();
        testSpell1.name = "Fireball";
        testSpell1.spellName = "Fireball";
        testSpell1.manaCost = 10;

        testSpell2 = ScriptableObject.CreateInstance<Spell>();
        testSpell2.name = "Ice Blast";
        testSpell2.spellName = "Ice Blast";
        testSpell2.manaCost = 15;
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up ScriptableObjects
        Object.DestroyImmediate(spellBook);
        Object.DestroyImmediate(testSpell1);
        Object.DestroyImmediate(testSpell2);
    }

    [Test]
    public void TryGetSpell_MatchingSequence_ReturnsTrue()
    {
        // Arrange
        var sequence = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var entry = new SpellBook.Entry
        {
            sequence = sequence,
            spell = testSpell1
        };
        spellBook.entries.Add(entry);

        // Act
        bool result = spellBook.TryGetSpell(sequence, out Spell spell);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(testSpell1, spell);
    }

    [Test]
    public void TryGetSpell_NonMatchingSequence_ReturnsFalse()
    {
        // Arrange
        var sequence1 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var sequence2 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        var entry = new SpellBook.Entry
        {
            sequence = sequence1,
            spell = testSpell1
        };
        spellBook.entries.Add(entry);

        // Act
        bool result = spellBook.TryGetSpell(sequence2, out Spell spell);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(spell);
    }

    [Test]
    public void TryGetSpell_EmptyBuffer_ReturnsFalse()
    {
        // Arrange
        var sequence = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var entry = new SpellBook.Entry
        {
            sequence = sequence,
            spell = testSpell1
        };
        spellBook.entries.Add(entry);

        var emptyBuffer = new List<GesturePair>();

        // Act
        bool result = spellBook.TryGetSpell(emptyBuffer, out Spell spell);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(spell);
    }

    [Test]
    public void TryGetSpell_MultiStepSequence_FindsCorrectSpell()
    {
        // Arrange
        var sequence1 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist),
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        var sequence2 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.PointingUp, GestureLabel.ILoveYou)
        };

        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence1, spell = testSpell1 });
        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence2, spell = testSpell2 });

        // Act
        bool result = spellBook.TryGetSpell(sequence1, out Spell spell);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(testSpell1, spell);
    }

    [Test]
    public void GetAllSpells_ReturnsAllSpells()
    {
        // Arrange
        var sequence1 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var sequence2 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence1, spell = testSpell1 });
        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence2, spell = testSpell2 });

        // Act
        var allSpells = spellBook.GetAllSpells();

        // Assert
        Assert.AreEqual(2, allSpells.Count);
        Assert.Contains(testSpell1, allSpells);
        Assert.Contains(testSpell2, allSpells);
    }

    [Test]
    public void GetAllSpells_EmptySpellBook_ReturnsEmptyList()
    {
        // Act
        var allSpells = spellBook.GetAllSpells();

        // Assert
        Assert.AreEqual(0, allSpells.Count);
    }

    [Test]
    public void GetAllSpells_SkipsNullSpells()
    {
        // Arrange
        var sequence = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence, spell = null });
        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence, spell = testSpell1 });

        // Act
        var allSpells = spellBook.GetAllSpells();

        // Assert
        Assert.AreEqual(1, allSpells.Count);
        Assert.Contains(testSpell1, allSpells);
    }

    [Test]
    public void GetSpellByName_FindsCorrectSpell()
    {
        // Arrange
        var sequence1 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence1, spell = testSpell1 });

        // Act
        var spell = spellBook.GetSpellByName("Fireball");

        // Assert
        Assert.AreEqual(testSpell1, spell);
    }

    [Test]
    public void GetSpellByName_NonExistentSpell_ReturnsNull()
    {
        // Arrange
        var sequence = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence, spell = testSpell1 });

        // Act
        var spell = spellBook.GetSpellByName("NonExistent");

        // Assert
        Assert.IsNull(spell);
    }

    [Test]
    public void GetEntries_ReturnsAllEntries()
    {
        // Arrange
        var sequence1 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var sequence2 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence1, spell = testSpell1 });
        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence2, spell = testSpell2 });

        // Act
        var entries = spellBook.GetEntries();

        // Assert
        Assert.AreEqual(2, entries.Count);
    }

    [Test]
    public void GetEntryByName_FindsCorrectEntry()
    {
        // Arrange
        var sequence = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var entry = new SpellBook.Entry { sequence = sequence, spell = testSpell1 };
        spellBook.entries.Add(entry);

        // Act
        var foundEntry = spellBook.GetEntryByName("Fireball");

        // Assert
        Assert.AreEqual(entry, foundEntry);
        Assert.AreEqual(testSpell1, foundEntry.spell);
    }

    [Test]
    public void GetEntryByName_NonExistentEntry_ReturnsNull()
    {
        // Arrange
        var sequence = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        spellBook.entries.Add(new SpellBook.Entry { sequence = sequence, spell = testSpell1 });

        // Act
        var entry = spellBook.GetEntryByName("NonExistent");

        // Assert
        Assert.IsNull(entry);
    }
}

