namespace GroveGames.Input;

public interface IPointerDownHandler : IInputHandler
{
    public void HandlePointerDown(ref PointerDownEvent pointerEvent);
}
