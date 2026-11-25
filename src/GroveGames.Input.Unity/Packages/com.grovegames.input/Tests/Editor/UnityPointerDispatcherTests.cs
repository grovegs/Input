using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace GroveGames.Input.Unity.Tests
{
    public sealed class UnityPointerDispatcherTests : InputTestFixture
    {
        [Test]
        public void Update_TouchEnded_Releases()
        {
            InputSystem.AddDevice<Touchscreen>();
            var dispatcher = new UnityPointerDispatcher();
            var handler = new FakePointerHandler();
            dispatcher.Register(handler);

            BeginTouch(1, new Vector2(10f, 10f));
            dispatcher.Update();
            EndTouch(1, new Vector2(10f, 10f));
            dispatcher.Update();

            Assert.That(handler.DownCount, Is.EqualTo(1));
            Assert.That(handler.UpCount, Is.EqualTo(1));
            Assert.That(handler.CancelCount, Is.EqualTo(0));
        }

        [Test]
        public void Update_TouchCanceled_CancelsInsteadOfReleasing()
        {
            var touchscreen = InputSystem.AddDevice<Touchscreen>();
            var dispatcher = new UnityPointerDispatcher();
            var handler = new FakePointerHandler();
            dispatcher.Register(handler);

            BeginTouch(1, new Vector2(10f, 10f));
            dispatcher.Update();
            InputSystem.QueueStateEvent(touchscreen, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Canceled, position = new Vector2(10f, 10f) });
            InputSystem.Update();
            dispatcher.Update();

            Assert.That(handler.DownCount, Is.EqualTo(1));
            Assert.That(handler.UpCount, Is.EqualTo(0));
            Assert.That(handler.CancelCount, Is.EqualTo(1));
        }

        private sealed class FakePointerHandler : IPointerDownHandler, IPointerUpHandler, IPointerCancelHandler
        {
            public int DownCount { get; private set; }
            public int UpCount { get; private set; }
            public int CancelCount { get; private set; }

            public void HandlePointerDown(ref PointerDownEvent pointerEvent)
            {
                DownCount++;
                pointerEvent.MarkHandled();
            }

            public void HandlePointerUp(in PointerUpEvent pointerEvent)
            {
                UpCount++;
            }

            public void HandlePointerCancel(in PointerCancelEvent pointerEvent)
            {
                CancelCount++;
            }
        }
    }
}
