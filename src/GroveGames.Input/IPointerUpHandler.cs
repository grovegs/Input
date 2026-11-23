namespace GroveGames.Input;

public interface IPointerUpHandler : IInputHandler
{
    public void HandlePointerUp(in PointerUpEvent pointerEvent);
}
