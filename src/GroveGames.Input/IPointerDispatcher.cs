namespace GroveGames.Input;

public interface IPointerDispatcher
{
    public void Register(IPointerDownHandler handler, int priority = 0);
    public void Unregister(IPointerDownHandler handler);
}
