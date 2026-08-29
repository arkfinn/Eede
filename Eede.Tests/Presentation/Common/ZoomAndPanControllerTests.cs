using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Eede.Domain.ImageEditing;
using Eede.Presentation.Common;
using NUnit.Framework;

namespace Eede.Tests.Presentation.Common
{
    [TestFixture]
    public class ZoomAndPanControllerTests
    {
        private Window _window;
        private ScrollViewer _scrollViewer;
        private Border _content;
        private ZoomAndPanController _controller;

        [SetUp]
        public void Setup()
        {
            _content = new Border
            {
                Width = 1000,
                Height = 1000
            };
            _scrollViewer = new ScrollViewer
            {
                Width = 200,
                Height = 200,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
                Content = _content
            };
            _controller = new ZoomAndPanController();

            _window = new Window
            {
                Content = _scrollViewer,
                Width = 400,
                Height = 400
            };
            _window.ApplyTemplate();
            _window.Show();
            _scrollViewer.ApplyTemplate();
            _window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        [AvaloniaTest]
        public void HandleWheel_WithCtrl_ZoomsInAndUpdatesOffset()
        {
            var initialMag = new Magnification(2f);
            Magnification? changedMag = null;
            _scrollViewer.Offset = new Vector(50, 50);
            _window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var pointer = new Avalonia.Input.Pointer(1, PointerType.Mouse, true);
            var wheelProps = new PointerPointProperties(RawInputModifiers.Control, PointerUpdateKind.Other);
            var wheelArgs = new PointerWheelEventArgs(
                _content,
                pointer,
                _window,
                new Point(100, 100),
                0,
                wheelProps,
                KeyModifiers.Control,
                new Vector(0, 1));

            bool handled = _controller.HandleWheel(
                _scrollViewer,
                wheelArgs,
                initialMag,
                mag =>
                {
                    changedMag = mag;
                    _content.Width = 2000;
                    _content.Height = 2000;
                },
                () =>
                {
                    _window.UpdateLayout();
                    Dispatcher.UIThread.RunJobs();
                },
                relativeTo: _content);

            Assert.That(handled, Is.True);
            Assert.That(wheelArgs.Handled, Is.True);
            Assert.That(changedMag?.Value, Is.EqualTo(4f));
            // content上の位置は (100 - 50) = 50
            // 幾何補正: (50 + 50) * (4 / 2) - 50 = 200 - 50 = 150
            Assert.That(_scrollViewer.Offset.X, Is.EqualTo(150.0).Within(0.001));
            Assert.That(_scrollViewer.Offset.Y, Is.EqualTo(150.0).Within(0.001));
        }

        [AvaloniaTest]
        public void HandleWheel_WithoutCtrl_DoesNothing()
        {
            var initialMag = new Magnification(2f);
            Magnification? changedMag = null;

            var pointer = new Avalonia.Input.Pointer(1, PointerType.Mouse, true);
            var wheelProps = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other);
            var wheelArgs = new PointerWheelEventArgs(
                _content,
                pointer,
                _window,
                new Point(100, 100),
                0,
                wheelProps,
                KeyModifiers.None,
                new Vector(0, 1));

            bool handled = _controller.HandleWheel(
                _scrollViewer,
                wheelArgs,
                initialMag,
                mag => changedMag = mag);

            Assert.That(handled, Is.False);
            Assert.That(wheelArgs.Handled, Is.False);
            Assert.That(changedMag, Is.Null);
        }

