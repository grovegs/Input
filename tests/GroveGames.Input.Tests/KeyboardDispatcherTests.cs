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
        dispatcher.Register(new TestHandler("only", false, new List<string>()));

        Assert.False(dispatcher.IsIdle);
    }

    [Fact]
    public void Press_HandlersWithPriorities_AsksTheHighestFirst()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new TestHandler("low", false, calls), 0);
        dispatcher.Register(new TestHandler("high", false, calls), 10);

        dispatcher.Press(Key.Escape);

        Assert.Equal(new[] { "high.Down.Escape", "low.Down.Escape" }, calls);
    }

    [Fact]
    public void Press_HandlerMarksHandled_StopsThere()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new TestHandler("low", false, calls), 0);
        dispatcher.Register(new TestHandler("high", true, calls), 10);

        dispatcher.Press(Key.Space);

        Assert.Equal(new[] { "high.Down.Space" }, calls);
    }

    [Fact]
    public void Press_NoKey_IsSilent()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new TestHandler("only", true, calls));

        dispatcher.Press(Key.None);

        Assert.Empty(calls);
    }

    [Fact]
    public void Release_AfterHandledPress_ReachesTheOwnerOnce()
    {
        var calls = new List<string>();
        var dispatcher = new KeyboardDispatcher();
        dispatcher.Register(new TestHandler("other", true, calls), 0);
        dispatcher.Register(new TestHandler("owner", true, calls), 10);
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
        dispatcher.Register(new TestHandler("watcher", false, calls));
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
        dispatcher.Register(new TestHandler("owner", true, calls));
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
        dispatcher.Register(new TestHandler("owner", true, calls));
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
        dispatcher.Register(new TestHandler("owner", true, calls));
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
        var owner = new TestHandler("owner", true, calls);
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
        var handler = new TestDownHandler();
        dispatcher.Register(handler);

        dispatcher.Press(Key.A);
        dispatcher.Unregister(handler);

        Assert.True(dispatcher.IsIdle);
    }

    private sealed class TestDownHandler : IKeyDownHandler
    {
        public void HandleKeyDown(ref KeyDownEvent keyEvent)
        {
            keyEvent.MarkHandled();
        }
    }

    private sealed class TestHandler : IKeyDownHandler, IKeyUpHandler
    {
        private readonly string _name;
        private readonly bool _handles;
        private readonly List<string> _calls;

        public TestHandler(string name, bool handles, List<string> calls)
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
