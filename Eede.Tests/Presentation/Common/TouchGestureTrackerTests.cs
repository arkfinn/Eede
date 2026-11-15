using Avalonia;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Eede.Presentation.Common;
using Eede.Presentation.Views.DataEntry;
using Eede.Presentation.Views.General;
using NUnit.Framework;
using ReactiveUI;

namespace Eede.Tests.Presentation.Common;

#nullable enable

[TestFixture]
public class TouchGestureTrackerTests
{
    [Test]
    public void SingleToTwoFingerTransition_ShouldRequestCancelAndRequireAllPointersUp()
    {
        var tracker = new TouchGestureTracker();
        var down1 = tracker.OnPointerDown(1, new Point(10, 10), 100);
        var down2 = tracker.OnPointerDown(2, new Point(50, 50), 120);
        bool requireAfterDown2 = tracker.RequireAllPointersUp;
        var up2 = tracker.OnPointerUp(2, new Point(50, 50), 200);
        bool requireAfterUp2 = tracker.RequireAllPointersUp;
        var moveRemaining = tracker.OnPointerMoved(1, new Point(20, 20), 220);
        var up1 = tracker.OnPointerUp(1, new Point(20, 20), 250);
        bool requireAfterAllUp = tracker.RequireAllPointersUp;
        var nextDown = tracker.OnPointerDown(3, new Point(30, 30), 300);

        Assert.Multiple(() =>
        {
            Assert.That(down1.CanStartSingleFingerAction, Is.True);
            Assert.That(down1.ShouldCancelSingleFingerAction, Is.False);
            Assert.That(down2.ShouldCancelSingleFingerAction, Is.True);
            Assert.That(down2.CanStartSingleFingerAction, Is.False);
            Assert.That(requireAfterDown2, Is.True);
            Assert.That(up2.ActivePointerCount, Is.EqualTo(1));
            Assert.That(requireAfterUp2, Is.True);
            Assert.That(moveRemaining.CanContinueSingleFingerAction, Is.False);
            Assert.That(up1.ActivePointerCount, Is.EqualTo(0));
            Assert.That(requireAfterAllUp, Is.False);
            Assert.That(nextDown.CanStartSingleFingerAction, Is.True);
        });
    }

    [Test]
    public void TwoFingerPinchAndPan_ShouldCalculateZoomStepAndPanDelta()
    {
        var tracker = new TouchGestureTracker();
        tracker.OnPointerDown(1, new Point(100, 100), 0);
        tracker.OnPointerDown(2, new Point(200, 100), 10);
        tracker.OnPointerMoved(1, new Point(90, 110), 50);
        var zoomInMove = tracker.OnPointerMoved(2, new Point(250, 110), 50);
        tracker.OnPointerMoved(1, new Point(130, 110), 100);
        var zoomOutMove = tracker.OnPointerMoved(2, new Point(210, 110), 100);

        Assert.Multiple(() =>
        {
            Assert.That(zoomInMove.IsMultiTouch, Is.True);
            Assert.That(zoomInMove.ZoomStepDelta, Is.EqualTo(1));
            Assert.That(zoomInMove.PinchCenter, Is.EqualTo(new Point(170, 110)));
            Assert.That(zoomInMove.PanDelta.Y, Is.GreaterThan(0));
            Assert.That(zoomOutMove.ZoomStepDelta, Is.EqualTo(-1));
        });
    }

    [TestCase(2, 200, 4.0, TouchTapAction.TwoFingerUndo)]
    [TestCase(3, 200, 4.0, TouchTapAction.ThreeFingerRedo)]
    [TestCase(2, 200, 20.0, TouchTapAction.None)]
    [TestCase(2, 500, 2.0, TouchTapAction.None)]
    public void MultiFingerTap_ShouldEvaluateUndoAndRedoCorrectly(int fingers, long durationMs, double moveDist, TouchTapAction expected)
    {
        var tracker = new TouchGestureTracker();
        for (int i = 1; i <= fingers; i++) tracker.OnPointerDown(i, new Point(i * 30, 50), 0);
        for (int i = 1; i <= fingers; i++) tracker.OnPointerMoved(i, new Point(i * 30 + moveDist, 50), durationMs / 2);

        TouchUpResult lastUp = default;
        for (int i = 1; i <= fingers; i++) lastUp = tracker.OnPointerUp(i, new Point(i * 30 + moveDist, 50), durationMs);
        Assert.That(lastUp.TapAction, Is.EqualTo(expected));
    }