        [AvaloniaTest]
        public void HandlePointer_MiddleDrag_PansScrollViewer()
        {
            _scrollViewer.Offset = new Vector(100, 100);
            _window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var pointer = new Avalonia.Input.Pointer(1, PointerType.Mouse, true);

            // 1. 中ボタン押下
            var pressedProperties = new PointerPointProperties(RawInputModifiers.MiddleMouseButton, PointerUpdateKind.MiddleButtonPressed);
            var pressedArgs = new PointerPressedEventArgs(
                _content,
                pointer,
                _window,
                new Point(50, 50),
                0,
                pressedProperties,
                KeyModifiers.None);

            bool pressedHandled = _controller.HandlePointerPressed(_scrollViewer, _content, pressedArgs);
            Assert.That(pressedHandled, Is.True);
            Assert.That(_controller.IsPanning, Is.True);

            // 2. ドラッグ移動 (50, 50) -> (30, 20) (左上へ移動 = コンテンツを左上へ引っ張る = Offset増加)
            // delta = (30 - 50, 20 - 50) = (-20, -30) -> NewOffset = (100 - (-20), 100 - (-30)) = (120, 130)
            var moveProperties = new PointerPointProperties(RawInputModifiers.MiddleMouseButton, PointerUpdateKind.Other);
            var moveArgs = new PointerEventArgs(
                InputElement.PointerMovedEvent,
                _content,
                pointer,
                _window,
                new Point(30, 20),
                0,
                moveProperties,
                KeyModifiers.None);

            bool moveHandled = _controller.HandlePointerMoved(_scrollViewer, _content, moveArgs);
            Assert.That(moveHandled, Is.True);
            Assert.That(_scrollViewer.Offset.X, Is.EqualTo(120.0).Within(0.001));
            Assert.That(_scrollViewer.Offset.Y, Is.EqualTo(130.0).Within(0.001));

            // 3. リリース
            var releasedProperties = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.MiddleButtonReleased);
            var releasedArgs = new PointerReleasedEventArgs(
                _content,
                pointer,
                _window,
                new Point(30, 20),
                0,
                releasedProperties,
                KeyModifiers.None,
                MouseButton.Middle);

            bool releaseHandled = _controller.HandlePointerReleased(_content, releasedArgs);
            Assert.That(releaseHandled, Is.True);
            Assert.That(_controller.IsPanning, Is.False);
        }

        [AvaloniaTest]
        public void HandlePointer_SpaceLeftDrag_PansScrollViewer()
        {
            _scrollViewer.Offset = new Vector(100, 100);
            _window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var pointer = new Avalonia.Input.Pointer(1, PointerType.Mouse, true);

            // 1. Space キー押下
            var keyArgs = new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key = Key.Space
            };
            _controller.HandleKeyDown(keyArgs);
            Assert.That(_controller.IsSpacePressed, Is.True);

            // 2. 左ボタン押下 (Space+左)
            var pressedProperties = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            var pressedArgs = new PointerPressedEventArgs(
                _content,
                pointer,
                _window,
                new Point(50, 50),
                0,
                pressedProperties,
                KeyModifiers.None);

            bool pressedHandled = _controller.HandlePointerPressed(_scrollViewer, _content, pressedArgs);
            Assert.That(pressedHandled, Is.True);
            Assert.That(_controller.IsPanning, Is.True);

            // 3. 移動 (50, 50) -> (70, 80) (右下へ移動 = Offset減少)
            var moveProperties = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.Other);
            var moveArgs = new PointerEventArgs(
                InputElement.PointerMovedEvent,
                _content,
                pointer,
                _window,
                new Point(70, 80),
                0,
                moveProperties,
                KeyModifiers.None);

            bool moveHandled = _controller.HandlePointerMoved(_scrollViewer, _content, moveArgs);
            Assert.That(moveHandled, Is.True);
            Assert.That(_scrollViewer.Offset.X, Is.EqualTo(80.0).Within(0.001));
            Assert.That(_scrollViewer.Offset.Y, Is.EqualTo(70.0).Within(0.001));

            // 4. 左ボタン離す
            var releasedProperties = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased);
            var releasedArgs = new PointerReleasedEventArgs(
                _content,
                pointer,
                _window,
                new Point(70, 80),
                0,
                releasedProperties,
                KeyModifiers.None,
                MouseButton.Left);

            bool releaseHandled = _controller.HandlePointerReleased(_content, releasedArgs);
            Assert.That(releaseHandled, Is.True);
            Assert.That(_controller.IsPanning, Is.False);
        }

        [AvaloniaTest]
        public void HandlePointer_NormalLeftDragWithoutSpace_DoesNotPan()
        {
            var pointer = new Avalonia.Input.Pointer(1, PointerType.Mouse, true);

            var pressedProperties = new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed);
            var pressedArgs = new PointerPressedEventArgs(
                _content,
                pointer,
                _window,
                new Point(50, 50),
                0,
                pressedProperties,
                KeyModifiers.None);

            bool pressedHandled = _controller.HandlePointerPressed(_scrollViewer, _content, pressedArgs);
            Assert.That(pressedHandled, Is.False);
            Assert.That(_controller.IsPanning, Is.False);
        }
    }
}
