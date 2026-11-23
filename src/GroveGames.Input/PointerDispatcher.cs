using System.Numerics;

namespace GroveGames.Input;

public sealed class PointerDispatcher : IPointerDispatcher
{
    private readonly HandlerChain<IPointerDownHandler> _downChain;
    private IPointerDownHandler? _owner;
    private IPointerMoveHandler? _moveOwner;
    private IPointerUpHandler? _upOwner;
    private IPointerCancelHandler? _cancelOwner;
    private int _pointerId;
    private Vector2 _position;
    private bool _isDown;

    public PointerDispatcher()
    {
        _downChain = new HandlerChain<IPointerDownHandler>();
    }

    public void Register(IPointerDownHandler handler, int priority = 0)
    {
        ArgumentNullException.ThrowIfNull(handler);

        _downChain.Add(handler, priority);
    }

    public void Unregister(IPointerDownHandler handler)
    {
        if (handler == null)
        {
            return;
        }

        if (ReferenceEquals(_owner, handler))
        {
            Cancel();
        }

        _downChain.Remove(handler);
    }

    public void Press(int pointerId, Vector2 position)
    {
        Cancel();
        _isDown = true;
        _pointerId = pointerId;
        _position = position;
        var entries = _downChain.Entries;
        var downEvent = new PointerDownEvent(pointerId, position);

        for (var i = 0; i < entries.Length; i++)
        {
            var handler = entries[i].Handler;
            handler.HandlePointerDown(ref downEvent);

            if (downEvent.IsHandled)
            {
                _owner = handler;
                _moveOwner = handler as IPointerMoveHandler;
                _upOwner = handler as IPointerUpHandler;
                _cancelOwner = handler as IPointerCancelHandler;
                return;
            }
        }
    }

    public void Move(Vector2 position)
    {
        if (!_isDown || position == _position)
        {
            return;
        }

        var delta = position - _position;
        _position = position;

        if (_moveOwner != null)
        {
            var moveEvent = new PointerMoveEvent(_pointerId, position, delta);
            _moveOwner.HandlePointerMove(in moveEvent);
        }
    }

    public void Release(Vector2 position)
    {
        if (!_isDown)
        {
            return;
        }

        var upOwner = _upOwner;
        Clear();

        if (upOwner != null)
        {
            var upEvent = new PointerUpEvent(_pointerId, position);
            upOwner.HandlePointerUp(in upEvent);
        }
    }

    public void Cancel()
    {
        if (!_isDown)
        {
            return;
        }

        var cancelOwner = _cancelOwner;
        Clear();

        if (cancelOwner != null)
        {
            var cancelEvent = new PointerCancelEvent(_pointerId);
            cancelOwner.HandlePointerCancel(in cancelEvent);
        }
    }

    private void Clear()
    {
        _isDown = false;
        _owner = null;
        _moveOwner = null;
        _upOwner = null;
        _cancelOwner = null;
    }
}