    [Test]
    public void SingleFingerLongPress_ShouldTriggerWhenStationary_AndCancelWhenMovedBeyondTolerance()
    {
        var tracker = new TouchGestureTracker();
        tracker.OnPointerDown(1, new Point(40, 40), 0);
        tracker.OnPointerMoved(1, new Point(43, 44), 200);
        bool early = tracker.TryTriggerLongPress(300, out _);
        bool triggered = tracker.TryTriggerLongPress(420, out var pos);
        var moveAfterLongPress = tracker.OnPointerMoved(1, new Point(45, 45), 450);

        var trackerMoved = new TouchGestureTracker();
        trackerMoved.OnPointerDown(1, new Point(40, 40), 0);
        trackerMoved.OnPointerMoved(1, new Point(50, 40), 100);
        bool triggeredAfterMove = trackerMoved.TryTriggerLongPress(500, out _);

        Assert.Multiple(() =>
        {
            Assert.That(early, Is.False);
            Assert.That(triggered, Is.True);
            Assert.That(pos, Is.EqualTo(new Point(43, 44)));
            Assert.That(moveAfterLongPress.CanContinueSingleFingerAction, Is.False);
            Assert.That(trackerMoved.IsLongPressEligible, Is.False);
            Assert.That(triggeredAfterMove, Is.False);
        });
    }

    [Test]
    public void CaptureLostAndStaleTimeout_ShouldClearResidualPointers()
    {
        var tracker = new TouchGestureTracker();
        tracker.OnPointerDown(1, new Point(10, 10), 0);
        tracker.OnPointerDown(2, new Point(20, 20), 10);
        tracker.OnPointerCaptureLost(1);
        tracker.OnPointerCaptureLost(2);
        Assert.That(tracker.ActivePointerCount, Is.EqualTo(0));

        tracker.OnPointerDown(10, new Point(10, 10), 100);
        tracker.OnPointerDown(11, new Point(20, 20), 110);
        var freshDown = tracker.OnPointerDown(12, new Point(30, 30), 5000);
        Assert.Multiple(() =>
        {
            Assert.That(freshDown.ActivePointerCount, Is.EqualTo(1));
            Assert.That(freshDown.CanStartSingleFingerAction, Is.True);
        });
    }

    [AvaloniaTest]
    public void PaletteForm_And_ColorSampleButton_ShouldSupportTouchModesAndLongPress()
    {
        int registeredSlot = -1;
        var paletteForm = new PaletteForm
        {
            IsRegisterMode = true,
            PointerRightButtonPressedCommand = ReactiveCommand.Create<int>(slot => registeredSlot = slot)
        };
        var touchPointer = new Avalonia.Input.Pointer(1, PointerType.Touch, true);
        paletteForm.HandleTouchPressSlot(3, new Point(55, 5), touchPointer);
        Assert.That(registeredSlot, Is.EqualTo(3));

        registeredSlot = -1;
        paletteForm.IsRegisterMode = false;
        paletteForm.HandleTouchPressSlot(5, new Point(90, 5), touchPointer);
        Assert.That(paletteForm.TriggerTouchLongPress(), Is.True);
        Assert.That(registeredSlot, Is.EqualTo(5));

        int putCount = 0, getCount = 0;
        var sampleBtn = new ColorSampleButton
        {
            PutCommand = ReactiveCommand.Create(() => { putCount++; }),
            GetCommand = ReactiveCommand.Create(() => { getCount++; }),
            TouchCanvasMode = TouchCanvasMode.Eyedropper
        };
        sampleBtn.HandleTouchPressed();
        Assert.That(getCount, Is.EqualTo(1));

        sampleBtn.TouchCanvasMode = TouchCanvasMode.Draw;
        sampleBtn.HandleTouchPressed();
        Assert.That(sampleBtn.TriggerTouchLongPress(), Is.True);
        sampleBtn.HandleTouchReleased();
        Assert.Multiple(() => { Assert.That(getCount, Is.EqualTo(2)); Assert.That(putCount, Is.EqualTo(0)); });
    }
}
