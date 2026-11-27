using System;

using UnityEngine.InputSystem;

using UnityKey = UnityEngine.InputSystem.Key;

namespace GroveGames.Input.Unity
{
    public sealed class UnityKeyboardDispatcher : IKeyboardDispatcher
    {
        private static readonly Key[] s_keys = CreateKeys();

        private readonly KeyboardDispatcher _dispatcher;

        public UnityKeyboardDispatcher()
        {
            _dispatcher = new KeyboardDispatcher();
        }

        public void Register(IKeyDownHandler handler, int priority = 0)
        {
            if (handler == null)
            {
                UnityEngine.Debug.LogError("Handler cannot be null");
                return;
            }

            _dispatcher.Register(handler, priority);
        }

        public void Unregister(IKeyDownHandler handler)
        {
            _dispatcher.Unregister(handler);
        }

        public void Update()
        {
            var keyboard = Keyboard.current;

            if (keyboard == null)
            {
                _dispatcher.ReleaseAll();
                return;
            }

            if (!keyboard.wasUpdatedThisFrame || _dispatcher.IsIdle)
            {
                return;
            }

            var controls = keyboard.allKeys;

            for (var i = 0; i < controls.Count; i++)
            {
                var control = controls[i];

                if (control == null)
                {
                    continue;
                }

                var index = (int)control.keyCode;

                if ((uint)index >= s_keys.Length)
                {
                    continue;
                }

                var key = s_keys[index];

                if (key == Key.None)
                {
                    continue;
                }

                var wasPressed = control.wasPressedThisFrame;
                var wasReleased = control.wasReleasedThisFrame;

                if (wasPressed && wasReleased && control.isPressed)
                {
                    _dispatcher.Release(key);
                    _dispatcher.Press(key);
                    continue;
                }

                if (wasPressed)
                {
                    _dispatcher.Press(key);
                }

                if (wasReleased)
                {
                    _dispatcher.Release(key);
                }
            }
        }

        private static Key[] CreateKeys()
        {
            var names = Enum.GetNames(typeof(UnityKey));
            var values = (UnityKey[])Enum.GetValues(typeof(UnityKey));
            var length = 0;

            for (var i = 0; i < values.Length; i++)
            {
                length = Math.Max(length, (int)values[i] + 1);
            }

            var keys = new Key[length];

            for (var i = 0; i < names.Length; i++)
            {
                if (Enum.TryParse<Key>(names[i], out var key))
                {
                    keys[(int)Enum.Parse(typeof(UnityKey), names[i])] = key;
                }
            }

            return keys;
        }
    }
}
