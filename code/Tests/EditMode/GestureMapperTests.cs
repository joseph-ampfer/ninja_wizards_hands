using NUnit.Framework;

public class GestureMapperTests
{
    [Test]
    public void ToEnum_ValidGestureName_ReturnsCorrectEnum()
    {
        // Test all valid gesture mappings
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("None"));
        Assert.AreEqual(GestureLabel.ClosedFist, GestureMapper.ToEnum("Closed_Fist"));
        Assert.AreEqual(GestureLabel.OpenPalm, GestureMapper.ToEnum("Open_Palm"));
        Assert.AreEqual(GestureLabel.PointingUp, GestureMapper.ToEnum("Pointing_Up"));
        Assert.AreEqual(GestureLabel.ThumbsDown, GestureMapper.ToEnum("Thumb_Down"));
        Assert.AreEqual(GestureLabel.ThumbsUp, GestureMapper.ToEnum("Thumb_Up"));
        Assert.AreEqual(GestureLabel.Victory, GestureMapper.ToEnum("Victory"));
        Assert.AreEqual(GestureLabel.ILoveYou, GestureMapper.ToEnum("ILoveYou"));
    }

    [Test]
    public void ToEnum_InvalidGestureName_ReturnsNone()
    {
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("InvalidGesture"));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("RandomString"));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum(""));
    }

    [Test]
    public void ToEnum_NullGestureName_ReturnsNone()
    {
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum(null));
    }

    [Test]
    public void ToEnum_CaseSensitive_WrongCaseReturnsNone()
    {
        // The mapper is case-sensitive
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("closed_fist"));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("CLOSED_FIST"));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("victory"));
    }

    [Test]
    public void ToEnum_WhitespaceString_ReturnsNone()
    {
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("   "));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("\t"));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("\n"));
    }

    [Test]
    public void ToEnum_PartialMatch_ReturnsNone()
    {
        // Partial matches should not work
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("Closed"));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("Fist"));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("Open"));
    }

    [Test]
    public void ToEnum_ExtraWhitespace_ReturnsNone()
    {
        // Extra whitespace should not match
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum(" Closed_Fist"));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("Closed_Fist "));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum(" Victory "));
    }

    [Test]
    public void ToEnum_AllEnumValues_HaveMappings()
    {
        // Ensure all enum values can be converted from string
        // This test documents expected behavior
        var expectedMappings = new System.Collections.Generic.Dictionary<string, GestureLabel>
        {
            { "None", GestureLabel.None },
            { "Closed_Fist", GestureLabel.ClosedFist },
            { "Open_Palm", GestureLabel.OpenPalm },
            { "Pointing_Up", GestureLabel.PointingUp },
            { "Thumb_Down", GestureLabel.ThumbsDown },
            { "Thumb_Up", GestureLabel.ThumbsUp },
            { "Victory", GestureLabel.Victory },
            { "ILoveYou", GestureLabel.ILoveYou }
        };

        foreach (var kvp in expectedMappings)
        {
            Assert.AreEqual(kvp.Value, GestureMapper.ToEnum(kvp.Key), 
                $"Mapping for {kvp.Key} should return {kvp.Value}");
        }
    }

    [Test]
    public void ToEnum_SpecialCharacters_ReturnsNone()
    {
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("@#$%"));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("!Victory!"));
    }

    [Test]
    public void ToEnum_NumericString_ReturnsNone()
    {
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("123"));
        Assert.AreEqual(GestureLabel.None, GestureMapper.ToEnum("0"));
    }
}

