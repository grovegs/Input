using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace GroveGames.Input.Unity
{
    public sealed class UnityPointerDispatcher : IPointerDispatcher, IDisposable
    {
        private readonly PointerDispatcher _dispatcher;
        private readonly Action<InputDevice, InputDeviceChange> _deviceChanged;
        private Pointer _pressedPointer;
        private TouchControl _touch;
        private int _touchId;
        private System.Numerics.Vector2 _position;
        private bool _disposed;

        public UnityPointerDispatcher()
        {
            _dispatcher = new PointerDispatcher();
            _deviceChanged = OnDeviceChange;
            InputSystem.onDeviceChange += _deviceChanged;
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

            if (pointer == null || !pointer.wasUpdatedThisFrame)
            {
                return;
            }

            if (pointer is Touchscreen touchscreen)
            {
                BeginTouch(touchscreen);
                return;
            }

            if (!pointer.press.wasPressedThisFrame)
            {
                return;
            }

            Begin(pointer, null, 0, ToVector(pointer.position.ReadValue()));
            UpdatePress(pointer, true);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            InputSystem.onDeviceChange -= _deviceChanged;
        }

        private void BeginTouch(Touchscreen touchscreen)
        {
            var touches = touchscreen.touches;

            for (var i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];

                if (!touch.press.wasPressedThisFrame)
                {
                    continue;
                }

                Begin(touchscreen, touch, touch.touchId.ReadValue(), ToVector(touch.position.ReadValue()));
                UpdateTouch(true);
                return;
            }
        }

        private void Begin(Pointer pointer, TouchControl touch, int touchId, System.Numerics.Vector2 position)
        {
            _pressedPointer = pointer;
            _touch = touch;
            _touchId = touchId;
            _position = position;
            _dispatcher.Press(pointer.deviceId, position);
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

            if (_touch != null)
            {
                UpdateTouch(isPressFrame);
                return;
            }

            var press = pointer.press;
            var position = ToVector(pointer.position.ReadValue());

            if (press.isPressed && !(press.wasReleasedThisFrame && !isPressFrame))
            {
                Move(position);
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
                Begin(pointer, null, 0, position);
            }
        }

        private void UpdateTouch(bool isPressFrame)
        {
            var touch = _touch;
            var press = touch.press;

            var phase = touch.phase.ReadValue();

            if (touch.touchId.ReadValue() != _touchId || (!isPressFrame && press.wasReleasedThisFrame && press.wasPressedThisFrame))
            {
                End(phase != TouchPhase.None, _position);
                return;
            }

            var position = ToVector(touch.position.ReadValue());

            switch (phase)
            {
                case TouchPhase.Began:
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    Move(position);
                    return;
                case TouchPhase.Ended:
                    Move(position);
                    End(true, position);
                    return;
                default:
                    End(false, default);
                    return;
            }
        }

        private void Move(System.Numerics.Vector2 position)
        {
            _position = position;
            _dispatcher.Move(position);
        }

        private void End(bool isRelease, System.Numerics.Vector2 position)
        {
            _pressedPointer = null;
            _touch = null;
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

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (_pressedPointer != null && ReferenceEquals(device, _pressedPointer) && (change == InputDeviceChange.SoftReset || change == InputDeviceChange.HardReset))
            {
                End(false, default);
            }
        }

        private static System.Numerics.Vector2 ToVector(UnityEngine.Vector2 value)
        {
            return new System.Numerics.Vector2(value.x, value.y);
        }
    }
}
