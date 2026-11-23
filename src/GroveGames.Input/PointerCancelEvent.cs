namespace GroveGames.Input;

public readonly struct PointerCancelEvent
{
    private readonly int _pointerId;
    public int PointerId => _pointerId;

    public PointerCancelEvent(int pointerId)
    {
        _pointerId = pointerId;
    }
}
