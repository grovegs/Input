namespace GroveGames.Input;

public sealed class KeyboardDispatcher : IKeyboardDispatcher
{
    private const int KeyCount = (int)Key.F12 + 1;

    private readonly HandlerChain<IKeyDownHandler> _downChain;
    private readonly IKeyUpHandler?[] _owners;
    private int _ownedCount;

    public bool IsIdle => _downChain.Count == 0 && _ownedCount == 0;

    public KeyboardDispatcher()
    {
        _downChain = new HandlerChain<IKeyDownHandler>();
        _owners = new IKeyUpHandler?[KeyCount];
    }

    public void Register(IKeyDownHandler handler, int priority = 0)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _downChain.Add(handler, priority);
    }

    public void Unregister(IKeyDownHandler handler)
    {
        if (handler == null)
        {
            return;
        }

        _downChain.Remove(handler);

        for (var i = 0; i < KeyCount && _ownedCount > 0; i++)
        {
            if (ReferenceEquals(_owners[i], handler))
            {
                ReleaseKey(i);
            }
        }
    }

    public void Press(Key key)
    {
        var index = (int)key;

        if (key == Key.None || (uint)index >= KeyCount)
        {
            return;
        }

        ReleaseKey(index);
        var entries = _downChain.Entries;
        var downEvent = new KeyDownEvent(key);

        for (var i = 0; i < entries.Length; i++)
        {
            var handler = entries[i].Handler;
            handler.HandleKeyDown(ref downEvent);

            if (downEvent.IsHandled)
            {
                if (handler is IKeyUpHandler upHandler)
                {
                    _owners[index] = upHandler;
                    _ownedCount++;
                }

                return;
            }
        }
    }

    public void Release(Key key)
    {
        ReleaseKey((int)key);
    }

    public void ReleaseAll()
    {
        for (var i = 0; i < KeyCount && _ownedCount > 0; i++)
        {
            ReleaseKey(i);
        }
    }

    private void ReleaseKey(int index)
    {
        if ((uint)index >= KeyCount)
        {
            return;
        }

        var owner = _owners[index];

        if (owner == null)
        {
            return;
        }

        _owners[index] = null;
        _ownedCount--;
        var upEvent = new KeyUpEvent((Key)index);
        owner.HandleKeyUp(in upEvent);
    }
}
