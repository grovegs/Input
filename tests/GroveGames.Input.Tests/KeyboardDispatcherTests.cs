namespace GroveGames.Input.Tests;

public sealed class KeyboardDispatcherTests
{
    [Fact]
    public void Register_NullHandler_ThrowsArgumentNullException()
    {
        var dispatcher = new KeyboardDispatcher();

        Assert.Throws<ArgumentNullException>(() => dispatcher.Register(null!));
    }

    [Fact]
    public void IsIdle_NoHandlers_IsTrue()
    {
        var dispatcher = new KeyboardDispatcher();

        Assert.True(dispatcher.IsIdle);
    }

    [Fact]
    public void IsIdle_HandlerRegistered_IsFalse()
    {
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new FakeHandler("only", false, new List<string>()));

        Assert.False(dispatcher.IsIdle);
    }

    [Fact]
    public void Press_HandlersWithPriorities_AsksTheHighestFirst()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new FakeHandler("low", false, calls), 0);
        dispatcher.Register(new FakeHandler("high", false, calls), 10);

        dispatcher.Press(Key.Escape);

        Assert.Equal(new[] { "high.Down.Escape", "low.Down.Escape" }, calls);
    }

    [Fact]
    public void Press_HandlerMarksHandled_StopsThere()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new FakeHandler("low", false, calls), 0);
        dispatcher.Register(new FakeHandler("high", true, calls), 10);

        dispatcher.Press(Key.Space);

        Assert.Equal(new[] { "high.Down.Space" }, calls);
    }

    [Fact]
    public void Press_NoKey_IsSilent()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new FakeHandler("only", true, calls));

        dispatcher.Press(Key.None);

        Assert.Empty(calls);
    }

    [Fact]
    public void Release_AfterHandledPress_ReachesTheOwnerOnce()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new FakeHandler("other", true, calls), 0);
        dispatcher.Register(new FakeHandler("owner", true, calls), 10);
        dispatcher.Press(Key.A);
        calls.Clear();

        dispatcher.Release(Key.A);
        dispatcher.Release(Key.A);

        Assert.Equal(new[] { "owner.Up.A" }, calls);
    }

    [Fact]
    public void Release_NobodyHandledThePress_IsSilent()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new FakeHandler("watcher", false, calls));
        dispatcher.Press(Key.A);
        calls.Clear();

        dispatcher.Release(Key.A);

        Assert.Empty(calls);
    }

    [Fact]
    public void Release_AnotherKey_KeepsTheOwner()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new FakeHandler("owner", true, calls));
        dispatcher.Press(Key.A);
        calls.Clear();

        dispatcher.Release(Key.B);
        dispatcher.Release(Key.A);

        Assert.Equal(new[] { "owner.Up.A" }, calls);
    }

    [Fact]
    public void Press_KeyAlreadyOwned_ReleasesItFirst()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new FakeHandler("owner", true, calls));
        dispatcher.Press(Key.A);
        calls.Clear();

        dispatcher.Press(Key.A);

        Assert.Equal(new[] { "owner.Up.A", "owner.Down.A" }, calls);
    }

    [Fact]
    public void ReleaseAll_SeveralKeysOwned_ReleasesEachOnce()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new FakeHandler("owner", true, calls));
        dispatcher.Press(Key.A);
        dispatcher.Press(Key.F12);
        calls.Clear();

        dispatcher.ReleaseAll();
        dispatcher.ReleaseAll();

        Assert.Equal(new[] { "owner.Up.A", "owner.Up.F12" }, calls);
    }

    [Fact]
    public void Unregister_Owner_ReleasesItsKeysAndStopsAskingIt()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        var owner = new FakeHandler("owner", true, calls);
        dispatcher.Register(owner);
        dispatcher.Press(Key.A);
        calls.Clear();

        dispatcher.Unregister(owner);
        dispatcher.Press(Key.A);

        Assert.Equal(new[] { "owner.Up.A" }, calls);
        Assert.True(dispatcher.IsIdle);
    }

    [Fact]
    public void Press_OwnerHandlesOnlyDown_KeepsNoOwner()
    {
        var dispatcher = new KeyboardDispatcher();
        var handler = new FakeDownHandler();
        dispatcher.Register(handler);

        dispatcher.Press(Key.A);
        dispatcher.Unregister(handler);

        Assert.True(dispatcher.IsIdle);
    }

    [Fact]
    public void Press_HandlerUnregistersItselfWhileHandling_GetsNoKeyUp()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new FakeSelfUnregisteringHandler(dispatcher, calls));

        dispatcher.Press(Key.A);
        var isIdle = dispatcher.IsIdle;
        dispatcher.Release(Key.A);

        Assert.True(isIdle);
        Assert.Equal(new[] { "Down.A" }, calls);
    }

    [Fact]
    public void Press_HandlerPressesTheSameKeyWhileHandling_BecomesIdleAfterUnregister()
    {
        var dispatcher = new KeyboardDispatcher();
        var handler = new FakeNestedPressHandler(dispatcher);
        dispatcher.Register(handler);
        dispatcher.Press(Key.A);

        dispatcher.Unregister(handler);

        Assert.True(dispatcher.IsIdle);
    }

    private sealed class FakeDownHandler : IKeyDownHandler
    {
        public void HandleKeyDown(ref KeyDownEvent keyEvent)
        {
            keyEvent.MarkHandled();
        }
    }

    private sealed class FakeSelfUnregisteringHandler : IKeyDownHandler, IKeyUpHandler
    {
        private readonly KeyboardDispatcher _dispatcher;
        private readonly List<string> _calls;

        public FakeSelfUnregisteringHandler(KeyboardDispatcher dispatcher, List<string> calls)
        {
            _dispatcher = dispatcher;
            _calls = calls;
        }

        public void HandleKeyDown(ref KeyDownEvent keyEvent)
        {
            _calls.Add($"Down.{keyEvent.Key}");
            keyEvent.MarkHandled();
            _dispatcher.Unregister(this);
        }

        public void HandleKeyUp(in KeyUpEvent keyEvent)
        {
            _calls.Add($"Up.{keyEvent.Key}");
        }
    }

    private sealed class FakeNestedPressHandler : IKeyDownHandler, IKeyUpHandler
    {
        private readonly KeyboardDispatcher _dispatcher;
        private bool _isPressing;

        public FakeNestedPressHandler(KeyboardDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _isPressing = false;
        }

        public void HandleKeyDown(ref KeyDownEvent keyEvent)
        {
            keyEvent.MarkHandled();

            if (_isPressing)
            {
                return;
            }

            _isPressing = true;
            _dispatcher.Press(keyEvent.Key);
            _isPressing = false;
        }

        public void HandleKeyUp(in KeyUpEvent keyEvent)
        {
        }
    }

    private sealed class FakeHandler : IKeyDownHandler, IKeyUpHandler
    {
        private readonly string _name;
        private readonly bool _handles;
        private readonly List<string> _calls;

        public FakeHandler(string name, bool handles, List<string> calls)
        {
            _name = name;
            _handles = handles;
            _calls = calls;
        }

        public void HandleKeyDown(ref KeyDownEvent keyEvent)
        {
            _calls.Add($"{_name}.Down.{keyEvent.Key}");

            if (_handles)
            {
                keyEvent.MarkHandled();
            }
        }

        public void HandleKeyUp(in KeyUpEvent keyEvent)
        {
            _calls.Add($"{_name}.Up.{keyEvent.Key}");
        }
    }
}
