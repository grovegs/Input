using UnityEngine.InputSystem;

namespace GroveGames.Input.Unity
{
    public sealed class UnityPointerDispatcher : IPointerDispatcher
    {
        private readonly PointerDispatcher _dispatcher;

        public UnityPointerDispatcher()
        {
            _dispatcher = new PointerDispatcher();
        }

        public void Register(IPointerDownHandler handler, int priority = 0)
        {
            if (handler == null)
            {
                UnityEngine.Debug.LogError("Handler cannot be null");
                return;
            }

            _dispatcher.Register(handler, priority);
        }

        public void Unregister(IPointerDownHandler handler)
        {
            _dispatcher.Unregister(handler);
        }

        public void Update()
        {
            var pointer = Pointer.current;

            if (pointer == null)
            {
                _dispatcher.Cancel();
                return;
            }

            if (!pointer.wasUpdatedThisFrame)
            {
                return;
            }

            var press = pointer.press;
            var value = pointer.position.ReadValue();
            var position = new System.Numerics.Vector2(value.x, value.y);

            if (press.wasPressedThisFrame)
            {
                _dispatcher.Press(pointer.deviceId, position);
            }
            else
            {
                _dispatcher.Move(position);
            }

            if (press.wasReleasedThisFrame)
            {
                _dispatcher.Release(position);
            }
        }
    }
}
