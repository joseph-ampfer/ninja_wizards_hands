using NUnit.Framework;

public class NetworkMatchPhaseTests
{
    [Test]
    public void EnumUnderlyingValues_MatchNetworkContract()
    {
        Assert.AreEqual(0, (byte)NetworkMatchPhase.WaitingForPlayers);
        Assert.AreEqual(1, (byte)NetworkMatchPhase.Prematch);
        Assert.AreEqual(2, (byte)NetworkMatchPhase.Fighting);
        Assert.AreEqual(3, (byte)NetworkMatchPhase.EndMatch);
        Assert.AreEqual(4, (byte)NetworkMatchPhase.Results);
    }
}
