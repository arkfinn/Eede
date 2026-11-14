using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.Settings;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Files;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Moq;
using NUnit.Framework;

namespace Eede.Presentation.Tests.ViewModels.DataDisplay;

[TestFixture]
public class DockPictureViewModelTitleTests
{
    private GlobalState _globalState;
    private AnimationViewModel _animationViewModel;
    private IBitmapAdapter<Bitmap> _bitmapAdapter;
    private IPictureFileIO _pictureFileIO;

    [SetUp]
    public void Setup()
    {
        DockPictureViewModel.ResetUntitledCounter();
        _globalState = new GlobalState();
        var patternsProvider = new AnimationPatternsProvider();
        var patternEditor = new AnimationPatternEditor(
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider));
        _bitmapAdapter = new AvaloniaBitmapAdapter();
        _animationViewModel = new AnimationViewModel(
            patternsProvider,
            patternEditor,
            new Mock<IFileSystem>().Object,
            _bitmapAdapter);
        _pictureFileIO = new Mock<IPictureFileIO>().Object;
    }

    [AvaloniaTest]
    public void NewDocument_HasNumberedTitleAndEmptySuffix()
    {
        var vm1 = DockPictureViewModel.FromSize(new PictureSize(32, 32), _globalState, _animationViewModel, _bitmapAdapter, _pictureFileIO);
        var vm2 = DockPictureViewModel.FromSize(new PictureSize(16, 16), _globalState, _animationViewModel, _bitmapAdapter, _pictureFileIO);

        Assert.That(vm1.Title, Is.EqualTo("無題-1"));
        Assert.That(vm2.Title, Is.EqualTo("無題-2"));
        Assert.That(vm1.DisambiguationSuffix, Is.EqualTo(""));
    }

    [AvaloniaTest]
    public void EditedDocument_HasTrailingAsterisk()
    {
        var vm = DockPictureViewModel.FromSize(new PictureSize(32, 32), _globalState, _animationViewModel, _bitmapAdapter, _pictureFileIO);
        Assert.That(vm.Title, Is.EqualTo("無題-1"));

        vm.Edited = true;
        Assert.That(vm.Title, Is.EqualTo("無題-1 *"));

        vm.Edited = false;
        Assert.That(vm.Title, Is.EqualTo("無題-1"));
    }

    [AvaloniaTest]
    public void FileDocument_HasFileNameTitle()
    {
        var picture = Picture.CreateEmpty(new PictureSize(32, 32));
        var filePath = new FilePath(@"C:\projects\game\sprites\hero_walk.png");

        var vm = DockPictureViewModel.FromFile(picture, filePath, _globalState, _animationViewModel, _bitmapAdapter, _pictureFileIO);

        Assert.That(vm.BaseName, Is.EqualTo("hero_walk.png"));
        Assert.That(vm.Title, Is.EqualTo("hero_walk.png"));
        Assert.That(vm.ToolTipText, Does.Contain("hero_walk.png"));
        Assert.That(vm.ToolTipText, Does.Contain(@"C:\projects\game\sprites"));
        Assert.That(vm.ToolTipText, Does.Contain("32 × 32 px"));
    }

    [AvaloniaTest]
    public void FileDocument_WithDisambiguationSuffix_IncludesSuffixInTitle()
    {
        var picture = Picture.CreateEmpty(new PictureSize(32, 32));
        var filePath = new FilePath(@"C:\projects\game\sprites\walk.png");

        var vm = DockPictureViewModel.FromFile(picture, filePath, _globalState, _animationViewModel, _bitmapAdapter, _pictureFileIO);
        vm.DisambiguationSuffix = " — player";

        Assert.That(vm.Title, Is.EqualTo("walk.png — player"));

        vm.Edited = true;
        Assert.That(vm.Title, Is.EqualTo("walk.png — player *"));
    }

    [AvaloniaTest]
    public void LongFileName_IsMiddleTruncatedInTitle()
    {
        var picture = Picture.CreateEmpty(new PictureSize(32, 32));
        var filePath = new FilePath(@"C:\projects\game\sprites\player_animation_attack_special_01.png");

        var vm = DockPictureViewModel.FromFile(picture, filePath, _globalState, _animationViewModel, _bitmapAdapter, _pictureFileIO);

        Assert.That(vm.BaseName, Is.EqualTo("player_animation_attack_special_01.png"));
        Assert.That(vm.Title.Length, Is.EqualTo(18));
        Assert.That(vm.Title, Does.Contain("..."));
        Assert.That(vm.Title, Does.EndWith(".png"));
    }
}
