using NUnit.Framework;

public class ConstantsTests
{
    [Test]
    public void GameModeKey_IsStable()
    {
        Assert.AreEqual("gameMode", Constants.GAME_MODE_KEY);
    }

    [Test]
    public void GameModeValues_AreExpectedStrings()
    {
        Assert.AreEqual("1v1", Constants.GAME_MODE_1V1);
        Assert.AreEqual("2v2", Constants.GAME_MODE_2V2);
        Assert.AreEqual("3v3", Constants.GAME_MODE_3V3);
        Assert.AreEqual("4v4", Constants.GAME_MODE_4V4);
        Assert.AreEqual("5v5", Constants.GAME_MODE_5V5);
    }

    [Test]
    public void CombatRadius_IsPositive()
    {
        Assert.AreEqual(13f, Constants.COMBAT_RADIUS);
    }
}
