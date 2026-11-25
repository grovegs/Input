using GroveGames.DependencyInjection;

namespace GroveGames.Input.Unity
{
    internal sealed class KeyboardDispatcherUpdater : IPreFrameUpdatable
    {
        private readonly UnityKeyboardDispatcher _target;

        public KeyboardDispatcherUpdater(UnityKeyboardDispatcher target)
        {
            _target = target;
        }

        public void PreFrameUpdate(DeltaTime deltaTime)
        {
            _target.Update();
        }
    }
}
