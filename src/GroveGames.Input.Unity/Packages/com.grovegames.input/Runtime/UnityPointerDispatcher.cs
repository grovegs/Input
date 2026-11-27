using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace GroveGames.Input.Unity
{
    public sealed class UnityPointerDispatcher : IPointerDispatcher
    {
        private readonly PointerDispatcher _dispatcher;
        private Pointer _pressedPointer;
        private int _touchId;

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
            if (_pressedPointer != null)
            {
                UpdatePress(_pressedPointer, false);

                if (_pressedPointer != null)
                {
                    return;
                }
            }

            var pointer = Pointer.current;

            if (pointer == null || !pointer.wasUpdatedThisFrame || !pointer.press.wasPressedThisFrame)
            {
                return;
            }

            _pressedPointer = pointer;
            _touchId = pointer is Touchscreen touchscreen ? touchscreen.primaryTouch.touchId.ReadValue() : 0;
            _dispatcher.Press(pointer.deviceId, ToVector(pointer.position.ReadValue()));

            UpdatePress(pointer, true);
        }

        private void UpdatePress(Pointer pointer, bool isPressFrame)
        {
            if (!pointer.added || !pointer.enabled)
            {
                End(false, default);
                return;
            }

            if (!isPressFrame && !pointer.wasUpdatedThisFrame)
            {
                return;
            }

            if (pointer is Touchscreen touchscreen)
            {
                UpdateTouch(touchscreen);
                return;
            }

            var press = pointer.press;
            var position = ToVector(pointer.position.ReadValue());

            if (press.isPressed && !(press.wasReleasedThisFrame && !isPressFrame))
            {
                _dispatcher.Move(position);
                return;
            }

            if (!press.wasReleasedThisFrame)
            {
                End(false, default);
                return;
            }

            End(true, position);

            if (!isPressFrame && press.wasPressedThisFrame && press.isPressed)
            {
                _pressedPointer = pointer;
                _touchId = 0;
                _dispatcher.Press(pointer.deviceId, position);
            }
        }

        private void UpdateTouch(Touchscreen touchscreen)
        {
            var touches = touchscreen.touches;

            for (var i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];

                if (touch.touchId.ReadValue() != _touchId)
                {
                    continue;
                }

                var position = ToVector(touch.position.ReadValue());

                switch (touch.phase.ReadValue())
                {
                    case TouchPhase.Began:
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        _dispatcher.Move(position);
                        return;
                    case TouchPhase.Ended:
                        _dispatcher.Move(position);
                        End(true, position);
                        return;
                    default:
                        End(false, default);
                        return;
                }
            }

            End(false, default);
        }

        private void End(bool isRelease, System.Numerics.Vector2 position)
        {
            _pressedPointer = null;
            _touchId = 0;

            if (isRelease)
            {
                _dispatcher.Release(position);
            }
            else
            {
                _dispatcher.Cancel();
            }
        }

        private static System.Numerics.Vector2 ToVector(UnityEngine.Vector2 value)
        {
            return new System.Numerics.Vector2(value.x, value.y);
        }
    }
}
