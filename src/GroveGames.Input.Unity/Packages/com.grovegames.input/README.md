# GroveGames.Input

[![Build Status](https://github.com/grovegs/Input/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/Input/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/Input/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/Input/actions/workflows/tests.yml)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.Input)](https://www.nuget.org/packages/GroveGames.Input)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/Input)](https://github.com/grovegs/Input/releases/latest)

Prioritized pointer and keyboard events with handled state for .NET and Unity

## Features

- Handlers register with a priority. A press goes to the highest priority first, and the first handler that marks it handled owns the pointer or the key until it is released
- `PointerDispatcher`: press, move, release and cancel for one pointer, on `System.Numerics.Vector2`
- `KeyboardDispatcher`: key down and key up with an engine-free `Key`
- Small handler interfaces: `IPointerDownHandler`, `IPointerMoveHandler`, `IPointerUpHandler`, `IPointerCancelHandler`, `IKeyDownHandler`, `IKeyUpHandler`
- Unity: `UnityPointerDispatcher` and `UnityKeyboardDispatcher` read the Input System every frame

## .NET

### Installation

```bash
dotnet add package GroveGames.Input
```

### Usage

```csharp
using System.Numerics;
using GroveGames.Input;

public sealed class DragHandler : IPointerDownHandler, IPointerMoveHandler, IPointerUpHandler
{
    public void HandlePointerDown(ref PointerDownEvent pointerEvent)
    {
        pointerEvent.MarkHandled();
    }

    public void HandlePointerMove(in PointerMoveEvent pointerEvent)
    {
    }

    public void HandlePointerUp(in PointerUpEvent pointerEvent)
    {
    }
}

var dispatcher = new PointerDispatcher();
dispatcher.Register(new DragHandler(), priority: 10);

dispatcher.Press(pointerId: 0, new Vector2(120f, 300f));
dispatcher.Move(new Vector2(140f, 310f));
dispatcher.Release(new Vector2(140f, 310f));
```

- Handlers with the same priority are asked in the order they registered.
- Only the handler that handled the press gets the move, the release and the cancel, and only the ones it implements.
- A new press while one is down cancels the first. Unregistering the owner cancels its press, and a handler that unregisters while handling the press does not become its owner.
- In Unity, a touch the system cancels (an incoming call, the notification shade) arrives as a cancel, not a release, so it never clicks.
- `KeyboardDispatcher` works the same way with `Press(Key)`, `Release(Key)` and `ReleaseAll()`.

An engine feeds a dispatcher from its own input. Positions are in whatever space the engine passes in.

## Unity

### Installation

1. Install the core package via [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity):
   - Open Window > NuGet > Manage NuGet Packages
   - Search for `GroveGames.Input` and install

2. Add the Unity package via Git URL:
   - Open Package Manager (Window > Package Manager)
   - Click "+" > "Add package from git URL"
   - Enter: `https://github.com/grovegs/Input.git?path=src/GroveGames.Input.Unity/Packages/com.grovegames.input`

3. Create a `csc.rsp` file in your `Assets/` directory with the following content to enable C# 10 features:

   ```text
   -langversion:10
   -nullable:enable
   ```

### Usage

With [GroveGames.DependencyInjection](https://github.com/grovegs/DependencyInjection) installed:

```csharp
using GroveGames.Input.Unity;

builder.AddPointerDispatcher();
builder.AddKeyboardDispatcher();
```

Both register the dispatcher as `IPointerDispatcher` or `IKeyboardDispatcher` and update it before every frame. Without DependencyInjection, create a `UnityPointerDispatcher` or `UnityKeyboardDispatcher` and call `Update()` once per frame.

- Positions are screen pixels, as `System.Numerics.Vector2`. Convert with `new UnityEngine.Vector2(position.X, position.Y)`.
- `Key` has the same names as the Input System's `Key`. OEM keys, F13 to F24, the media keys and the IME key are not dispatched.

## Development

### Project Structure

- `src/GroveGames.Input/` - Core .NET library (`net10.0`, `netstandard2.1`)
- `src/GroveGames.Input.Unity/` - Unity package
- `tests/` - xUnit tests
- `sandbox/` - Sample applications for testing

### Commands

```bash
dotnet build                        # Build all projects
dotnet format                       # Format code
dotnet test                         # Run tests
dotnet pack -c Release              # Create NuGet packages
```

### Requirements

- .NET 10.0 SDK

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
