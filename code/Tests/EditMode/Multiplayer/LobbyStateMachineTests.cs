using NUnit.Framework;

public class LobbyStateMachineTests
{
    private sealed class RecordingLobbyState : LobbyStateBase
    {
        private readonly LobbyStateId _id;

        public RecordingLobbyState(LobbyStateId id)
        {
            _id = id;
        }

        public override LobbyStateId Id => _id;

        public int EnterCount { get; private set; }
        public int ExitCount { get; private set; }

        public override void Enter(LobbyContext ctx) => EnterCount++;
        public override void Exit(LobbyContext ctx) => ExitCount++;
    }

    [Test]
    public void TransitionTo_FirstState_OnlyCallsEnter()
    {
        var ctx = new LobbyContext();
        var fsm = new LobbyStateMachine(ctx);
        var idle = new RecordingLobbyState(LobbyStateId.Idle);
        fsm.Register(idle);

        fsm.TransitionTo(LobbyStateId.Idle);

        Assert.AreSame(idle, fsm.Current);
        Assert.AreEqual(1, idle.EnterCount);
        Assert.AreEqual(0, idle.ExitCount);
    }

    [Test]
    public void TransitionTo_SecondState_CallsExitThenEnter()
    {
        var ctx = new LobbyContext();
        var fsm = new LobbyStateMachine(ctx);
        var idle = new RecordingLobbyState(LobbyStateId.Idle);
        var searching = new RecordingLobbyState(LobbyStateId.Searching);
        fsm.Register(idle);
        fsm.Register(searching);

        fsm.TransitionTo(LobbyStateId.Idle);
        fsm.TransitionTo(LobbyStateId.Searching);

        Assert.AreSame(searching, fsm.Current);
        Assert.AreEqual(1, idle.EnterCount);
        Assert.AreEqual(1, idle.ExitCount);
        Assert.AreEqual(1, searching.EnterCount);
        Assert.AreEqual(0, searching.ExitCount);
    }

    [Test]
    public void TransitionTo_UnregisteredId_DoesNotChangeCurrentOrCallExit()
    {
        var ctx = new LobbyContext();
        var fsm = new LobbyStateMachine(ctx);
        var idle = new RecordingLobbyState(LobbyStateId.Idle);
        fsm.Register(idle);

        fsm.TransitionTo(LobbyStateId.Idle);
        var unknown = (LobbyStateId)255;
        fsm.TransitionTo(unknown);

        Assert.AreSame(idle, fsm.Current);
        Assert.AreEqual(1, idle.EnterCount);
        Assert.AreEqual(0, idle.ExitCount);
    }
}
