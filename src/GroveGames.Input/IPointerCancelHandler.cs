namespace GroveGames.Input;

public interface IPointerCancelHandler : IInputHandler
{
    public void HandlePointerCancel(in PointerCancelEvent pointerEvent);
}
