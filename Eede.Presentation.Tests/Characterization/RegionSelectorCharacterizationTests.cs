using Avalonia.Headless.NUnit;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Pictures;
using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.DataEntry;
using Moq;
using NUnit.Framework;
using System;

namespace Eede.Presentation.Tests.Characterization;

[TestFixture]
public class RegionSelectorCharacterizationTests
{
    private GlobalState _globalState;
    private Mock<IAddFrameProvider> _addFrameProviderMock;
    private Mock<IClipboard> _clipboardServiceMock;
    private AvaloniaBitmapAdapter _bitmapAdapter;
    private DrawingSessionProvider _drawingSessionProvider;
    private ISelectionService _selectionService;
    private InteractionCoordinator _coordinator;

    [SetUp]
    public void SetUp()
    {
        _globalState = new GlobalState();
        _addFrameProviderMock = new Mock<IAddFrameProvider>();
        _clipboardServiceMock = new Mock<IClipboard>();
        _bitmapAdapter = new AvaloniaBitmapAdapter();
        _drawingSessionProvider = new DrawingSessionProvider();
        _selectionService = new SelectionService(
            new CopySelectionUseCase(_clipboardServiceMock.Object),
            new CutSelectionUseCase(_clipboardServiceMock.Object),
            new PasteFromClipboardUseCase(_clipboardServiceMock.Object, _drawingSessionProvider));
        _coordinator = new InteractionCoordinator(_drawingSessionProvider);
    }

    private DrawableCanvasViewModel CreateViewModel(Picture initialPicture)
    {
        _drawingSessionProvider.Update(new DrawingSession(initialPicture));
        var vm = new DrawableCanvasViewModel(
            _globalState,
            _addFrameProviderMock.Object,
            _clipboardServiceMock.Object,
            _bitmapAdapter,
            _drawingSessionProvider,
            _selectionService,
            _coordinator);
        return vm;
    }

    private Picture CreateFilledPicture(PictureSize size, ArgbColor color)
    {
        byte[] data = new byte[size.Width * size.Height * 4];
        for (int i = 0; i < data.Length; i += 4)
        {
            data[i] = color.Blue;
            data[i + 1] = color.Green;
            data[i + 2] = color.Red;
            data[i + 3] = color.Alpha;
        }
        return Picture.Create(size, data);
    }

    /// <summary>
    /// ケース1: 何もない状態からドラッグ開始 -> 新しい選択範囲が作成される
    /// </summary>
    [AvaloniaTest]
    public void Select_Tool_And_Drag_Creates_New_Selection()
    {
        var vm = CreateViewModel(Picture.CreateEmpty(new PictureSize(32, 32)));
        vm.Magnification = new Magnification(1);
        vm.DrawStyle = new RegionSelector();

        // 1. Start dragging at (10, 10)
        vm.DrawBeginCommand.Execute(new Position(10, 10)).Subscribe();
        
        // 2. Drag to (20, 20)
        vm.DrawingCommand.Execute(new Position(20, 20)).Subscribe();
        
        // Assert: During drag, selecting area should be updated visually via IsRegionSelecting and SelectingArea
        Assert.That(vm.IsRegionSelecting, Is.True);
        Assert.That(vm.SelectingArea.HasValue, Is.True);
        
        // 3. Finish drag
        vm.DrawEndCommand.Execute(new Position(20, 20)).Subscribe();

        // Assert: Final selection
        Assert.That(vm.SelectingArea.Value.X, Is.EqualTo(10));
        Assert.That(vm.SelectingArea.Value.Y, Is.EqualTo(10));
        Assert.That(vm.SelectingArea.Value.Width, Is.GreaterThan(0));
    }

    /// <summary>
    /// ケース2: 既存の選択範囲がある状態で、範囲外からドラッグ開始 -> 既存選択が解除され、新しい選択範囲が作成される
    /// </summary>
    [AvaloniaTest]
    public void Drag_Outside_Existing_Selection_Starts_New_Selection()
    {
        var vm = CreateViewModel(Picture.CreateEmpty(new PictureSize(32, 32)));
        vm.Magnification = new Magnification(1);
        vm.DrawStyle = new RegionSelector();

        // 1. Create selection (0,0)-(10,10)
        vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();
        Assert.That(vm.SelectingArea.Value.X, Is.EqualTo(0));

        // 2. Start dragging OUTSIDE existing selection (20, 20)
        vm.DrawBeginCommand.Execute(new Position(20, 20)).Subscribe();
        
        // Assert: Old selection is gone (or replaced by new empty/small selection at 20,20)
        // DrawBegin resets selection in RegionSelector.
        // During drag, it should be a new selection starting at 20,20
        vm.DrawingCommand.Execute(new Position(30, 30)).Subscribe();
        
        Assert.That(vm.SelectingArea.HasValue, Is.True);
        Assert.That(vm.SelectingArea.Value.X, Is.EqualTo(20)); // New start pos
        
        vm.DrawEndCommand.Execute(new Position(30, 30)).Subscribe();
    }

