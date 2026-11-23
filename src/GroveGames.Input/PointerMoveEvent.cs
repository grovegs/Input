using System.Numerics;

namespace GroveGames.Input;

public readonly struct PointerMoveEvent
{
    private readonly int _pointerId;
    private readonly Vector2 _position;
    private readonly Vector2 _delta;
    public int PointerId => _pointerId;
    public Vector2 Position => _position;
    public Vector2 Delta => _delta;

    public PointerMoveEvent(int pointerId, Vector2 position, Vector2 delta)
    {
        _pointerId = pointerId;
        _position = position;
        _delta = delta;
    }
}
