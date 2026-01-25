using Eede.Application.Drawings;
using Eede.Application.Services;
using Eede.Application.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.Common.Adapters;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using ReactiveUI;
using ReactiveUI.Testing;
using System.Windows.Input;
using Avalonia.Headless.NUnit;

namespace Eede.Presentation.Tests.ViewModels.DataEntry;

public class DrawableCanvasViewModelTests
{
    private Mock<IClipboardService> _mockClipboard;
    private Mock<IAddFrameProvider> _mockAddFrameProvider;
    private GlobalState _globalState;

    [SetUp]
    public void Setup()
    {
        _mockClipboard = new Mock<IClipboardService>();
        _mockAddFrameProvider = new Mock<IAddFrameProvider>();
        _globalState = new GlobalState();
    }

    [AvaloniaTest]
    public void Characterization_DrawingAction()
    {
        // 仕様化テスト: 現在の挙動を固定する
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DrawableCanvasViewModel(_globalState, _mockAddFrameProvider.Object, _mockClipboard.Object, new AvaloniaBitmapAdapter(), new DrawActionUseCase());
            viewModel.SetPicture(Picture.CreateEmpty(new PictureSize(32, 32)));
            
            // 赤色で(10, 10)に描画
            var penColor = new ArgbColor(255, 255, 0, 0); // Red
            viewModel.PenColor = penColor;
            viewModel.PenSize = 1;
            viewModel.Magnification = new Magnification(1);
            scheduler.AdvanceBy(1);

            viewModel.DrawBeginCommand.Execute(new Position(10, 10)).Subscribe();
            scheduler.AdvanceBy(1);
            viewModel.DrawingCommand.Execute(new Position(10, 10)).Subscribe();
            scheduler.AdvanceBy(1);
            viewModel.DrawEndCommand.Execute(new Position(10, 10)).Subscribe();
            scheduler.AdvanceBy(1);

            var picture = viewModel.PictureBuffer.Previous;
            var pixel = picture.PickColor(new Position(10, 10));
            
            // 現在の挙動では、PenSize=1の場合、指定した座標が描画されるはず
            Assert.Multiple(() =>
            {
                Assert.That(pixel.Alpha, Is.EqualTo(255), "Alpha should be 255");
                Assert.That(pixel.Red, Is.EqualTo(255), "Red should be 255");
                Assert.That(pixel.Green, Is.EqualTo(0), "Green should be 0");
                Assert.That(pixel.Blue, Is.EqualTo(0), "Blue should be 0");
            });
        });
    }

    [AvaloniaTest]
    public void Characterization_MagnificationUpdate()
    {
        // 仕様化テスト: 倍率変更時に内部状態が更新されるか
        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new DrawableCanvasViewModel(_globalState, _mockAddFrameProvider.Object, _mockClipboard.Object, new AvaloniaBitmapAdapter(), new DrawActionUseCase());
            viewModel.SetPicture(Picture.CreateEmpty(new PictureSize(32, 32)));
            
            var initialMag = viewModel.Magnification.Value;
            Assert.That(initialMag, Is.EqualTo(4));

            viewModel.Magnification = new Magnification(8);
            
            Assert.That(viewModel.Magnification.Value, Is.EqualTo(8));
        });
    }
}
