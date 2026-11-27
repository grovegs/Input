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

        [Test]
        public void Update_FirstOfTwoFingersLifts_ReleasesAtOnce()
        {
            InputSystem.AddDevice<Touchscreen>();
            var dispatcher = new UnityPointerDispatcher();
            var handler = new FakePointerHandler();
            dispatcher.Register(handler);

            BeginTouch(1, new Vector2(10f, 10f));
            dispatcher.Update();
            BeginTouch(2, new Vector2(500f, 500f));
            dispatcher.Update();
            EndTouch(1, new Vector2(20f, 20f));
            dispatcher.Update();

            Assert.That(handler.Log, Is.EqualTo("D M(20,20) U(20,20) "));
        }

        [Test]
        public void Update_SecondFingerMoves_DoesNotMoveThePress()
        {
            InputSystem.AddDevice<Touchscreen>();
            var dispatcher = new UnityPointerDispatcher();
            var handler = new FakePointerHandler();
            dispatcher.Register(handler);

            BeginTouch(1, new Vector2(10f, 10f));
            dispatcher.Update();
            BeginTouch(2, new Vector2(500f, 500f));
            dispatcher.Update();
            MoveTouch(2, new Vector2(600f, 600f));
            dispatcher.Update();

            Assert.That(handler.Log, Is.EqualTo("D "));
        }

        [Test]
        public void Update_TwoFingersCanceledTogether_Cancels()
        {
            var touchscreen = InputSystem.AddDevice<Touchscreen>();
            var dispatcher = new UnityPointerDispatcher();
            var handler = new FakePointerHandler();
            dispatcher.Register(handler);

            BeginTouch(1, new Vector2(10f, 10f));
            dispatcher.Update();
            BeginTouch(2, new Vector2(500f, 500f));
            dispatcher.Update();
            InputSystem.QueueStateEvent(touchscreen, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Canceled, position = new Vector2(10f, 10f) });
            InputSystem.QueueStateEvent(touchscreen, new TouchState { touchId = 2, phase = UnityEngine.InputSystem.TouchPhase.Canceled, position = new Vector2(500f, 500f) });
            InputSystem.Update();
            dispatcher.Update();

            Assert.That(handler.UpCount, Is.EqualTo(0));
            Assert.That(handler.CancelCount, Is.EqualTo(1));
        }

        [Test]
        public void Update_FocusLostWhileMouseHeld_Cancels()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            var dispatcher = new UnityPointerDispatcher();
            var handler = new FakePointerHandler();
            dispatcher.Register(handler);

            Press(mouse.leftButton);
            dispatcher.Update();
            ScheduleFocusChangedEvent(false);
            InputSystem.Update();
            dispatcher.Update();
            ScheduleFocusChangedEvent(true);
            InputSystem.Update();
            dispatcher.Update();

            Assert.That(handler.UpCount, Is.EqualTo(0));
            Assert.That(handler.CancelCount, Is.EqualTo(1));
        }

        [Test]
        public void Update_FocusLostWhileTouching_Cancels()
        {
            InputSystem.AddDevice<Touchscreen>();
            var dispatcher = new UnityPointerDispatcher();
            var handler = new FakePointerHandler();
            dispatcher.Register(handler);

            BeginTouch(1, new Vector2(10f, 10f));
            dispatcher.Update();
            ScheduleFocusChangedEvent(false);
            InputSystem.Update();
            dispatcher.Update();
            ScheduleFocusChangedEvent(true);
            InputSystem.Update();
            dispatcher.Update();

            Assert.That(handler.UpCount, Is.EqualTo(0));
            Assert.That(handler.CancelCount, Is.EqualTo(1));
        }

        [Test]
        public void Update_MouseReleasedAndPressedInOneFrame_ReleasesThenPresses()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            var dispatcher = new UnityPointerDispatcher();
            var handler = new FakePointerHandler();
            dispatcher.Register(handler);

            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(10f, 10f) }.WithButton(MouseButton.Left));
            InputSystem.Update();
            dispatcher.Update();
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(10f, 10f) });
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(10f, 10f) }.WithButton(MouseButton.Left));
            InputSystem.Update();
            dispatcher.Update();
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(10f, 10f) });
            InputSystem.Update();
            dispatcher.Update();

            Assert.That(handler.Log, Is.EqualTo("D U(10,10) D U(10,10) "));
        }

        [Test]
        public void Update_MouseMovesDuringTouch_KeepsFollowingTheTouch()
        {
            InputSystem.AddDevice<Touchscreen>();
            var mouse = InputSystem.AddDevice<Mouse>();
            var dispatcher = new UnityPointerDispatcher();
            var handler = new FakePointerHandler();
            dispatcher.Register(handler);

            BeginTouch(1, new Vector2(10f, 10f));
            dispatcher.Update();
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(900f, 900f) });
            InputSystem.Update();
            dispatcher.Update();
            EndTouch(1, new Vector2(10f, 10f));
            dispatcher.Update();

            Assert.That(handler.Log, Is.EqualTo("D U(10,10) "));
        }

        private sealed class FakePointerHandler : IPointerDownHandler, IPointerMoveHandler, IPointerUpHandler, IPointerCancelHandler
        {
            public int DownCount { get; private set; }
            public int UpCount { get; private set; }
            public int CancelCount { get; private set; }

            private readonly System.Text.StringBuilder _log;

            public FakePointerHandler()
            {
                _log = new System.Text.StringBuilder();
            }

            public string Log => _log.ToString();

            public void HandlePointerDown(ref PointerDownEvent pointerEvent)
            {
                _log.Append("D ");
                DownCount++;
                pointerEvent.MarkHandled();
            }

            public void HandlePointerMove(in PointerMoveEvent pointerEvent)
            {
                _log.Append("M(").Append(pointerEvent.Position.X).Append(',').Append(pointerEvent.Position.Y).Append(") ");
            }

            public void HandlePointerUp(in PointerUpEvent pointerEvent)
            {
                _log.Append("U(").Append(pointerEvent.Position.X).Append(',').Append(pointerEvent.Position.Y).Append(") ");
                UpCount++;
            }

            public void HandlePointerCancel(in PointerCancelEvent pointerEvent)
            {
                _log.Append("C ");
                CancelCount++;
            }
        }
    }
}
