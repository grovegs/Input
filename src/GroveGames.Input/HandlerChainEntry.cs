namespace GroveGames.Input;

internal readonly struct HandlerChainEntry<T> where T : class
{
    private readonly T _handler;
    private readonly int _priority;
    public T Handler => _handler;
    public int Priority => _priority;

    public HandlerChainEntry(T handler, int priority)
    {
        _handler = handler;
        _priority = priority;
    }
}
