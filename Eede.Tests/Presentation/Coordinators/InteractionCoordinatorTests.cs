using Eede.Application.Drawings;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Coordinators;
using NUnit.Framework;
using System;
using System.Reactive;
using Avalonia.Headless.NUnit;
using ReactiveUI;

namespace Eede.Tests.Presentation.Coordinators;

#nullable enable

[TestFixture]
public class InteractionCoordinatorTests
{
    private DrawingSessionProvider _sessionProvider = default!;
    private InteractionCoordinator _coordinator = default!;

    [SetUp]
    public void SetUp()
    {
        _sessionProvider = new DrawingSessionProvider();
        _coordinator = new InteractionCoordinator(_sessionProvider);
    }

    [AvaloniaTest]
    public void PointerRightButtonPressed_ShouldPickColorWithAlpha()
    {
        // 1. アルファ値を含む画像データを作成
        var size = new PictureSize(16, 16);
        var expectedColor = new ArgbColor(128, 255, 0, 0); // 半透明の赤
        var imageData = new byte[size.Width * size.Height * 4];

        // (5, 5) の位置に色を書き込む (Bgra8888)
        int index = (5 * 4) + (size.Width * 4 * 5);
        imageData[index + 0] = expectedColor.Blue;
        imageData[index + 1] = expectedColor.Green;
        imageData[index + 2] = expectedColor.Red;
        imageData[index + 3] = expectedColor.Alpha;

        var picture = Picture.Create(size, imageData);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        // 2. 右クリック実行
        ArgbColor? pickedColor = null;
        var pos = new Position(5, 5);
        _coordinator.PointerRightButtonPressed(
            pos,
            new DrawingBuffer(picture),
            new FreeCurve(),
            false,
            new PictureSize(16, 16),
            (color) => pickedColor = color,
            null!);

        // 3. 検証
        Assert.That(pickedColor, Is.Not.Null, "Color should be picked");
        Assert.That(pickedColor!.Value.Alpha, Is.EqualTo(128), "Alpha value should be picked correctly");
        Assert.That(pickedColor!.Value.Red, Is.EqualTo(255), "Red value should be picked correctly");
    }

    [AvaloniaTest]
    public void PointerRightButtonPressed_ShouldPickColorEvenWhenCancellingDraw()
    {
        // 描画中に右クリックした場合でも色が拾えることを確認
        var size = new PictureSize(16, 16);
        var expectedColor = new ArgbColor(200, 0, 255, 0); // 半透明の緑
        var imageData = new byte[size.Width * size.Height * 4];
        int index = (10 * 4) + (size.Width * 4 * 10);
        imageData[index + 0] = expectedColor.Blue;
        imageData[index + 1] = expectedColor.Green;
        imageData[index + 2] = expectedColor.Red;
        imageData[index + 3] = expectedColor.Alpha;

        var picture = Picture.Create(size, imageData);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        // 1. 描画開始 (左クリック相当)
        var buffer = new DrawingBuffer(picture);
        _coordinator.PointerBegin(new Position(0, 0), buffer, new FreeCurve(), new PenStyle(new DirectImageBlender()), false, false, new PictureSize(16, 16), null!);

        // 2. 描画中に右クリック
        ArgbColor? pickedColor = null;
        _coordinator.PointerRightButtonPressed(
            new Position(10, 10),
            _coordinator.CurrentBuffer!,
            new FreeCurve(),
            false,
            new PictureSize(16, 16),
            (color) => pickedColor = color,
            null!);

        // 3. 検証
        Assert.That(pickedColor, Is.Not.Null, "Color should be picked even during drawing cancellation");
        Assert.That(pickedColor!.Value.Alpha, Is.EqualTo(200));
        Assert.That(pickedColor.Value.Green, Is.EqualTo(255));
    }

    [AvaloniaTest]
    public void Painted_WhenRegionSelectTool_ShouldNotRenderCursorLayer()
    {
        var size = new PictureSize(16, 16);
        var baseColor = new ArgbColor(255, 0, 0, 0); // 黒背景
        var pixels = new byte[size.Width * size.Height * 4];
        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = baseColor.Blue;
            pixels[i + 1] = baseColor.Green;
            pixels[i + 2] = baseColor.Red;
            pixels[i + 3] = baseColor.Alpha;
        }
        var picture = Picture.Create(size, pixels);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        var penColor = new ArgbColor(255, 255, 0, 0); // 赤ペン
        var penStyle = new PenStyle(new DirectImageBlender(), penColor, 1);
        var transfer = new DirectImageTransfer();
        var buffer = new DrawingBuffer(picture);

