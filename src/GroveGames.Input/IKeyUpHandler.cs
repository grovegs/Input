namespace GroveGames.Input;

public interface IKeyUpHandler : IInputHandler
{
    public void HandleKeyUp(in KeyUpEvent keyEvent);
}
