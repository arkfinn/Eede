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

namespace Eede.Presentation.Tests.Services;

[TestFixture]
public class InteractionCoordinatorCharacterizationTests
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

    [AvaloniaTest]
    public void SelectionMove_And_Commit_Test()
    {
        // 32x32 の赤い画像
        var red = new ArgbColor(255, 255, 0, 0);
        var initialPicture = CreateFilledPicture(new PictureSize(32, 32), red);
        var vm = CreateViewModel(initialPicture);
        vm.Magnification = new Magnification(1);

        // 1. (0,0)-(8,8) を選択
        vm.DrawStyle = new RegionSelector();
        vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(8, 8)).Subscribe();

        // 2. ドラッグ移動 (0,0) -> (10,10)
        vm.DrawBeginCommand.Execute(new Position(4, 4)).Subscribe();
        vm.DrawingCommand.Execute(new Position(14, 14)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(14, 14)).Subscribe();

        // 非破壊プレビュー仕様:
        // 内部バッファ (PictureBuffer) は書き換わっていないので (0,0) は赤いまま (255)
        Assert.That(vm.PictureBuffer.Fetch().PickColor(new Position(0, 0)).Alpha, Is.EqualTo(255), "内部バッファは未確定の間は書き換わらない");

        // 表示用画像 (Paintedの結果) では (0,0) はクリアされ、(10,10) にプレビューがあるはず
        var painted = _coordinator.Painted(vm.PictureBuffer, vm.PenStyle, vm.ImageTransfer);
        Assert.That(painted.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0), "表示用画像では元の位置がクリアされている");
        Assert.That(vm.PreviewPixels, Is.Not.Null);

        // 3. 選択範囲外 (20,20) をクリックして確定
        vm.DrawBeginCommand.Execute(new Position(20, 20)).Subscribe();

        // 確定後：内部バッファが更新され、(10,10) が赤、(0,0) が透明になる
        var result = vm.PictureBuffer.Fetch();
        Assert.That(result.PickColor(new Position(10, 10)), Is.EqualTo(red), "確定後の移動先に画像が書き込まれている");
        Assert.That(result.PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0), "確定後の元の位置は透明になっている");
        Assert.That(vm.PreviewPixels, Is.Null, "確定後のプレビューがクリアされている");
    }

    [AvaloniaTest]
    public void SelectionMove_And_Cancel_Test()
    {
        // 32x32 の赤い画像
        var red = new ArgbColor(255, 255, 0, 0);
        var initialPicture = CreateFilledPicture(new PictureSize(32, 32), red);
        var vm = CreateViewModel(initialPicture);
        vm.Magnification = new Magnification(1);

        // 1. 移動操作
        vm.DrawStyle = new RegionSelector();
        vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(8, 8)).Subscribe();
        vm.DrawBeginCommand.Execute(new Position(4, 4)).Subscribe();
        vm.DrawingCommand.Execute(new Position(14, 14)).Subscribe();
        vm.DrawEndCommand.Execute(new Position(14, 14)).Subscribe();

        // 2. 右クリックでキャンセル
        vm.PointerRightButtonPressedCommand.Execute(new Position(14, 14)).Subscribe();

        // キャンセル後：(0,0) が赤に戻っており、プレビューは消えているはず
        var result = vm.PictureBuffer.Fetch();
        Assert.That(result.PickColor(new Position(0, 0)), Is.EqualTo(red), "キャンセル後の元の位置に画像が復元されている");
        Assert.That(vm.PreviewPixels, Is.Null, "キャンセル後のプレビューがクリアされている");
    }
}
