namespace GroveGames.Input;

public interface IKeyboardDispatcher
{
    public void Register(IKeyDownHandler handler, int priority = 0);
    public void Unregister(IKeyDownHandler handler);
}
