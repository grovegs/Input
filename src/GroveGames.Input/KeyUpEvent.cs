namespace GroveGames.Input;

public readonly struct KeyUpEvent
{
    private readonly Key _key;
    public Key Key => _key;

    public KeyUpEvent(Key key)
    {
        _key = key;
    }
}
