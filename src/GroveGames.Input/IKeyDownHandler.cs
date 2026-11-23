namespace GroveGames.Input;

public interface IKeyDownHandler : IInputHandler
{
    public void HandleKeyDown(ref KeyDownEvent keyEvent);
}
