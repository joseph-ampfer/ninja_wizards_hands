using NUnit.Framework;
using System.Collections.Generic;

public class GestureSequenceTests
{
    [Test]
    public void Constructor_WithValidList_CreatesSequence()
    {
        var steps = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist),
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        var sequence = new GestureSequence(steps);

        Assert.AreEqual(2, sequence.Steps.Count);
        Assert.AreEqual(steps[0], sequence.Steps[0]);
        Assert.AreEqual(steps[1], sequence.Steps[1]);
    }

    [Test]
    public void Constructor_CreatesDeepCopy()
    {
        var steps = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var sequence = new GestureSequence(steps);
        steps.Add(new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp));

        // Sequence should not be affected by changes to original list
        Assert.AreEqual(1, sequence.Steps.Count);
    }

    [Test]
    public void Equals_SameSequences_ReturnsTrue()
    {
        var steps = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist),
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        var sequence1 = new GestureSequence(steps);
        var sequence2 = new GestureSequence(steps);

        Assert.IsTrue(sequence1.Equals(sequence2));
    }

    [Test]
    public void Equals_DifferentLength_ReturnsFalse()
    {
        var steps1 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var steps2 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist),
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        var sequence1 = new GestureSequence(steps1);
        var sequence2 = new GestureSequence(steps2);

        Assert.IsFalse(sequence1.Equals(sequence2));
    }

    [Test]
    public void Equals_DifferentOrder_ReturnsFalse()
    {
        var steps1 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist),
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        var steps2 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp),
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var sequence1 = new GestureSequence(steps1);
        var sequence2 = new GestureSequence(steps2);

        Assert.IsFalse(sequence1.Equals(sequence2));
    }

    [Test]
    public void Equals_DifferentSteps_ReturnsFalse()
    {
        var steps1 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var steps2 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        var sequence1 = new GestureSequence(steps1);
        var sequence2 = new GestureSequence(steps2);

        Assert.IsFalse(sequence1.Equals(sequence2));
    }

    [Test]
    public void Equals_EmptySequences_ReturnsTrue()
    {
        var sequence1 = new GestureSequence(new List<GesturePair>());
        var sequence2 = new GestureSequence(new List<GesturePair>());

        Assert.IsTrue(sequence1.Equals(sequence2));
    }

    [Test]
    public void GetHashCode_SameSequences_ReturnsSameHash()
    {
        var steps = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist),
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        var sequence1 = new GestureSequence(steps);
        var sequence2 = new GestureSequence(steps);

        Assert.AreEqual(sequence1.GetHashCode(), sequence2.GetHashCode());
    }

    [Test]
    public void GetHashCode_DifferentSequences_ReturnsDifferentHash()
    {
        var steps1 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var steps2 = new List<GesturePair>
        {
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        var sequence1 = new GestureSequence(steps1);
        var sequence2 = new GestureSequence(steps2);

        // Note: Hash collisions are possible but unlikely
        Assert.AreNotEqual(sequence1.GetHashCode(), sequence2.GetHashCode());
    }

    [Test]
    public void ToString_SingleStep_ReturnsFormattedString()
    {
        var steps = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        };

        var sequence = new GestureSequence(steps);
        string result = sequence.ToString();

        Assert.AreEqual("(Left:OpenPalm,Right:ClosedFist)", result);
    }

    [Test]
    public void ToString_MultipleSteps_ReturnsFormattedStringWithArrows()
    {
        var steps = new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist),
            new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp)
        };

        var sequence = new GestureSequence(steps);
        string result = sequence.ToString();

        Assert.AreEqual("(Left:OpenPalm,Right:ClosedFist) -> (Left:Victory,Right:ThumbsUp)", result);
    }

    [Test]
    public void EqualsObject_WithNull_ReturnsFalse()
    {
        var sequence = new GestureSequence(new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        });

        Assert.IsFalse(sequence.Equals(null));
    }

    [Test]
    public void EqualsObject_WithDifferentType_ReturnsFalse()
    {
        var sequence = new GestureSequence(new List<GesturePair>
        {
            new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist)
        });

        object other = "not a gesture sequence";

        Assert.IsFalse(sequence.Equals(other));
    }
}

