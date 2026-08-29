#nullable enable
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Eede.Domain.ImageEditing;

namespace Eede.Presentation.Common
{
    public class ZoomAndPanController
    {
        private bool _isPanning;
        private bool _isSpacePan;
        private bool _isSpacePressed;
        private Point _panStartPoint;
        private Vector _panStartOffset;
        private IPointer? _capturedPointer;

        public bool IsPanning => _isPanning;
        public bool IsSpacePressed => _isSpacePressed;

        public event Action<bool>? SpaceStateChanged;

        public bool HandleWheel(
            ScrollViewer? scrollViewer,
            PointerWheelEventArgs e,
            Magnification currentMag,
            Action<Magnification> setMag,
            Action? updateLayout = null,
            Visual? relativeTo = null)
        {
            if (scrollViewer == null) return false;
            if ((e.KeyModifiers & KeyModifiers.Control) == 0) return false;

            int deltaY = e.Delta.Y > 0 ? 1 : (e.Delta.Y < 0 ? -1 : 0);
            if (deltaY == 0)
            {
                e.Handled = true;
                return true;
            }

            var newMag = ZoomHelper.CalculateNextMagnification(currentMag, deltaY);
            if (newMag.Value != currentMag.Value)
            {
                var pointerPos = e.GetPosition(relativeTo ?? scrollViewer);
                var newOffset = ZoomHelper.CalculateZoomOffset(scrollViewer.Offset, pointerPos, currentMag.Value, newMag.Value);
                setMag(newMag);
                updateLayout?.Invoke();
                scrollViewer.Offset = newOffset;
            }

            e.Handled = true;
            return true;
        }

        public bool HandlePointerPressed(
            ScrollViewer? scrollViewer,
            Control captureTarget,
            PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(captureTarget).Properties;
            bool isMiddle = props.IsMiddleButtonPressed;
            bool isSpaceLeft = _isSpacePressed && props.IsLeftButtonPressed;

            if (isMiddle || isSpaceLeft)
            {
                _isPanning = true;
                _isSpacePan = isSpaceLeft;
                _panStartPoint = scrollViewer != null ? e.GetPosition(scrollViewer) : e.GetPosition(captureTarget);
                _panStartOffset = scrollViewer?.Offset ?? new Vector(0, 0);
                _capturedPointer = e.Pointer;
                e.Pointer.Capture(captureTarget);
                e.Handled = true;
                return true;
            }
            return false;
        }

        public bool HandlePointerMoved(
            ScrollViewer? scrollViewer,
            Control relativeTarget,
            PointerEventArgs e)
        {
            if (!_isPanning || scrollViewer == null) return false;

            var currentPoint = e.GetPosition(scrollViewer);
            var delta = currentPoint - _panStartPoint;
            scrollViewer.Offset = ZoomHelper.CalculatePanOffset(_panStartOffset, delta);
            e.Handled = true;
            return true;
        }

        public bool HandlePointerReleased(
            Control? relativeTarget,
            PointerReleasedEventArgs e)
        {
            if (!_isPanning) return false;

            var props = e.GetCurrentPoint(relativeTarget).Properties;
            if (!props.IsMiddleButtonPressed && !props.IsLeftButtonPressed)
            {
                _isPanning = false;
                _isSpacePan = false;
                _capturedPointer?.Capture(null);
                _capturedPointer = null;
                e.Handled = true;
                return true;
            }
            return false;
        }

        public void HandlePointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            _isPanning = false;
            _isSpacePan = false;
            _capturedPointer = null;
        }

        public bool HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space && !_isSpacePressed)
            {
                _isSpacePressed = true;
                SpaceStateChanged?.Invoke(true);
                return true;
            }
            return false;
        }

        public bool HandleKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                _isSpacePressed = false;
                SpaceStateChanged?.Invoke(false);
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _isPanning = false;
            _isSpacePan = false;
            _isSpacePressed = false;
            _capturedPointer?.Capture(null);
            _capturedPointer = null;
            SpaceStateChanged?.Invoke(false);
        }
    }
}