        // 1. 通常の描画ツールのとき: ホバー位置にペンの赤色が描画される
        _coordinator.PointerMoved(new Position(5, 5), buffer, new FreeCurve(), penStyle, false, false, new PictureSize(16, 16));
        var paintedFreeCurve = _coordinator.Painted(buffer, penStyle, transfer);
        Assert.That(paintedFreeCurve, Is.Not.Null);
        var pixelAtPos = paintedFreeCurve!.PickColor(new Position(5, 5));
        Assert.That(pixelAtPos.Red, Is.EqualTo(255), "Draw tool should preview pen color on cursor position");

        // 2. 範囲選択ツールのとき: ホバー位置でもペンの赤色は描画されず、元の黒のまま
        var regionSelector = new RegionSelector();
        _coordinator.ChangeDrawStyle(regionSelector);
        _coordinator.PointerMoved(new Position(5, 5), buffer, regionSelector, penStyle, false, false, new PictureSize(16, 16));
        var paintedRegionSelect = _coordinator.Painted(buffer, penStyle, transfer);
        Assert.That(paintedRegionSelect, Is.Not.Null);
        var pixelAtPosRegion = paintedRegionSelect!.PickColor(new Position(5, 5));
        Assert.That(pixelAtPosRegion.Red, Is.EqualTo(0), "RegionSelector should NOT preview pen color on canvas");
    }

    [AvaloniaTest]
    public void HoverCursor_WhenRegionSelectTool_ShouldUpdateVisibilityAndCursorType()
    {
        var size = new PictureSize(16, 16);
        var picture = Picture.CreateEmpty(size);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();
        var buffer = new DrawingBuffer(picture);
        var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
        var regionSelector = new RegionSelector();

        // 1. 描画ツールのホバー時: IsHoverCursorVisible は false, カーソルは Arrow
        _coordinator.PointerMoved(new Position(4, 4), buffer, new FreeCurve(), penStyle, false, false, new PictureSize(16, 16));
        Assert.That(_coordinator.IsHoverCursorVisible, Is.False);
        Assert.That(_coordinator.ActiveCursor, Is.EqualTo(Avalonia.Input.StandardCursorType.Arrow));

        // 2. 範囲選択ツールのホバー時: IsHoverCursorVisible は true, HoverPosition が一致, カーソルは Cross
        _coordinator.ChangeDrawStyle(regionSelector);
        _coordinator.PointerMoved(new Position(4, 4), buffer, regionSelector, penStyle, false, false, new PictureSize(16, 16));
        Assert.That(_coordinator.IsHoverCursorVisible, Is.True);
        Assert.That(_coordinator.HoverPosition, Is.EqualTo(new Position(4, 4)));
        Assert.That(_coordinator.ActiveCursor, Is.EqualTo(Avalonia.Input.StandardCursorType.Cross));

        // 3. キャンバス外に出た時: IsHoverCursorVisible は false, HoverPosition は null
        _coordinator.CanvasLeave(buffer);
        Assert.That(_coordinator.IsHoverCursorVisible, Is.False);
        Assert.That(_coordinator.HoverPosition, Is.Null);
    }

    [AvaloniaTest]
    public void PointerRightButtonPressed_WhenRegionSelectorAndUnselected_ShouldDoNothingAndNotPickColor()
    {
        var size = new PictureSize(32, 32);
        var picture = Picture.CreateEmpty(size);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        var regionSelector = new RegionSelector();
        _coordinator.SetupRegionSelector(regionSelector, new DrawingBuffer(picture), false, new PictureSize(16, 16));
        _coordinator.ChangeDrawStyle(regionSelector);

        var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
        ArgbColor? pickedColor = null;

        _coordinator.PointerRightButtonPressed(
            new Position(4, 4),
            _coordinator.CurrentBuffer!,
            regionSelector,
            false,
            new PictureSize(16, 16),
            color => pickedColor = color,
            null!);
        _coordinator.PointerMoved(new Position(20, 20), _coordinator.CurrentBuffer!, regionSelector, penStyle, false, false, new PictureSize(16, 16));

        Assert.Multiple(() =>
        {
            Assert.That(pickedColor, Is.Null);
            Assert.That(_coordinator.SelectingArea, Is.Null);
            Assert.That(_coordinator.IsRegionSelecting, Is.False);
            Assert.That(_coordinator.IsShowHandles, Is.False);
        });
    }

    [AvaloniaTest]
    public void PointerRightButtonPressed_WhenDraggingNewRegion_ShouldCancelAndNotPickColor()
    {
        var size = new PictureSize(32, 32);
        var picture = Picture.CreateEmpty(size);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        var regionSelector = new RegionSelector();
        var gridSize = new PictureSize(16, 16);
        _coordinator.SetupRegionSelector(regionSelector, new DrawingBuffer(picture), false, gridSize);
        _coordinator.ChangeDrawStyle(regionSelector);

        var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
        _coordinator.PointerBegin(new Position(2, 2), _coordinator.CurrentBuffer!, regionSelector, penStyle, false, false, gridSize, null!);
        _coordinator.PointerMoved(new Position(10, 10), _coordinator.CurrentBuffer!, regionSelector, penStyle, false, false, gridSize);
        Assert.That(_coordinator.IsRegionSelecting, Is.True);

        ArgbColor? pickedColor = null;
        _coordinator.PointerRightButtonPressed(
            new Position(10, 10),
            _coordinator.CurrentBuffer!,
            regionSelector,
            false,
            gridSize,
            color => pickedColor = color,
            null!);

        Assert.Multiple(() =>
        {
            Assert.That(pickedColor, Is.Null);
            Assert.That(_coordinator.CurrentBuffer!.IsDrawing(), Is.False);
            Assert.That(_coordinator.SelectingArea, Is.Null);
            Assert.That(_coordinator.IsRegionSelecting, Is.False);
        });
    }

    [AvaloniaTest]
    public void PointerRightButtonPressed_WhenRegionSelected_ShouldClearSelectionAndNotPickColor()
    {
        var size = new PictureSize(32, 32);
        var picture = Picture.CreateEmpty(size);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        var regionSelector = new RegionSelector();
        var gridSize = new PictureSize(16, 16);
        _coordinator.SetupRegionSelector(regionSelector, new DrawingBuffer(picture), false, gridSize);
        _coordinator.ChangeDrawStyle(regionSelector);

        var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
        _coordinator.PointerBegin(new Position(2, 2), _coordinator.CurrentBuffer!, regionSelector, penStyle, false, false, gridSize, null!);
        _coordinator.PointerMoved(new Position(10, 10), _coordinator.CurrentBuffer!, regionSelector, penStyle, false, false, gridSize);
        _coordinator.PointerLeftButtonReleased(new Position(10, 10), _coordinator.CurrentBuffer!, regionSelector, false, gridSize, penStyle, false, null!);
        Assert.That(_coordinator.IsShowHandles, Is.True);
        Assert.That(_coordinator.SelectingArea, Is.Not.Null);

        ArgbColor? pickedColor = null;
        _coordinator.PointerRightButtonPressed(
            new Position(20, 20),
            _coordinator.CurrentBuffer!,
            regionSelector,
            false,
            gridSize,
            color => pickedColor = color,
            null!);

        Assert.Multiple(() =>
        {
            Assert.That(pickedColor, Is.Null);
            Assert.That(_coordinator.SelectingArea, Is.Null);
            Assert.That(_coordinator.IsRegionSelecting, Is.False);
            Assert.That(_coordinator.IsShowHandles, Is.False);
        });
    }

    [AvaloniaTest]
    public void PointerRightButtonPressed_WhenDraggingSelection_ShouldCancelMoveAndNotPickColor()
    {
        var size = new PictureSize(32, 32);
        var picture = Picture.CreateEmpty(size);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        var regionSelector = new RegionSelector();
        var gridSize = new PictureSize(16, 16);
        _coordinator.SetupRegionSelector(regionSelector, new DrawingBuffer(picture), false, gridSize);
        _coordinator.ChangeDrawStyle(regionSelector);

        var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);
        _coordinator.PointerBegin(new Position(0, 0), _coordinator.CurrentBuffer!, regionSelector, penStyle, false, false, gridSize, null!);
        _coordinator.PointerMoved(new Position(8, 8), _coordinator.CurrentBuffer!, regionSelector, penStyle, false, false, gridSize);
        _coordinator.PointerLeftButtonReleased(new Position(8, 8), _coordinator.CurrentBuffer!, regionSelector, false, gridSize, penStyle, false, null!);

        // 選択範囲内を左ドラッグ開始
        _coordinator.PointerBegin(new Position(4, 4), _coordinator.CurrentBuffer!, regionSelector, penStyle, false, false, gridSize, null!);
        _coordinator.PointerMoved(new Position(12, 12), _coordinator.CurrentBuffer!, regionSelector, penStyle, false, false, gridSize);
        Assert.That(_coordinator.PreviewPixels, Is.Not.Null);

        ArgbColor? pickedColor = null;
        _coordinator.PointerRightButtonPressed(
            new Position(12, 12),
            _coordinator.CurrentBuffer!,
            regionSelector,
            false,
            gridSize,
            color => pickedColor = color,
            null!);

        Assert.Multiple(() =>
        {
            Assert.That(pickedColor, Is.Null);
            Assert.That(_coordinator.PreviewPixels, Is.Null);
            Assert.That(_coordinator.SelectingArea, Is.Null);
            Assert.That(_coordinator.IsShowHandles, Is.False);
        });
    }

    [AvaloniaTest]
    public void PointerRightButtonPressed_WithFreeCurveThenSwitchingToRegionSelector_ShouldNotLeaveGhostSelection()
    {
        var size = new PictureSize(32, 32);
        var picture = Picture.CreateEmpty(size);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        var freeCurve = new FreeCurve();
        var regionSelector = new RegionSelector();
        var gridSize = new PictureSize(16, 16);
        var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);

        _coordinator.PointerRightButtonPressed(
            new Position(4, 4),
            _coordinator.CurrentBuffer!,
            freeCurve,
            false,
            gridSize,
            _ => { },
            null!);

        _coordinator.ChangeDrawStyle(regionSelector);
        _coordinator.PointerMoved(new Position(16, 16), _coordinator.CurrentBuffer!, regionSelector, penStyle, false, false, gridSize);

        Assert.Multiple(() =>
        {
            Assert.That(_coordinator.SelectingArea, Is.Null);
            Assert.That(_coordinator.IsRegionSelecting, Is.False);
        });
    }

    [AvaloniaTest]
    public void PointerRightButtonPressed_WhenOutOfBounds_ShouldNotThrowAndShouldCancelDrawingSafely()
    {
        var size = new PictureSize(16, 16);
        var picture = Picture.CreateEmpty(size);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        var freeCurve = new FreeCurve();
        var penStyle = new PenStyle(new DirectImageBlender(), new ArgbColor(255, 255, 0, 0), 1);

        // 左ドラッグ中にキャンバス外へ出て右クリックキャンセルした場合
        _coordinator.PointerBegin(new Position(2, 2), _coordinator.CurrentBuffer!, freeCurve, penStyle, false, false, size, null!);
        Assert.That(_coordinator.CurrentBuffer!.IsDrawing(), Is.True);

        ArgbColor? pickedColor = null;
        Assert.DoesNotThrow(() => _coordinator.PointerRightButtonPressed(
            new Position(16, 16),
            _coordinator.CurrentBuffer!,
            freeCurve,
            false,
            size,
            color => pickedColor = color,
            null!));

        Assert.Multiple(() =>
        {
            Assert.That(_coordinator.CurrentBuffer!.IsDrawing(), Is.False);
            Assert.That(pickedColor, Is.Null);
        });

        // 負座標での右クリックも例外にならないこと
        Assert.DoesNotThrow(() => _coordinator.PointerRightButtonPressed(
            new Position(-5, -5),
            _coordinator.CurrentBuffer!,
            freeCurve,
            false,
            size,
            color => pickedColor = color,
            null!));
        Assert.That(pickedColor, Is.Null);
    }

    [AvaloniaTest]
    public void CancelDrawing_WhenDrawingInProgress_ShouldRestoreBufferWithoutPickingColor()
    {
        var size = new PictureSize(16, 16);
        var originalColor = new ArgbColor(255, 10, 20, 30);
        var picture = Picture.CreateEmpty(size);
        var blender = new DirectImageBlender();
        picture = new Drawer(picture, new PenStyle(blender, originalColor, 1)).DrawPoint(new Position(2, 2)).Picture;

        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        var freeCurve = new FreeCurve();
        var drawPen = new PenStyle(blender, new ArgbColor(255, 255, 0, 0), 1);

        _coordinator.PointerBegin(new Position(2, 2), _coordinator.CurrentBuffer!, freeCurve, drawPen, false, false, size, null!);
        _coordinator.PointerMoved(new Position(3, 2), _coordinator.CurrentBuffer!, freeCurve, drawPen, false, false, size);
        Assert.That(_coordinator.CurrentBuffer!.IsDrawing(), Is.True);

        _coordinator.CancelDrawing();

        Assert.Multiple(() =>
        {
            Assert.That(_coordinator.CurrentBuffer!.IsDrawing(), Is.False);
            Assert.That(_coordinator.CurrentBuffer.Fetch().PickColor(new Position(2, 2)), Is.EqualTo(originalColor));
            Assert.That(_coordinator.CurrentBuffer.Fetch().PickColor(new Position(3, 2)).Alpha, Is.EqualTo(0));
        });
    }
}
