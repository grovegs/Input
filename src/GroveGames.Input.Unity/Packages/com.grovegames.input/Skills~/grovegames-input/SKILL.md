---
name: grovegames-input
description: Rules for pointer and keyboard input in game code with GroveGames.Input. Use when writing or reviewing a class that implements IPointerDownHandler, IKeyDownHandler or one of the other handler interfaces, registers with a dispatcher, or decides which of two things under the pointer gets the press.
---

# Using GroveGames.Input

Rules for game code that uses this package. They hold in every game. The API is in the package's `README.md`.

- Game code never reads the engine's input API. It implements handler interfaces and registers with `IPointerDispatcher` or `IKeyboardDispatcher`.
- Register in `Initialize` and unregister in `Dispose`, with the same instance.
- Priority decides who is asked first; higher wins. Give priorities as named constants in one place per game, never as numbers at the call site. Something that must block everything below it, such as a modal, registers above it.
- Call `MarkHandled()` only when the press is really yours. A handler that marks every press handled blocks everything with a lower priority.
- Only the handler that handled the press gets the move, the release and the cancel. Do not expect a release for a press you did not handle.
- Implement `IPointerCancelHandler` whenever a press starts something that must be undone, such as a drag. A cancel arrives instead of a release when a second press starts, the owner is unregistered, or in Unity the system cancels the touch (an incoming call or the notification shade).
- Handlers run inside the dispatch, so keep them short. A handler that unregisters itself inside `HandlePointerDown` or `HandleKeyDown` does not own the press and gets no later events for it. Do not call `Press`, `Release` or `Cancel` on a dispatcher from inside a handler.

## Unity

- The handler interfaces, the events and `Key` are in the `GroveGames.Input` namespace. `GroveGames.Input.Unity` holds `UnityPointerDispatcher`, `UnityKeyboardDispatcher` and the registration.
- Register with `builder.AddPointerDispatcher()` and `builder.AddKeyboardDispatcher()` in the root installer.
- Never read `Pointer.current`, `Keyboard.current`, `Touchscreen.current` or `UnityEngine.Input` in game code.
- An event's `Position` is `System.Numerics.Vector2` in screen pixels. Convert it once at the edge: `new UnityEngine.Vector2(position.X, position.Y)`.
- An asmdef that overrides references lists `GroveGames.Input.dll` in its precompiled references.
