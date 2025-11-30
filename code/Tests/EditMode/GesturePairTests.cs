using NUnit.Framework;

public class GesturePairTests
{
    [Test]
    public void Constructor_SetsLeftAndRight()
    {
        var pair = new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist);
        
        Assert.AreEqual(GestureLabel.OpenPalm, pair.Left);
        Assert.AreEqual(GestureLabel.ClosedFist, pair.Right);
    }

    [Test]
    public void Equals_SamePairs_ReturnsTrue()
    {
        var pair1 = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        var pair2 = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        
        Assert.IsTrue(pair1.Equals(pair2));
        Assert.IsTrue(pair2.Equals(pair1));
    }

    [Test]
    public void Equals_DifferentPairs_ReturnsFalse()
    {
        var pair1 = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        var pair2 = new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist);
        
        Assert.IsFalse(pair1.Equals(pair2));
        Assert.IsFalse(pair2.Equals(pair1));
    }

    [Test]
    public void Equals_SwappedLeftRight_ReturnsFalse()
    {
        var pair1 = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        var pair2 = new GesturePair(GestureLabel.ThumbsUp, GestureLabel.Victory);
        
        Assert.IsFalse(pair1.Equals(pair2));
    }

    [Test]
    public void EqualsOperator_SamePairs_ReturnsTrue()
    {
        var pair1 = new GesturePair(GestureLabel.PointingUp, GestureLabel.ILoveYou);
        var pair2 = new GesturePair(GestureLabel.PointingUp, GestureLabel.ILoveYou);
        
        Assert.IsTrue(pair1 == pair2);
    }

    [Test]
    public void NotEqualsOperator_DifferentPairs_ReturnsTrue()
    {
        var pair1 = new GesturePair(GestureLabel.PointingUp, GestureLabel.ILoveYou);
        var pair2 = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        
        Assert.IsTrue(pair1 != pair2);
    }

    [Test]
    public void GetHashCode_SamePairs_ReturnsSameHash()
    {
        var pair1 = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        var pair2 = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        
        Assert.AreEqual(pair1.GetHashCode(), pair2.GetHashCode());
    }

    [Test]
    public void GetHashCode_DifferentPairs_ReturnsDifferentHash()
    {
        var pair1 = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        var pair2 = new GesturePair(GestureLabel.OpenPalm, GestureLabel.ClosedFist);
        
        // Note: Hash collisions are possible but unlikely for different pairs
        Assert.AreNotEqual(pair1.GetHashCode(), pair2.GetHashCode());
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        var pair = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        string result = pair.ToString();
        
        Assert.AreEqual("(Left:Victory,Right:ThumbsUp)", result);
    }

    [Test]
    public void ToString_WithNone_IncludesNone()
    {
        var pair = new GesturePair(GestureLabel.None, GestureLabel.OpenPalm);
        string result = pair.ToString();
        
        Assert.AreEqual("(Left:None,Right:OpenPalm)", result);
    }

    [Test]
    public void EqualsObject_WithNull_ReturnsFalse()
    {
        var pair = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        
        Assert.IsFalse(pair.Equals(null));
    }

    [Test]
    public void EqualsObject_WithDifferentType_ReturnsFalse()
    {
        var pair = new GesturePair(GestureLabel.Victory, GestureLabel.ThumbsUp);
        object other = "not a gesture pair";
        
        Assert.IsFalse(pair.Equals(other));
    }
}

