using System.Numerics;

namespace GroveGames.Input.Tests;

public sealed class PointerDispatcherTests
{
    private static readonly Vector2 s_start = new(10f, 20f);
    private static readonly Vector2 s_next = new(13f, 24f);

    [Fact]
    public void Register_NullHandler_ThrowsArgumentNullException()
    {
        var dispatcher = new PointerDispatcher();

        Assert.Throws<ArgumentNullException>(() => dispatcher.Register(null!));
    }

    [Fact]
    public void Press_HandlersWithPriorities_AsksTheHighestFirst()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        dispatcher.Register(new FakeHandler("low", false, calls), 0);
        dispatcher.Register(new FakeHandler("high", false, calls), 10);
        dispatcher.Register(new FakeHandler("middle", false, calls), 5);

        dispatcher.Press(1, s_start);

        Assert.Equal(new[] { "high.Down", "middle.Down", "low.Down" }, calls);
    }

    [Fact]
    public void Press_SamePriority_AsksInRegistrationOrder()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        dispatcher.Register(new FakeHandler("first", false, calls));
        dispatcher.Register(new FakeHandler("second", false, calls));

        dispatcher.Press(1, s_start);

        Assert.Equal(new[] { "first.Down", "second.Down" }, calls);
    }

    [Fact]
    public void Press_HandlerMarksHandled_StopsThere()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        dispatcher.Register(new FakeHandler("low", false, calls), 0);
        dispatcher.Register(new FakeHandler("high", true, calls), 10);

        dispatcher.Press(1, s_start);

        Assert.Equal(new[] { "high.Down" }, calls);
    }

    [Fact]
    public void Register_SameHandlerTwice_AsksItOnce()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        var handler = new FakeHandler("only", false, calls);
        dispatcher.Register(handler);
        dispatcher.Register(handler, 10);

        dispatcher.Press(1, s_start);

        Assert.Equal(new[] { "only.Down" }, calls);
    }

    [Fact]
    public void Press_Event_CarriesPointerAndPosition()
    {
        var dispatcher = new PointerDispatcher();
        var handler = new FakeHandler("owner", true, new List<string>());
        dispatcher.Register(handler);

        dispatcher.Press(7, s_start);

        Assert.Equal(7, handler.PointerId);
        Assert.Equal(s_start, handler.Position);
    }

    [Fact]
    public void Move_AfterHandledPress_ReachesOnlyTheOwnerWithDelta()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        var owner = new FakeHandler("owner", true, calls);
        dispatcher.Register(new FakeHandler("other", true, calls), 0);
        dispatcher.Register(owner, 10);
        dispatcher.Press(1, s_start);
        calls.Clear();

        dispatcher.Move(s_next);

        Assert.Equal(new[] { "owner.Move" }, calls);
        Assert.Equal(s_next, owner.Position);
        Assert.Equal(new Vector2(3f, 4f), owner.Delta);
    }

    [Fact]
    public void Move_SamePosition_IsSilent()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        dispatcher.Register(new FakeHandler("owner", true, calls));
        dispatcher.Press(1, s_start);
        calls.Clear();

        dispatcher.Move(s_start);

        Assert.Empty(calls);
    }

    [Fact]
    public void Move_WithoutPress_IsSilent()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        dispatcher.Register(new FakeHandler("owner", true, calls));

        dispatcher.Move(s_next);

        Assert.Empty(calls);
    }

    [Fact]
    public void Release_AfterHandledPress_ReachesTheOwnerOnce()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        var owner = new FakeHandler("owner", true, calls);
        dispatcher.Register(owner);
        dispatcher.Press(4, s_start);
        calls.Clear();

        dispatcher.Release(s_next);
        dispatcher.Release(s_next);
        dispatcher.Move(s_start);

        Assert.Equal(new[] { "owner.Up" }, calls);
        Assert.Equal(4, owner.PointerId);
        Assert.Equal(s_next, owner.Position);
    }

    [Fact]
    public void Release_NobodyHandledThePress_IsSilent()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        dispatcher.Register(new FakeHandler("watcher", false, calls));
        dispatcher.Press(1, s_start);
        calls.Clear();

        dispatcher.Move(s_next);
        dispatcher.Release(s_next);

        Assert.Empty(calls);
    }

    [Fact]
    public void Cancel_AfterHandledPress_ReachesTheOwnerOnce()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        dispatcher.Register(new FakeHandler("owner", true, calls));
        dispatcher.Press(1, s_start);
        calls.Clear();

        dispatcher.Cancel();
        dispatcher.Cancel();
        dispatcher.Release(s_next);

        Assert.Equal(new[] { "owner.Cancel" }, calls);
    }

    [Fact]
    public void Press_WhileAnotherPressIsDown_CancelsTheFirst()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        dispatcher.Register(new FakeHandler("owner", true, calls));
        dispatcher.Press(1, s_start);
        calls.Clear();

        dispatcher.Press(2, s_next);

        Assert.Equal(new[] { "owner.Cancel", "owner.Down" }, calls);
    }

    [Fact]
    public void Unregister_Owner_CancelsItAndStopsAskingIt()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        var owner = new FakeHandler("owner", true, calls);
        dispatcher.Register(owner);
        dispatcher.Press(1, s_start);
        calls.Clear();

        dispatcher.Unregister(owner);
        dispatcher.Press(1, s_start);

        Assert.Equal(new[] { "owner.Cancel" }, calls);
    }

    [Fact]
    public void Unregister_NotTheOwner_KeepsThePress()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        var other = new FakeHandler("other", false, calls);
        dispatcher.Register(new FakeHandler("owner", true, calls), 10);
        dispatcher.Register(other, 0);
        dispatcher.Press(1, s_start);
        calls.Clear();

        dispatcher.Unregister(other);
        dispatcher.Move(s_next);

        Assert.Equal(new[] { "owner.Move" }, calls);
    }

    [Fact]
    public void Move_OwnerHandlesOnlyDown_IsSilent()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        dispatcher.Register(new FakeDownHandler(calls));
        dispatcher.Press(1, s_start);
        calls.Clear();

        dispatcher.Move(s_next);
        dispatcher.Release(s_next);
        dispatcher.Cancel();

        Assert.Empty(calls);
    }

    [Fact]
    public void Press_HandlerUnregistersItselfWhileHandling_GetsNoLaterEvents()
    {
        var calls = new List<string>();
        var dispatcher = new PointerDispatcher();
        dispatcher.Register(new FakeSelfUnregisteringHandler(dispatcher, calls));

        dispatcher.Press(1, s_start);
        dispatcher.Move(s_next);
        dispatcher.Release(s_next);

        Assert.Equal(new[] { "Down" }, calls);
    }

    private sealed class FakeDownHandler : IPointerDownHandler
    {
        private readonly List<string> _calls;

        public FakeDownHandler(List<string> calls)
        {
            _calls = calls;
        }

        public void HandlePointerDown(ref PointerDownEvent pointerEvent)
        {
            _calls.Add("Down");
            pointerEvent.MarkHandled();
        }
    }

    private sealed class FakeSelfUnregisteringHandler : IPointerDownHandler, IPointerMoveHandler, IPointerUpHandler, IPointerCancelHandler
    {
        private readonly PointerDispatcher _dispatcher;
        private readonly List<string> _calls;

        public FakeSelfUnregisteringHandler(PointerDispatcher dispatcher, List<string> calls)
        {
            _dispatcher = dispatcher;
            _calls = calls;
        }

        public void HandlePointerDown(ref PointerDownEvent pointerEvent)
        {
            _calls.Add("Down");
            pointerEvent.MarkHandled();
            _dispatcher.Unregister(this);
        }

        public void HandlePointerMove(in PointerMoveEvent pointerEvent)
        {
            _calls.Add("Move");
        }

        public void HandlePointerUp(in PointerUpEvent pointerEvent)
        {
            _calls.Add("Up");
        }

        public void HandlePointerCancel(in PointerCancelEvent pointerEvent)
        {
            _calls.Add("Cancel");
        }
    }

    private sealed class FakeHandler : IPointerDownHandler, IPointerMoveHandler, IPointerUpHandler, IPointerCancelHandler
    {
        private readonly string _name;
        private readonly bool _handles;
        private readonly List<string> _calls;
        private int _pointerId;
        private Vector2 _position;
        private Vector2 _delta;

        public int PointerId => _pointerId;
        public Vector2 Position => _position;
        public Vector2 Delta => _delta;

        public FakeHandler(string name, bool handles, List<string> calls)
        {
            _name = name;
            _handles = handles;
            _calls = calls;
        }

        public void HandlePointerDown(ref PointerDownEvent pointerEvent)
        {
            _calls.Add($"{_name}.Down");
            _pointerId = pointerEvent.PointerId;
            _position = pointerEvent.Position;

            if (_handles)
            {
                pointerEvent.MarkHandled();
            }
        }

        public void HandlePointerMove(in PointerMoveEvent pointerEvent)
        {
            _calls.Add($"{_name}.Move");
            _position = pointerEvent.Position;
            _delta = pointerEvent.Delta;
        }

        public void HandlePointerUp(in PointerUpEvent pointerEvent)
        {
            _calls.Add($"{_name}.Up");
            _pointerId = pointerEvent.PointerId;
            _position = pointerEvent.Position;
        }

        public void HandlePointerCancel(in PointerCancelEvent pointerEvent)
        {
            _calls.Add($"{_name}.Cancel");
        }
    }
}
