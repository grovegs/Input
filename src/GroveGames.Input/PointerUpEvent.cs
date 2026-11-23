using System.Numerics;

namespace GroveGames.Input;

public readonly struct PointerUpEvent
{
    private readonly int _pointerId;
    private readonly Vector2 _position;
    public int PointerId => _pointerId;
    public Vector2 Position => _position;

    public PointerUpEvent(int pointerId, Vector2 position)
    {
        _pointerId = pointerId;
        _position = position;
    }
}
