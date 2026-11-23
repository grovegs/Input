namespace GroveGames.Input;

public interface IPointerMoveHandler : IInputHandler
{
    public void HandlePointerMove(in PointerMoveEvent pointerEvent);
}
