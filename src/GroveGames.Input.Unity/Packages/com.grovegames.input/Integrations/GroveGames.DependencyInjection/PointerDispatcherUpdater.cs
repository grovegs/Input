using GroveGames.DependencyInjection;

namespace GroveGames.Input.Unity
{
    internal sealed class PointerDispatcherUpdater : IPreFrameUpdatable
    {
        private readonly UnityPointerDispatcher _target;

        public PointerDispatcherUpdater(UnityPointerDispatcher target)
        {
            _target = target;
        }

        public void PreFrameUpdate(DeltaTime deltaTime)
        {
            _target.Update();
        }
    }
}
