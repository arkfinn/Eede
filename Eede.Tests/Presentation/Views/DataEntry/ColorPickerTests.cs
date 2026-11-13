using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Eede.Domain.Palettes;
using Eede.Presentation.Views.DataEntry;
using NUnit.Framework;
using System.Linq;

namespace Eede.Presentation.Tests.Views.DataEntry;

#nullable enable

[TestFixture]
public class ColorPickerTests
{
    private static Window ShowInWindow(Control control)
    {
        var window = new Window
        {
            Content = control,
            Width = 300,
            Height = 400
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static void ClickControl(Window window, Control target, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        var center = new Point(target.Bounds.Width / 2.0, target.Bounds.Height / 2.0);
        var windowPoint = target.TranslatePoint(center, window);
        Assert.That(windowPoint, Is.Not.Null);
        window.MouseMove(windowPoint!.Value, modifiers);
        window.MouseDown(windowPoint.Value, MouseButton.Left, modifiers);
        window.MouseUp(windowPoint.Value, MouseButton.Left, modifiers);
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTest]
    public void DepthModeToggle_SwitchesRangeAndMaps16StepColors()
    {
        var picker = new ColorPicker
        {
            NowColor = new ArgbColor(255, 255, 136, 0)
        };
        var window = ShowInWindow(picker);
        try
        {
            var depthButton = picker.FindControl<Button>("DepthModeButton");
            var depthText = picker.FindControl<TextBlock>("depthModeText");
            var pickerR = picker.FindControl<GradationPicker>("pickerR");
            var pickerG = picker.FindControl<GradationPicker>("pickerG");

            Assert.That(depthButton, Is.Not.Null);
            Assert.That(depthText?.Text, Is.EqualTo("256"));
            Assert.That(picker.DepthMode, Is.EqualTo(ColorDepthMode.Depth256));
            Assert.That(pickerR?.MaxValue, Is.EqualTo(255));
            Assert.That(pickerR?.LargeChange, Is.EqualTo(16));

            ClickControl(window, depthButton!);

            Assert.Multiple(() =>
            {
                Assert.That(picker.DepthMode, Is.EqualTo(ColorDepthMode.Depth16));
                Assert.That(depthText?.Text, Is.EqualTo("16"));
                Assert.That(pickerR?.MaxValue, Is.EqualTo(15));
                Assert.That(pickerR?.LargeChange, Is.EqualTo(1));
                Assert.That(pickerR?.Value, Is.EqualTo(15));
                Assert.That(pickerG?.Value, Is.EqualTo(8));
            });

            // 16段階モードで値を15に設定すると255に写像されること
            pickerG!.Value = 15;
            Dispatcher.UIThread.RunJobs();
            Assert.That(picker.NowColor.Green, Is.EqualTo(255));

            // 256段階モードへ戻せること
            ClickControl(window, depthButton!);
            Assert.Multiple(() =>
            {
                Assert.That(picker.DepthMode, Is.EqualTo(ColorDepthMode.Depth256));
                Assert.That(depthText?.Text, Is.EqualTo("256"));
                Assert.That(pickerR?.MaxValue, Is.EqualTo(255));
                Assert.That(pickerR?.LargeChange, Is.EqualTo(16));
            });
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void StepToneButtons_ShiftRgbUniformlyWithoutChangingAlpha()
    {
        var picker = new ColorPicker
        {
            NowColor = new ArgbColor(180, 100, 150, 200)
        };
        var window = ShowInWindow(picker);
        try
        {
            var stepUp = picker.FindControl<Button>("StepUpButton");
            var stepDown = picker.FindControl<Button>("StepDownButton");
            Assert.That(stepUp, Is.Not.Null);
            Assert.That(stepDown, Is.Not.Null);

            ClickControl(window, stepUp!);
            Assert.That(picker.NowColor, Is.EqualTo(new ArgbColor(180, 116, 166, 216)));

            ClickControl(window, stepDown!);
            Assert.That(picker.NowColor, Is.EqualTo(new ArgbColor(180, 100, 150, 200)));

            // 16段階モード時は1ステップ(17相当)ずつ増減
            var depthButton = picker.FindControl<Button>("DepthModeButton");
            ClickControl(window, depthButton!);

            picker.NowColor = new ArgbColor(255, 85, 170, 255); // steps 5, 10, 15
            Dispatcher.UIThread.RunJobs();

            ClickControl(window, stepDown!);
            Assert.That(picker.NowColor, Is.EqualTo(new ArgbColor(255, 68, 153, 238))); // steps 4, 9, 14
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void StepToneButtons_InHsvMode_PreserveHueWhenVReachesZeroAndStepsBackUp()
    {
        var picker = new ColorPicker
        {
            NowColor = new ArgbColor(255, 0, 255, 0) // Pure Green: H=120, S=255, V=255
        };
        var window = ShowInWindow(picker);
        try
        {
            var modeText = picker.FindControl<TextBlock>("text1");
            var modeButton = modeText?.FindAncestorOfType<Button>();
            var pickerR = picker.FindControl<GradationPicker>("pickerR");
            var pickerB = picker.FindControl<GradationPicker>("pickerB");
            var stepDown = picker.FindControl<Button>("StepDownButton");
            var stepUp = picker.FindControl<Button>("StepUpButton");

            Assert.That(modeButton, Is.Not.Null);
            ClickControl(window, modeButton!);

            Assert.That(modeText?.Text, Is.EqualTo("H"));
            Assert.That(pickerR?.Value, Is.EqualTo(120));

            // Vを16にしてからStepDownで0へ落とし、再びStepUpで16へ戻してもHue=120が壊れないこと
            pickerB!.Value = 16;
            Dispatcher.UIThread.RunJobs();
            Assert.That(pickerR?.Value, Is.EqualTo(120));

            ClickControl(window, stepDown!);
            Assert.Multiple(() =>
            {
                Assert.That(picker.NowColor, Is.EqualTo(new ArgbColor(255, 0, 0, 0)));
                Assert.That(pickerB.Value, Is.EqualTo(0));
                Assert.That(pickerR?.Value, Is.EqualTo(120));
            });

            ClickControl(window, stepUp!);
            Assert.Multiple(() =>
            {
                Assert.That(pickerB.Value, Is.EqualTo(16));
                Assert.That(pickerR?.Value, Is.EqualTo(120));
                Assert.That(picker.NowColor.Green, Is.GreaterThan(0));
                Assert.That(picker.NowColor.Red, Is.EqualTo(0));
                Assert.That(picker.NowColor.Blue, Is.EqualTo(0));
            });
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void GradationSlider_TrackClickMovesByLargeChange_AndThumbDragReachesMax15()
    {
        var slider = new GradationSlider
        {
            Width = 24,
            Height = 88,
            MinValue = 0,
            MaxValue = 15,
            LargeChange = 1,
            Value = 0
        };
        var window = ShowInWindow(slider);
        try
        {
            // Value=0 (bottom), click near top track (Y=10) -> increments by LargeChange (+1)
            var topTrack = slider.TranslatePoint(new Point(12, 10), window)!.Value;
            window.MouseMove(topTrack);
            window.MouseDown(topTrack, MouseButton.Left);
            window.MouseUp(topTrack, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.That(slider.Value, Is.EqualTo(1));

            // Set Value=10, click below thumb (Y=80) -> decrements by LargeChange (-1)
            slider.Value = 10;
            Dispatcher.UIThread.RunJobs();
            var bottomTrack = slider.TranslatePoint(new Point(12, 80), window)!.Value;
            window.MouseMove(bottomTrack);
            window.MouseDown(bottomTrack, MouseButton.Left);
            window.MouseUp(bottomTrack, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.That(slider.Value, Is.EqualTo(9));

            // Drag starting inside thumb or long drag (> 4px) to top edge -> reaches MaxValue (15)
            var midTrack = slider.TranslatePoint(new Point(12, 44), window)!.Value;
            var topEdge = slider.TranslatePoint(new Point(12, 0), window)!.Value;
            window.MouseMove(midTrack);
            window.MouseDown(midTrack, MouseButton.Left);
            window.MouseMove(topEdge);
            window.MouseUp(topEdge, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            Assert.That(slider.Value, Is.EqualTo(15));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void GradationPicker_ShiftClickArrow_MovesByLargeChange()
    {
        var gp = new GradationPicker
        {
            MaxValue = 255,
            LargeChange = 16,
            Value = 10
        };
        var window = ShowInWindow(gp);
        try
        {
            var buttons = gp.GetVisualDescendants().OfType<RepeatButton>().ToList();
            Assert.That(buttons.Count, Is.EqualTo(2));

            ClickControl(window, buttons[0], RawInputModifiers.Shift);
            Assert.That(gp.Value, Is.EqualTo(26));

            ClickControl(window, buttons[1], RawInputModifiers.Shift);
            Assert.That(gp.Value, Is.EqualTo(10));
        }
        finally
        {
            window.Close();
        }
    }
}
