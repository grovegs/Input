namespace GroveGames.Input;

public struct KeyDownEvent
{
    private readonly Key _key;
    private bool _isHandled;
    public Key Key => _key;

    public bool IsHandled => _isHandled;

    public KeyDownEvent(Key key)
    {
        _key = key;
        _isHandled = false;
    }

    public void MarkHandled()
    {
        _isHandled = true;
    }
}
