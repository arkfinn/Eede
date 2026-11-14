using Moq;
using NUnit.Framework;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Domain.ImageEditing;
using Eede.Domain.Files;
using Eede.Presentation.Common.Adapters;
using Eede.Application.Pictures;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Avalonia.Media.Imaging;
using Eede.Application.Infrastructure;
using Eede.Presentation.Files;
using System.Threading.Tasks;
using System;
using Eede.Presentation.Common.Models;
using Eede.Application.Animations;
using Eede.Domain.Animations;
using System.Collections.Generic;
using Eede.Domain.SharedKernel;
using Avalonia.Headless.NUnit;
using Eede.Presentation.Common.Enums;
using ReactiveUI;
using System.Reactive.Threading.Tasks;
using RxVoid = ReactiveUI.Primitives.RxVoid;

namespace Eede.Presentation.Tests.ViewModels.DataDisplay;

[TestFixture]
public class DockPictureViewModelSaveTests
{
    private Mock<IBitmapAdapter<Bitmap>> _bitmapAdapterMock;
    private Mock<IPictureFileIO> _PictureFileIOMock;
    private Mock<IFileStorage> _fileStorageMock;
    private Mock<IAnimationPatternsProvider> _patternsProviderMock;
    private Mock<IAnimationPatternEditor> _patternEditorMock;
    private Mock<IFileSystem> _fileSystemMock;
    private GlobalState _globalState;
    private AnimationViewModel _animationViewModel;

    [SetUp]
    public void SetUp()
    {
        _bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        _PictureFileIOMock = new Mock<IPictureFileIO>();
        _fileStorageMock = new Mock<IFileStorage>();
        _patternsProviderMock = new Mock<IAnimationPatternsProvider>();
        _patternEditorMock = new Mock<IAnimationPatternEditor>();
        _fileSystemMock = new Mock<IFileSystem>();

        _patternsProviderMock.Setup(x => x.Current).Returns(new AnimationPatterns());

        _globalState = new GlobalState();
        _animationViewModel = new AnimationViewModel(
            _patternsProviderMock.Object,
            _patternEditorMock.Object,
            _fileSystemMock.Object,
            new AvaloniaBitmapAdapter());

        // BitmapAdapterのデフォルト挙動を設定
        _bitmapAdapterMock.Setup(x => x.ConvertToBitmap(It.IsAny<Picture>()))
            .Returns(() => null!);
        _bitmapAdapterMock.Setup(x => x.ConvertToPicture(It.IsAny<Bitmap>()))
            .Returns(Picture.CreateEmpty(new PictureSize(1, 1)));
    }

    [AvaloniaTest]
    public async Task Save_WhenPngFile_ShouldNotShowFilePicker()
    {
        // Arrange
        var path = new FilePath("test.png");
        var vm = CreateViewModel(path);

        bool saveInvoked = false;
        vm.PictureSave += async (s, e) =>
        {
            saveInvoked = true;
            await e.File.SaveAsync(_fileStorageMock.Object);
        };

        // Act
        await vm.Save();

        // Assert
        Assert.That(saveInvoked, Is.True);
        _fileStorageMock.Verify(x => x.SaveFilePickerAsync(), Times.Never);
    }

    [AvaloniaTest]
    public async Task Save_WhenBmpFile_ShouldShowFilePicker()
    {
        // Arrange
        var path = new FilePath("test.bmp");
        var vm = CreateViewModel(path);

        vm.PictureSave += async (s, e) =>
        {
            await e.File.SaveAsync(_fileStorageMock.Object);
        };

        // Act
        await vm.Save();

        // Assert
        _fileStorageMock.Verify(x => x.SaveFilePickerAsync(), Times.Once);
    }

    [AvaloniaTest]
    public async Task Save_WhenNewFile_ShouldShowFilePicker()
    {
        // Arrange
        var vm = CreateViewModel(FilePath.Empty());

        vm.PictureSave += async (s, e) =>
        {
            await e.File.SaveAsync(_fileStorageMock.Object);
        };

        // Act
        await vm.Save();

        // Assert
        _fileStorageMock.Verify(x => x.SaveFilePickerAsync(), Times.Once);
    }

    [AvaloniaTest]
    public async Task SaveAs_WhenPngFile_ShouldShowFilePicker()
    {
        // Arrange
        var path = new FilePath("test.png");
        var vm = CreateViewModel(path);

        vm.PictureSave += async (s, e) =>
        {
            await e.File.SaveAsAsync(_fileStorageMock.Object);
        };

        // Act
        await vm.SaveAs();

        // Assert
        _fileStorageMock.Verify(x => x.SaveFilePickerAsync(), Times.Once);
    }

    [AvaloniaTest]
    public async Task SaveAs_WhenNewFile_ShouldShowFilePicker()
    {
        // Arrange
        var vm = CreateViewModel(FilePath.Empty());

        vm.PictureSave += async (s, e) =>
        {
            await e.File.SaveAsAsync(_fileStorageMock.Object);
        };

        // Act
        await vm.SaveAs();

        // Assert
        _fileStorageMock.Verify(x => x.SaveFilePickerAsync(), Times.Once);
    }

    [AvaloniaTest]
    public async Task ExecuteClosing_WhenSaveAlertResultIsSaveAndAsyncSaveSucceeds_SetsClosableTrue()
    {
        var path = new FilePath("test.png");
        var vm = CreateViewModel(path);
        vm.Edited = true;
        bool saved = false;
        vm.PictureSave += async (s, e) =>
        {
            await Task.Delay(10);
            saved = true;
        };

        vm.SaveAlertResult = SaveAlertResult.Save;
        bool result = await ((ReactiveCommand<RxVoid, bool>)vm.OnClosing).Execute().ToTask();

        Assert.Multiple(() =>
        {
            Assert.That(saved, Is.True);
            Assert.That(result, Is.True);
            Assert.That(vm.Closable, Is.True);
            Assert.That(vm.Edited, Is.False);
        });
    }

    [AvaloniaTest]
    public async Task ExecuteClosing_WhenSaveAlertResultIsSaveAndSaveIsCanceled_KeepsClosableFalse()
    {
        var vm = CreateViewModel(FilePath.Empty());
        vm.Edited = true;
        vm.PictureSave += async (s, e) =>
        {
            await Task.Yield();
            e.Cancel();
        };

        vm.SaveAlertResult = SaveAlertResult.Save;
        bool result = await vm.ExecuteClosing();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(vm.Closable, Is.False);
            Assert.That(vm.Edited, Is.True);
        });
    }

    [AvaloniaTest]
    public async Task ExecuteClosing_WhenSaveAlertResultIsCancel_KeepsClosableFalse()
    {
        var vm = CreateViewModel(new FilePath("test.png"));
        vm.Edited = true;
        vm.SaveAlertResult = SaveAlertResult.Cancel;

        bool result = await vm.ExecuteClosing();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(vm.Closable, Is.False);
            Assert.That(vm.Edited, Is.True);
        });
    }

    private DockPictureViewModel CreateViewModel(FilePath path)
    {
        var vm = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _PictureFileIOMock.Object);
        vm.Initialize(Picture.CreateEmpty(new PictureSize(1, 1)), path);
        return vm;
    }
}

