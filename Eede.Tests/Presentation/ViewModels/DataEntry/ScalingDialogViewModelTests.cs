using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.ViewModels.DataEntry;
using NUnit.Framework;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace Eede.Presentation.Tests.ViewModels.DataEntry;

#nullable enable

[TestFixture]
public class ScalingDialogViewModelTests
{
    private PictureSize originalSize = default!;

    [SetUp]
    public void SetUp()
    {
        originalSize = new PictureSize(100, 200);
    }

    [Test]
    public void InitialValuesTest()
    {
        var vm = new ScalingDialogViewModel(originalSize);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Width, Is.EqualTo(100));
            Assert.That(vm.Height, Is.EqualTo(200));
            Assert.That(vm.WidthPercent, Is.EqualTo(100.0));
            Assert.That(vm.HeightPercent, Is.EqualTo(100.0));
            Assert.That(vm.IsLockAspectRatio, Is.True);
        });
    }

    [Test]
    public void WidthChangeUpdatesEverythingWhenLocked()
    {
        var vm = new ScalingDialogViewModel(originalSize) { IsLockAspectRatio = true };

        vm.Width = 200;

        Assert.Multiple(() =>
        {
            Assert.That(vm.Height, Is.EqualTo(400), "Height should be updated by aspect ratio");
            Assert.That(vm.WidthPercent, Is.EqualTo(200.0), "WidthPercent should be updated");
            Assert.That(vm.HeightPercent, Is.EqualTo(200.0), "HeightPercent should be updated");
        });
    }

    [Test]
    public void WidthPercentChangeUpdatesEverythingWhenLocked()
    {
        var vm = new ScalingDialogViewModel(originalSize) { IsLockAspectRatio = true };

        vm.WidthPercent = 50.0;

        Assert.Multiple(() =>
        {
            Assert.That(vm.Width, Is.EqualTo(50), "Width should be updated");
            Assert.That(vm.Height, Is.EqualTo(100), "Height should be updated");
            Assert.That(vm.HeightPercent, Is.EqualTo(50.0), "HeightPercent should match WidthPercent when locked");
        });
    }

    [Test]
    public void HeightChangeUpdatesEverythingWhenLocked()
    {
        var vm = new ScalingDialogViewModel(originalSize) { IsLockAspectRatio = true };

        vm.Height = 100;

        Assert.Multiple(() =>
        {
            Assert.That(vm.Width, Is.EqualTo(50));
            Assert.That(vm.WidthPercent, Is.EqualTo(50.0));
            Assert.That(vm.HeightPercent, Is.EqualTo(50.0));
        });
    }

    [Test]
    public void IndependentChangeWhenNotLocked()
    {
        var vm = new ScalingDialogViewModel(originalSize) { IsLockAspectRatio = false };

        vm.Width = 300;

        Assert.Multiple(() =>
        {
            Assert.That(vm.Height, Is.EqualTo(200), "Height should stay the same");
            Assert.That(vm.WidthPercent, Is.EqualTo(300.0));
            Assert.That(vm.HeightPercent, Is.EqualTo(100.0));
        });
    }

    [Test]
    public void PresetButtonsUpdateEverything()
    {
        var vm = new ScalingDialogViewModel(originalSize) { IsLockAspectRatio = true };

        vm.ApplyPreset.Execute(3.0).Subscribe();

        Assert.Multiple(() =>
        {
            Assert.That(vm.WidthPercent, Is.EqualTo(300.0));
            Assert.That(vm.HeightPercent, Is.EqualTo(300.0));
            Assert.That(vm.Width, Is.EqualTo(300));
            Assert.That(vm.Height, Is.EqualTo(600));
        });
    }

    [Test]
    public void AnchorSelectionUpdatesAlignment()
    {
        var vm = new ScalingDialogViewModel(originalSize);
        // 縮小状態にする
        vm.IsLockAspectRatio = false;
        vm.Width = 50;
        vm.Height = 100;

        vm.SetAnchorCommand.Execute("BottomRight").Subscribe();

        Assert.Multiple(() =>
        {
            Assert.That(vm.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Right));
            Assert.That(vm.VerticalAlignment, Is.EqualTo(VerticalAlignment.Bottom));
        });
    }

    [Test]
    public void EnlargementResetsAnchorToTopLeft()
    {
        var vm = new ScalingDialogViewModel(originalSize);
        // 縮小してアンカーを変更
        vm.IsLockAspectRatio = false;
        vm.Width = 50;
        vm.Height = 100;
        vm.SetAnchorCommand.Execute("BottomRight").Subscribe();

        // 拡大に戻す
        vm.Width = 150;
        vm.Height = 250;

        Assert.Multiple(() =>
        {
            Assert.That(vm.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Left), "Horizontal should reset to Left on enlargement");
            Assert.That(vm.VerticalAlignment, Is.EqualTo(VerticalAlignment.Top), "Vertical should reset to Top on enlargement");
            Assert.That(vm.IsHorizontalShrink, Is.False, "Width 150 is NOT shrink for 100");
            Assert.That(vm.IsVerticalShrink, Is.False, "Height 250 is NOT shrink for 200");
        });
    }

    [Test]
    public void PartialEnlargementControlTest()
    {
        var vm = new ScalingDialogViewModel(originalSize);
        vm.IsLockAspectRatio = false;
        // 幅だけ拡大、高さは縮小
        vm.Width = 150;
        vm.Height = 50;

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsHorizontalShrink, Is.False, "150 < 100 is False");
            Assert.That(vm.IsVerticalShrink, Is.True, "50 < 200 is True");
            Assert.That(vm.IsTopCenterEnabled, Is.False, "Horizontal center should be disabled when width is enlarged");
            Assert.That(vm.IsCenterLeftEnabled, Is.True, "Vertical center should be enabled when height is shrunken");
            Assert.That(vm.IsCenterCenterEnabled, Is.False, "CenterCenter should be disabled when width is enlarged");
        });
    }

    [Test]
    public void MinimumValueEnforcementTest()
    {
        var vm = new ScalingDialogViewModel(originalSize);

        vm.WidthPercent = 0;
        Assert.Multiple(() =>
        {
            Assert.That(vm.Width, Is.GreaterThanOrEqualTo(1), "Width should be at least 1");
            Assert.That(vm.WidthPercent, Is.GreaterThan(0), "WidthPercent should be greater than 0");
        });

        vm.Width = -10;
        Assert.That(vm.Width, Is.GreaterThanOrEqualTo(1), "Width should be clamped to 1");

        vm.Height = -5;
        Assert.That(vm.Height, Is.GreaterThanOrEqualTo(1), "Height should be clamped to 1");

        vm.HeightPercent = -1.0;
        Assert.That(vm.HeightPercent, Is.GreaterThan(0), "HeightPercent should be greater than 0");
    }

    [Test]
    public void OkCommandGeneratesCorrectResizeContext()
    {
        var vm = new ScalingDialogViewModel(originalSize);
        vm.IsLockAspectRatio = false;
        vm.Width = 150;
        vm.Height = 300;
        vm.SetAnchorCommand.Execute("TopLeft").Subscribe(); // 拡大時はTopLeftのみ有効

        ResizeContext? result = null;
        vm.OkCommand.Subscribe(ctx => result = ctx);
        vm.OkCommand.Execute().Subscribe();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.TargetSize.Width, Is.EqualTo(150));
            Assert.That(result!.TargetSize.Height, Is.EqualTo(300));
            Assert.That(result!.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Left));
            Assert.That(result!.VerticalAlignment, Is.EqualTo(VerticalAlignment.Top));
        });
    }
}
