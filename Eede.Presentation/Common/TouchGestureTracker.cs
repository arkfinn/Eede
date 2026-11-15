#nullable enable
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Eede.Domain.ImageEditing;

namespace Eede.Presentation.Common;

public enum TouchCanvasMode { Draw, Eyedropper, Pan }
public enum TouchDockMode { Pull, Push, Pan }
public enum TouchTapAction { None, TwoFingerUndo, ThreeFingerRedo }

public readonly record struct TouchDownResult(int ActivePointerCount, bool ShouldCancelSingleFingerAction, bool CanStartSingleFingerAction);
public readonly record struct TouchMoveResult(int ActivePointerCount, bool IsMultiTouch, bool CanContinueSingleFingerAction, Vector PanDelta, int ZoomStepDelta, Point PinchCenter);
public readonly record struct TouchUpResult(int ActivePointerCount, TouchTapAction TapAction, bool ShouldEndSingleFingerAction);

public sealed class TouchGestureTracker
{
    public const int LongPressDelayMs = 420;
    public const double SingleFingerMoveTolerance = 6.0;
    public const double MultiTouchPanThreshold = 6.0;
    public const double MultiTouchTapMoveThreshold = 10.0;
    public const int MultiTouchTapMaxDurationMs = 350;
    public const double PinchZoomInThreshold = 1.30;
    public const double PinchZoomOutThreshold = 0.77;
    public const long StalePointerTimeoutMs = 3000;

    private readonly Dictionary<int, (Point Start, Point Current)> _activePointers = new(4);
    private long _gestureStartTimeMs, _lastEventTimestampMs;
    private int _maxPointerCount;
    private double _maxMoveDistance, _anchorDistance;
    private bool _requireAllPointersUp, _singleFingerActionActive, _isLongPressEligible, _longPressTriggered, _hasPanOrPinchActivated;
    private Point _initialMidpoint, _lastMidpoint;

    public int ActivePointerCount => _activePointers.Count;
    public bool RequireAllPointersUp => _requireAllPointersUp;
    public bool IsLongPressEligible => _isLongPressEligible;

    public TouchDownResult OnPointerDown(int pointerId, Point position, long timestampMs)
    {
        if (_activePointers.Count > 0 && timestampMs - _lastEventTimestampMs > StalePointerTimeoutMs) Reset();
        _lastEventTimestampMs = timestampMs;
        _activePointers[pointerId] = (position, position);

        if (_activePointers.Count == 1 && !_requireAllPointersUp)
        {
            _gestureStartTimeMs = timestampMs;
            _maxPointerCount = 1;
            _maxMoveDistance = 0;
            _isLongPressEligible = _singleFingerActionActive = true;
            _longPressTriggered = _hasPanOrPinchActivated = false;
            return new TouchDownResult(1, false, true);
        }

        bool shouldCancel = _singleFingerActionActive;
        _singleFingerActionActive = _isLongPressEligible = false;
        _requireAllPointersUp = true;
        if (_activePointers.Count > _maxPointerCount) _maxPointerCount = _activePointers.Count;
        if (_activePointers.Count == 2 && TryGetFirstTwoPointers(out var p0, out var p1))
        {
            _initialMidpoint = _lastMidpoint = new Point((p0.Current.X + p1.Current.X) * 0.5, (p0.Current.Y + p1.Current.Y) * 0.5);
            _anchorDistance = GetDistance(p0.Current, p1.Current);
        }
        return new TouchDownResult(_activePointers.Count, shouldCancel, false);
    }

    public bool TryTriggerLongPress(long currentTimestampMs, out Point position)
    {
        if (_activePointers.Count == 1 && !_requireAllPointersUp && _isLongPressEligible && !_longPressTriggered &&
            currentTimestampMs - _gestureStartTimeMs >= LongPressDelayMs)
        {
            foreach (var entry in _activePointers.Values)
            {
                _longPressTriggered = true;
                _isLongPressEligible = _singleFingerActionActive = false;
                position = entry.Current;
                return true;
            }
        }
        position = default;
        return false;
    }

