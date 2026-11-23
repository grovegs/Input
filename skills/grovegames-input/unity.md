## Unity

- The handler interfaces, the events and `Key` are in the `GroveGames.Input` namespace. `GroveGames.Input.Unity` holds `UnityPointerDispatcher`, `UnityKeyboardDispatcher` and the registration.
- Register with `builder.AddPointerDispatcher()` and `builder.AddKeyboardDispatcher()` in the root installer.
- Never read `Pointer.current`, `Keyboard.current`, `Touchscreen.current` or `UnityEngine.Input` in game code.
- An event's `Position` is `System.Numerics.Vector2` in screen pixels. Convert it once at the edge: `new UnityEngine.Vector2(position.X, position.Y)`.
- An asmdef that overrides references lists `GroveGames.Input.dll` in its precompiled references.