    /// <summary>
    /// ケース3: 既存の選択範囲がある状態で、範囲内からドラッグ開始 -> 移動モードになる
    /// </summary>
    [AvaloniaTest]
    public void Drag_Inside_Existing_Selection_Moves_It()
    {
        // 32x32 Red Picture
        var red = new ArgbColor(255, 255, 0, 0);
        var initialPicture = CreateFilledPicture(new PictureSize(32, 32), red);
        var vm = CreateViewModel(initialPicture);
        vm.Magnification = new Magnification(1);

        // 1. Select (0,0)-(8,8)
        vm.DrawStyle = new RegionSelector();
        vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(8, 8)).Subscribe();

        // Verify selection exists
        Assert.That(vm.SelectingArea.HasValue, Is.True);
        Assert.That(vm.SelectingArea.Value.X, Is.EqualTo(0));

        // 2. Click INSIDE existing selection (e.g. 4,4).
        // This should start dragging.
        vm.DrawBeginCommand.Execute(new Position(4, 4)).Subscribe();
        
        // Verify we ARE in dragging mode (PreviewPixels is NOT null)
        Assert.That(vm.PreviewPixels, Is.Not.Null, "Should be dragging (PreviewPixels should be set)");

        // 3. Drag to (10,10) and finish
        vm.DrawingCommand.Execute(new Position(10, 10)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();

        // 4. Verify preview is at new position (10,10) relative to start.
        // Start drag at (4,4), End drag at (10,10) -> Delta (6,6).
        // Original Area (0,0) -> New Pos (6,6).
        // Wait, Coordinator calculates delta from StartPosition.
        // DraggingState uses _nowPosition - _startPosition.
        // If we finished dragging, we are in SelectionPreviewState.
        
        Assert.That(vm.PreviewPosition, Is.EqualTo(new Position(6, 6)));
        
        // 5. Commit by clicking outside
        vm.DrawBeginCommand.Execute(new Position(20, 20)).Subscribe();

        // 6. Verify pixels moved
        var result = vm.PictureBuffer.Fetch();
        Assert.That(result.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0), "Original pos should be cleared");
        Assert.That(result.PickColor(new Position(6, 6)), Is.EqualTo(red), "New pos should be red");
    }

    [AvaloniaTest]
    public void Dragging_Should_Clear_Original_Position()
    {
        // 32x32 Red Picture
        var red = new ArgbColor(255, 255, 0, 0);
        var initialPicture = CreateFilledPicture(new PictureSize(32, 32), red);
        var vm = CreateViewModel(initialPicture);
        vm.Magnification = new Magnification(1);

        // 1. Select (0,0)-(8,8)
        vm.DrawStyle = new RegionSelector();
        vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(8, 8)).Subscribe();

        // 2. Start dragging from (4,4)
        vm.DrawBeginCommand.Execute(new Position(4, 4)).Subscribe();

        // 3. Move to (14, 14). Offset is (10, 10).
        vm.DrawingCommand.Execute(new Position(14, 14)).Subscribe();

        // 4. Check the visual (via Painted).
        var painted = _coordinator.Painted(vm.PictureBuffer, vm.PenStyle, vm.ImageTransfer);

        // Original position (0,0) should be cleared (Alpha 0).
        Assert.That(painted.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0), "Original position should be cleared during drag");

        // New position (10,10) should be red.
        Assert.That(painted.PickColor(new Position(10, 10)), Is.EqualTo(red), "New position should be red");
    }

    [AvaloniaTest]
    public void Drag_Twice_Should_Maintain_Cut_State()
    {
        // 32x32 Transparent Picture with Red rect at (0,0)-(8,8)
        var red = new ArgbColor(255, 255, 0, 0);
        var size = new PictureSize(32, 32);
        var rectSize = new PictureSize(8, 8);
        var rectData = new byte[8 * 8 * 4];
        for (int i = 0; i < rectData.Length; i += 4)
        {
            rectData[i] = red.Blue; rectData[i + 1] = red.Green; rectData[i + 2] = red.Red; rectData[i + 3] = red.Alpha;
        }
        var rectPic = Picture.Create(rectSize, rectData);
        var initialPicture = Picture.CreateEmpty(size).Blend(new DirectImageBlender(), rectPic, new Position(0, 0));

        var vm = CreateViewModel(initialPicture);
        vm.Magnification = new Magnification(1);

        // 1. Select (0,0)-(8,8)
        vm.DrawStyle = new RegionSelector();
        vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(8, 8)).Subscribe();

        // 2. Drag 1: (0,0) -> (10,10)
        vm.DrawBeginCommand.Execute(new Position(4, 4)).Subscribe();
        vm.DrawingCommand.Execute(new Position(10, 10)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();

        // Verify state after Drag 1
        var painted1 = _coordinator.Painted(vm.PictureBuffer, vm.PenStyle, vm.ImageTransfer);
        Assert.That(painted1.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0), "Original pos (0,0) should be clear after drag 1");
        Assert.That(painted1.PickColor(new Position(10, 10)), Is.EqualTo(red), "New pos (10,10) should be red after drag 1");

        // 3. Drag 2: (10,10) -> (20,20)
        // Note: Clicking on (10,10) which is the current preview position
        vm.DrawBeginCommand.Execute(new Position(10, 10)).Subscribe();
        vm.DrawingCommand.Execute(new Position(20, 20)).Subscribe();
        
        // Check visual during drag 2
        var painted2 = _coordinator.Painted(vm.PictureBuffer, vm.PenStyle, vm.ImageTransfer);

        // Verify original pos (0,0) is STILL clear
        Assert.That(painted2.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0), "Original pos (0,0) should remain clear during drag 2");
        
        // Verify intermediate pos (10,10) is clear (since we moved away from it)
        Assert.That(painted2.PickColor(new Position(10, 10)).Alpha, Is.EqualTo(0), "Intermediate pos (10,10) should be clear during drag 2");

        // Verify new pos (20,20) is red
        Assert.That(painted2.PickColor(new Position(20, 20)), Is.EqualTo(red), "New pos (20,20) should be red during drag 2");
    }

    [AvaloniaTest]
    public void Dragging_With_DirectBlender_Should_Not_Apply_Transparency()
    {
        // Background: Green
        var green = new ArgbColor(255, 0, 255, 0);
        var initialPicture = CreateFilledPicture(new PictureSize(32, 32), green);
        
        // Selection: Red with Blue center
        // Blue will be our "transparent color" candidate
        var red = new ArgbColor(255, 255, 0, 0);
        var blue = new ArgbColor(255, 0, 0, 255);
        var selectionData = new byte[8 * 8 * 4];
        for (int i = 0; i < selectionData.Length; i += 4)
        {
            // Center 4x4 is blue, rest red
            int x = (i / 4) % 8;
            int y = (i / 4) / 8;
            bool isCenter = x >= 2 && x < 6 && y >= 2 && y < 6;
            var c = isCenter ? blue : red;
            selectionData[i] = c.Blue; selectionData[i + 1] = c.Green; selectionData[i + 2] = c.Red; selectionData[i + 3] = c.Alpha;
        }
        var selectionPic = Picture.Create(new PictureSize(8, 8), selectionData);
        // Blend selection onto initial picture at (0,0)
        initialPicture = initialPicture.Blend(new DirectImageBlender(), selectionPic, new Position(0, 0));

        var vm = CreateViewModel(initialPicture);
        vm.Magnification = new Magnification(1);
        
        // Setup Blender (Direct = Off)
        vm.ImageBlender = new DirectImageBlender();
        vm.BackgroundColor = new BackgroundColor(blue); // This should be ignored by DirectImageBlender

        // 1. Select (0,0)-(8,8)
        vm.DrawStyle = new RegionSelector();
        vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(8, 8)).Subscribe();

        // 2. Drag to (10,10)
        vm.DrawBeginCommand.Execute(new Position(4, 4)).Subscribe();
        vm.DrawingCommand.Execute(new Position(14, 14)).Subscribe();

        // 3. Verify visual
        var painted = _coordinator.Painted(vm.PictureBuffer, vm.PenStyle, vm.ImageTransfer);
        
        // The center of the dragged image (at 10+2, 10+2 = 12,12) should be BLUE (opaque), not Green (transparent/underlying).
        Assert.That(painted.PickColor(new Position(12, 12)), Is.EqualTo(blue), "Center should remain Blue with DirectImageBlender");
    }

    [AvaloniaTest]
    public void Dragging_With_AlphaBlender_Should_Apply_Transparency()
    {
        // Background: Green
        var green = new ArgbColor(255, 0, 255, 0);
        var initialPicture = CreateFilledPicture(new PictureSize(32, 32), green);
        
        // Selection: Red with Blue center
        var red = new ArgbColor(255, 255, 0, 0);
        var blue = new ArgbColor(255, 0, 0, 255);
        var selectionData = new byte[8 * 8 * 4];
        for (int i = 0; i < selectionData.Length; i += 4)
        {
            int x = (i / 4) % 8;
            int y = (i / 4) / 8;
            bool isCenter = x >= 2 && x < 6 && y >= 2 && y < 6;
            var c = isCenter ? blue : red;
            selectionData[i] = c.Blue; selectionData[i + 1] = c.Green; selectionData[i + 2] = c.Red; selectionData[i + 3] = c.Alpha;
        }
        var selectionPic = Picture.Create(new PictureSize(8, 8), selectionData);
        initialPicture = initialPicture.Blend(new DirectImageBlender(), selectionPic, new Position(0, 0));

        var vm = CreateViewModel(initialPicture);
        vm.Magnification = new Magnification(1);
        
        // Setup Blender (Alpha = On)
        vm.ImageBlender = new AlphaImageBlender();
        vm.BackgroundColor = new BackgroundColor(blue); // Blue pixels should become transparent

        // 1. Select (0,0)-(8,8)
        vm.DrawStyle = new RegionSelector();
        vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(8, 8)).Subscribe();

        // 2. Drag to (10,10)
        vm.DrawBeginCommand.Execute(new Position(4, 4)).Subscribe();
        vm.DrawingCommand.Execute(new Position(14, 14)).Subscribe();

        // 3. Verify visual
        var painted = _coordinator.Painted(vm.PictureBuffer, vm.PenStyle, vm.ImageTransfer);
        
        // The center of the dragged image (at 12,12) should be GREEN (underlying), because Blue became transparent.
        Assert.That(painted.PickColor(new Position(12, 12)), Is.EqualTo(green), "Center should be Green (transparent) with AlphaImageBlender");
    }

    [AvaloniaTest]
    public void Commit_And_Start_New_Selection_In_Single_Click()
    {
        // 32x32 Red Picture
        var red = new ArgbColor(255, 255, 0, 0);
        var initialPicture = CreateFilledPicture(new PictureSize(32, 32), red);
        var vm = CreateViewModel(initialPicture);
        vm.Magnification = new Magnification(1);

        // 1. Select (0,0)-(8,8)
        vm.DrawStyle = new RegionSelector();
        vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(8, 8)).Subscribe();

        // 2. Drag to (10,10) and release -> Preview State
        vm.DrawBeginCommand.Execute(new Position(4, 4)).Subscribe();
        vm.DrawingCommand.Execute(new Position(10, 10)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();

        // Assert: In preview state
        Assert.That(vm.PreviewPixels, Is.Not.Null);

        // 3. Click outside (20,20) to commit AND start new selection
        vm.DrawBeginCommand.Execute(new Position(20, 20)).Subscribe();

        // Assert 1: Preview is gone (Committed)
        Assert.That(vm.PreviewPixels, Is.Null, "Preview should be null (committed)");
        // Assert 2: Image is updated
        var result = vm.PictureBuffer.Fetch();
        Assert.That(result.PickColor(new Position(10, 10)), Is.EqualTo(red), "Moved pixels should be committed");

        // 4. Drag to (25,25) to confirm selection logic
        vm.DrawingCommand.Execute(new Position(25, 25)).Subscribe();

        // Assert 3: New selection started at (20,20)
        Assert.That(vm.IsRegionSelecting, Is.True, "Should be selecting new region");
        Assert.That(vm.SelectingArea.Value.X, Is.EqualTo(20), "New selection should start at click position (20,20)");
    }
}