    public TouchMoveResult OnPointerMoved(int pointerId, Point position, long timestampMs)
    {
        if (!_activePointers.TryGetValue(pointerId, out var entry)) return default;
        _lastEventTimestampMs = timestampMs;
        _activePointers[pointerId] = (entry.Start, position);
        double dist = GetDistance(entry.Start, position);
        if (dist > _maxMoveDistance) _maxMoveDistance = dist;

        if (_activePointers.Count == 1)
        {
            if (_requireAllPointersUp) return new TouchMoveResult(1, false, false, default, 0, position);
            if (dist > SingleFingerMoveTolerance) _isLongPressEligible = false;
            return new TouchMoveResult(1, false, _singleFingerActionActive && !_longPressTriggered, default, 0, position);
        }

        if (TryGetFirstTwoPointers(out var p0, out var p1))
        {
            var midpoint = new Point((p0.Current.X + p1.Current.X) * 0.5, (p0.Current.Y + p1.Current.Y) * 0.5);
            double distance = GetDistance(p0.Current, p1.Current);
            double ratio = _anchorDistance > 1.0 ? distance / _anchorDistance : 1.0;
            if (!_hasPanOrPinchActivated && (GetDistance(_initialMidpoint, midpoint) >= MultiTouchPanThreshold || ratio >= PinchZoomInThreshold || ratio <= PinchZoomOutThreshold))
                _hasPanOrPinchActivated = true;

            Vector panDelta = default;
            int zoomStep = 0;
            if (_hasPanOrPinchActivated)
            {
                panDelta = midpoint - _lastMidpoint;
                _lastMidpoint = midpoint;
                if (ratio >= PinchZoomInThreshold) { zoomStep = 1; _anchorDistance = distance; }
                else if (ratio <= PinchZoomOutThreshold && distance > 1.0) { zoomStep = -1; _anchorDistance = distance; }
            }
            return new TouchMoveResult(_activePointers.Count, true, false, panDelta, zoomStep, midpoint);
        }
        return new TouchMoveResult(_activePointers.Count, true, false, default, 0, position);
    }

    public TouchUpResult OnPointerUp(int pointerId, Point position, long timestampMs)
    {
        if (_activePointers.TryGetValue(pointerId, out var entry))
        {
            double dist = GetDistance(entry.Start, position);
            if (dist > _maxMoveDistance) _maxMoveDistance = dist;
            _activePointers.Remove(pointerId);
        }
        _lastEventTimestampMs = timestampMs;
        _isLongPressEligible = false;

        if (_activePointers.Count == 0)
        {
            bool shouldEndSingle = _singleFingerActionActive && !_requireAllPointersUp && !_longPressTriggered;
            var tapAction = (_requireAllPointersUp && !_hasPanOrPinchActivated && timestampMs - _gestureStartTimeMs <= MultiTouchTapMaxDurationMs && _maxMoveDistance <= MultiTouchTapMoveThreshold)
                ? (_maxPointerCount == 2 ? TouchTapAction.TwoFingerUndo : _maxPointerCount == 3 ? TouchTapAction.ThreeFingerRedo : TouchTapAction.None)
                : TouchTapAction.None;
            Reset();
            return new TouchUpResult(0, tapAction, shouldEndSingle);
        }

        if (_activePointers.Count == 2 && TryGetFirstTwoPointers(out var p0, out var p1))
        {
            _lastMidpoint = new Point((p0.Current.X + p1.Current.X) * 0.5, (p0.Current.Y + p1.Current.Y) * 0.5);
            _anchorDistance = GetDistance(p0.Current, p1.Current);
        }
        return new TouchUpResult(_activePointers.Count, TouchTapAction.None, false);
    }

    public bool OnPointerCaptureLost(int pointerId)
    {
        bool wasSingleActive = _singleFingerActionActive;
        _activePointers.Remove(pointerId);
        _isLongPressEligible = _singleFingerActionActive = false;
        if (_activePointers.Count == 0) Reset();
        return wasSingleActive;
    }

    public void Reset()
    {
        _activePointers.Clear();
        _requireAllPointersUp = _singleFingerActionActive = _isLongPressEligible = _longPressTriggered = _hasPanOrPinchActivated = false;
        _maxPointerCount = 0;
        _maxMoveDistance = 0;
    }

    public static void ApplyMultiTouchPanAndZoom(in TouchMoveResult move, ScrollViewer? scrollViewer, Magnification currentMag, Action<Magnification> setMag, Action updateLayout, Vector margin)
    {
        if (scrollViewer == null) return;
        if (move.PanDelta != default) scrollViewer.Offset = ZoomHelper.CalculatePanOffset(scrollViewer.Offset, move.PanDelta);
        if (move.ZoomStepDelta != 0)
        {
            var newMag = ZoomHelper.CalculateNextMagnification(currentMag, move.ZoomStepDelta);
            if (newMag.Value != currentMag.Value)
            {
                var newOffset = ZoomHelper.CalculateZoomOffset(scrollViewer.Offset, move.PinchCenter, currentMag.Value, newMag.Value, margin);
                setMag(newMag);
                updateLayout();
                scrollViewer.Offset = newOffset;
            }
        }
    }

    private bool TryGetFirstTwoPointers(out (Point Start, Point Current) first, out (Point Start, Point Current) second)
    {
        first = second = default;
        int idx = 0;
        foreach (var entry in _activePointers.Values)
        {
            if (idx == 0) first = entry;
            else if (idx == 1) { second = entry; return true; }
            idx++;
        }
        return false;
    }

    private static double GetDistance(Point a, Point b) => Math.Sqrt(((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y)));
}
