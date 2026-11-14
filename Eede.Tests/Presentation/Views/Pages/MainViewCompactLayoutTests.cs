using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.Settings;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Settings;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Controls;
using Eede.Presentation.Coordinators;
using Eede.Presentation.Settings;
using Eede.Presentation.Theming;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.General;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.DataEntry;
using Eede.Presentation.Views.Pages;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Eede.Presentation.Tests.Views.Pages;

#nullable enable

[TestFixture]
public class MainViewCompactLayoutTests
{
    private static MainViewModel CreateMainViewModel()
    {
        var state = new GlobalState();
        var clipboard = new Mock<IClipboard>().Object;
        var bitmapAdapter = new AvaloniaBitmapAdapter();
        var pictureRepo = new Mock<IPictureRepository>().Object;
        var drawStyleFactory = new Mock<IDrawStyleFactory>();
        drawStyleFactory.Setup(x => x.Create(It.IsAny<DrawStyleType>())).Returns(new Mock<IDrawStyle>().Object);
        var sessionProvider = new DrawingSessionProvider();
        sessionProvider.Update(new DrawingSession(Picture.CreateEmpty(new PictureSize(16, 16))));
        var coordinator = new InteractionCoordinator(sessionProvider);
        var selectionClipboard = new SelectionClipboard(new CopySelectionUseCase(clipboard), new CutSelectionUseCase(clipboard), new PasteFromClipboardUseCase(clipboard, sessionProvider));
        var drawableCanvasVM = new DrawableCanvasViewModel(state, new Mock<IAddFrameProvider>().Object, clipboard, bitmapAdapter, sessionProvider, selectionClipboard, coordinator);
        var patternsProvider = new AnimationPatternsProvider();
        var animationVM = new AnimationViewModel(patternsProvider, new AnimationPatternEditor(new AddAnimationPatternUseCase(patternsProvider), new ReplaceAnimationPatternUseCase(patternsProvider), new RemoveAnimationPatternUseCase(patternsProvider)), new Mock<IFileSystem>().Object, bitmapAdapter);
        var settingsRepo = new Mock<ISettingsRepository>();
        settingsRepo.Setup(x => x.LoadAsync()).ReturnsAsync(new AppSettings());
        var pictureFileIO = new PictureFileIO(new SavePictureUseCase(pictureRepo, settingsRepo.Object), new LoadPictureUseCase(pictureRepo, settingsRepo.Object));

        return new MainViewModel(
            state, clipboard, bitmapAdapter, pictureRepo, drawStyleFactory.Object,
            new Mock<ITransformImageUseCase>().Object, new Mock<IScalingImageUseCase>().Object,
            new Mock<ITransferImageToCanvasUseCase>().Object, new Mock<ITransferImageFromCanvasUseCase>().Object,
            sessionProvider, drawableCanvasVM, animationVM, new DrawingSessionViewModel(sessionProvider),
            new PaletteContainerViewModel(new Mock<IPaletteRepository>().Object, new Mock<IPaletteSessionRepository>().Object),
            pictureFileIO, new Mock<IThemeDetector>().Object,
            new LoadSettingsUseCase(settingsRepo.Object), new SaveSettingsUseCase(settingsRepo.Object),
            new WelcomeViewModel(settingsRepo.Object, new Mock<IExternalBrowserLauncher>().Object),
            () => new DockPictureViewModel(state, animationVM, bitmapAdapter, pictureFileIO),
            () => new NewPictureWindowViewModel()
        );
    }

    [AvaloniaTest]
    [TestCase(1024, 720)]
    [TestCase(800, 650)]
    public void DesktopSizes_ShouldNotActivateCompactLayout_AndShouldShowBothPanesAndPalette(double width, double height)
    {
        var vm = CreateMainViewModel();
        var view = new MainView { DataContext = vm };
        var window = new Window { Content = view, Width = width, Height = height };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(vm.IsCompactLayout, Is.False);
                Assert.That(view.Classes.Contains("compact"), Is.False);
                Assert.That(view.FindControl<Control>("LeftWorkspacePane")?.IsVisible, Is.True);
                Assert.That(view.FindControl<Control>("RightDockPane")?.IsVisible, Is.True);
                Assert.That(view.FindControl<Control>("MainSplitter")?.IsVisible, Is.True);
                Assert.That(view.FindControl<Control>("PaletteHostPanel")?.IsVisible, Is.True);
            });
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void MobileLandscapeSize_ShouldActivateCompactLayout_AndSupportPaneAndPaletteToggles()
    {
        var vm = CreateMainViewModel();
        var view = new MainView { DataContext = vm };
        var window = new Window { Content = view, Width = 667, Height = 320 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var leftPane = view.FindControl<Control>("LeftWorkspacePane");
            var rightPane = view.FindControl<Control>("RightDockPane");
            var splitter = view.FindControl<Control>("MainSplitter");
            var paletteHost = view.FindControl<Control>("PaletteHostPanel");
            var leftToolPanel = view.FindControl<Control>("LeftToolPanel");

            Assert.Multiple(() =>
            {
                Assert.That(vm.IsCompactLayout, Is.True);
                Assert.That(view.Classes.Contains("compact"), Is.True);
                Assert.That(vm.IsCompactDockActive, Is.False);
                Assert.That(vm.IsCompactPaletteOpen, Is.False);
                Assert.That(vm.IsCompactToolPanelOpen, Is.True);
                Assert.That(leftPane?.IsVisible, Is.True);
                Assert.That(rightPane?.IsVisible, Is.False);
                Assert.That(splitter?.IsVisible, Is.False);
                Assert.That(paletteHost?.IsVisible, Is.False);
                Assert.That(leftToolPanel?.IsVisible, Is.True);
                Assert.That(view.GetVisualDescendants().OfType<PaletteContainer>().Count(), Is.LessThanOrEqualTo(1));
            });

            vm.IsCompactPaletteOpen = true;
            Dispatcher.UIThread.RunJobs();
            Assert.That(paletteHost?.IsVisible, Is.True);

            vm.IsCompactToolPanelOpen = false;
            Dispatcher.UIThread.RunJobs();
            Assert.That(leftToolPanel?.IsVisible, Is.False);

            vm.IsCompactDockActive = true;
            Dispatcher.UIThread.RunJobs();
            Assert.Multiple(() =>
            {
                Assert.That(leftPane?.IsVisible, Is.False);
                Assert.That(rightPane?.IsVisible, Is.True);
            });
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void ScalingDialogContentView_ShouldContainScrollViewer_AndMaintainDialogButtons()
    {
        var vm = new ScalingDialogViewModel(new PictureSize(32, 32));
        var view = new ScalingDialogContentView { DataContext = vm };
        var window = new Window { Content = view, Width = 400, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var scrollViewer = view.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
            var dialogButtons = view.GetVisualDescendants().OfType<DialogButtons>().FirstOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(scrollViewer, Is.Not.Null);
                Assert.That(dialogButtons, Is.Not.Null);
                Assert.That(dialogButtons!.IsVisible, Is.True);
            });
        }
        finally
        {
            window.Close();
        }
    }
}
