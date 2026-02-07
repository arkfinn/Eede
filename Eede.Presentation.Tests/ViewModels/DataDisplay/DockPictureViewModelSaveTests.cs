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

namespace Eede.Presentation.Tests.ViewModels.DataDisplay;

[TestFixture]
public class DockPictureViewModelSaveTests
{
    private Mock<IBitmapAdapter<Bitmap>> _bitmapAdapterMock;
    private Mock<IPictureIOService> _pictureIOServiceMock;
    private Mock<IFileStorage> _fileStorageMock;
    private Mock<IAnimationPatternsProvider> _patternsProviderMock;
    private Mock<IAnimationPatternService> _patternServiceMock;
    private Mock<IFileSystem> _fileSystemMock;
    private GlobalState _globalState;
    private AnimationViewModel _animationViewModel;

    [SetUp]
    public void SetUp()
    {
        _bitmapAdapterMock = new Mock<IBitmapAdapter<Bitmap>>();
        _pictureIOServiceMock = new Mock<IPictureIOService>();
        _fileStorageMock = new Mock<IFileStorage>();
        _patternsProviderMock = new Mock<IAnimationPatternsProvider>();
        _patternServiceMock = new Mock<IAnimationPatternService>();
        _fileSystemMock = new Mock<IFileSystem>();

        _patternsProviderMock.Setup(x => x.Current).Returns(new AnimationPatterns());

        _globalState = new GlobalState();
        _animationViewModel = new AnimationViewModel(
            _patternsProviderMock.Object,
            _patternServiceMock.Object,
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

    private DockPictureViewModel CreateViewModel(FilePath path)
    {
        var vm = new DockPictureViewModel(_globalState, _animationViewModel, _bitmapAdapterMock.Object, _pictureIOServiceMock.Object);
        vm.Initialize(Picture.CreateEmpty(new PictureSize(1, 1)), path);
        return vm;
    }
}