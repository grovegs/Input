using System.Numerics;

namespace GroveGames.Input;

public struct PointerDownEvent
{
    private readonly int _pointerId;
    private readonly Vector2 _position;
    private bool _isHandled;
    public int PointerId => _pointerId;
    public Vector2 Position => _position;

    public bool IsHandled => _isHandled;

    public PointerDownEvent(int pointerId, Vector2 position)
    {
        _pointerId = pointerId;
        _position = position;
        _isHandled = false;
    }

    public void MarkHandled()
    {
        _isHandled = true;
    }
}
