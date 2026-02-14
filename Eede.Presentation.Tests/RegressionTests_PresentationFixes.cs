using Avalonia.Media.Imaging;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Services;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Pages;
using Moq;
using NUnit.Framework;
using System;
using System.Reactive;
using System.Reactive.Subjects;
using Avalonia.Headless.NUnit;
using Dock.Model.Avalonia.Controls;
using System.Threading.Tasks;

namespace Eede.Presentation.Tests;

#nullable enable

[TestFixture]
public class RegressionTests_PresentationFixes
{
    private Mock<IDrawingSessionProvider> _drawingSessionProviderMock = default!;
    private Mock<IInteractionCoordinator> _coordinatorMock = default!;
    private DrawingSessionProvider _realDrawingSessionProvider = default!;
    private InteractionCoordinator _realCoordinator = default!;

    [SetUp]
    public void Setup()
    {
        _drawingSessionProviderMock = new Mock<IDrawingSessionProvider>();
        // Ensure CurrentSession is never null to prevent NRE in DrawingSessionViewModel
        var initialPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        _drawingSessionProviderMock.Setup(x => x.CurrentSession).Returns(new DrawingSession(initialPicture));

        _coordinatorMock = new Mock<IInteractionCoordinator>();
        _realDrawingSessionProvider = new DrawingSessionProvider();
        _realCoordinator = new InteractionCoordinator(_realDrawingSessionProvider);
    }

    [AvaloniaTest]
    public void DocumentSwitching_ShouldNot_OverwriteWorkspace()
    {
        // Setup MainViewModel with mocked DrawingSessionProvider to verify Update calls
        var mainVM = CreateMainViewModel(_drawingSessionProviderMock.Object, _coordinatorMock.Object);

        var picture1 = Picture.CreateEmpty(new PictureSize(16, 16));
        var dockVM1 = new DockPictureViewModel(new GlobalState(), mainVM.AnimationViewModel, new AvaloniaBitmapAdapter(), Mock.Of<IPictureIOService>());
        dockVM1.PictureBuffer = picture1;
        var doc1 = new Document { DataContext = dockVM1 };

        var picture2 = Picture.CreateEmpty(new PictureSize(16, 16));
        var dockVM2 = new DockPictureViewModel(new GlobalState(), mainVM.AnimationViewModel, new AvaloniaBitmapAdapter(), Mock.Of<IPictureIOService>());
        dockVM2.PictureBuffer = picture2;
        var doc2 = new Document { DataContext = dockVM2 };

        mainVM.Pictures.Add(dockVM1);
        mainVM.Pictures.Add(dockVM2);

        // Act: Switch ActiveDockable
        mainVM.ActiveDockable = doc1;
        mainVM.ActiveDockable = doc2;

        // Assert: Update should NOT have been called on the provider
        // Reset the mock call counts because the constructor might have triggered some updates during initialization
        _drawingSessionProviderMock.Invocations.Clear();
        mainVM.ActiveDockable = doc1;
        mainVM.ActiveDockable = doc2;

        _drawingSessionProviderMock.Verify(x => x.Update(It.IsAny<DrawingSession>()), Times.Never,
            "Workspace should not be overwritten automatically when switching documents.");
    }

    [AvaloniaTest]
    public void InteractionCoordinator_ShouldHandle_ExternalBufferUpdate_Correctly()
    {
        // Setup InteractionCoordinator with real session provider
        var initialPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        _realDrawingSessionProvider.Update(new DrawingSession(initialPicture));

        // 1. Initial interaction (Normal state)
        _realCoordinator.SyncWithSession();
        Assert.That(_realCoordinator.CurrentBuffer!.Fetch(), Is.EqualTo(initialPicture));

        // 2. External update (e.g. Undo or Push)
        var updatedPicture = Picture.CreateEmpty(new PictureSize(32, 32));
        _realDrawingSessionProvider.Update(new DrawingSession(updatedPicture));

        // 3. Sync with session should detect the change and reset the internal session safely
        _realCoordinator.SyncWithSession();

        // Assert: Buffer is updated
        Assert.That(_realCoordinator.CurrentBuffer!.Fetch(), Is.EqualTo(updatedPicture), "Coordinator should sync with external buffer changes.");

        // 4. Verify no crash on subsequent interaction
        // Use 16x16 grid size to avoid DivideByZero if HalfBoxArea expects power-of-two or similar
        Assert.DoesNotThrow(() =>
        {
            _realCoordinator.PointerBegin(new Position(0, 0), new DrawingBuffer(updatedPicture), new RegionSelector(), new PenStyle(new DirectImageBlender()), false, false, new PictureSize(16, 16), null!);
        }, "Coordinator should not crash after an external buffer update.");
    }

    [AvaloniaTest]
    public void InteractionCoordinator_ShouldNot_RevertToOldImage_OnPointerActionAfterExternalUpdate()
    {
        // This test specifically addresses the "revert" bug.
        var initialPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        _realDrawingSessionProvider.Update(new DrawingSession(initialPicture));
        _realCoordinator.SyncWithSession();

        // 1. External update to a DIFFERENT picture
        var newPicture = Picture.CreateEmpty(new PictureSize(16, 16)); // different instance/content
        _realDrawingSessionProvider.Update(new DrawingSession(newPicture));

        // 2. Sync
        _realCoordinator.SyncWithSession();

        // 3. Perform a pointer action. 
        // Previously, if SyncWithSession was broken, this might have used an old internal session with 'initialPicture'.
        _realCoordinator.PointerBegin(new Position(0, 0), new DrawingBuffer(newPicture), new RegionSelector(), new PenStyle(new DirectImageBlender()), false, false, new PictureSize(16, 16), null!);

        // Assert: CurrentBuffer should be the newPicture, not the old one
        Assert.That(_realCoordinator.CurrentBuffer!.Fetch(), Is.EqualTo(newPicture), "Should maintain the new picture after external update, even during interactions.");
    }

    [AvaloniaTest]
    public void InteractionCoordinator_ShouldNotCrash_WhenSyncIsCalledDuringOperation()
    {
        var initialPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        var provider = new DrawingSessionProvider();
        provider.Update(new DrawingSession(initialPicture));

        var coordinator = new InteractionCoordinator(provider);
        coordinator.SyncWithSession();

        // Simulate an environment where provider.Update triggers coordinator.SyncWithSession
        // (In the real app, this happens via SessionChanged event in DrawableCanvasViewModel)
        provider.SessionChanged += (session) =>
        {
            coordinator.SyncWithSession();
        };

        var nextPicture = Picture.CreateEmpty(new PictureSize(16, 16));

        Assert.DoesNotThrow(() =>
        {
            // PointerBegin will call provider.Update several times internally
            coordinator.PointerBegin(
                new Position(0, 0),
                new DrawingBuffer(initialPicture),
                new RegionSelector(),
                new PenStyle(new DirectImageBlender()),
                false, false, new PictureSize(16, 16),
                null!);
        }, "Coordinator should be resilient to SyncWithSession calls triggered during its own operations.");
    }

    private MainViewModel CreateMainViewModel(IDrawingSessionProvider sessionProvider, IInteractionCoordinator coordinator)
    {
        var state = new GlobalState();
        var clipboard = new Mock<IClipboard>().Object;
        var bitmapAdapter = new Mock<IBitmapAdapter<Bitmap>>().Object;
        var pictureRepo = new Mock<IPictureRepository>().Object;
        var drawStyleFactory = new Mock<IDrawStyleFactory>().Object;
        var transformUseCase = new Mock<ITransformImageUseCase>().Object;
        var transferToCanvas = new Mock<ITransferImageToCanvasUseCase>().Object;
        var transferFromCanvas = new Mock<ITransferImageFromCanvasUseCase>().Object;

        var selectionService = new SelectionService(
            new CopySelectionUseCase(clipboard),
            new CutSelectionUseCase(clipboard),
            new PasteFromClipboardUseCase(clipboard, sessionProvider)
        );

        var drawableCanvasVM = new DrawableCanvasViewModel(
            state,
            new Mock<IAddFrameProvider>().Object,
            clipboard,
            bitmapAdapter,
            sessionProvider,
            selectionService,
            coordinator
        );

        var patternsProvider = new AnimationPatternsProvider();
        var animationVM = new AnimationViewModel(
            patternsProvider,
            new AnimationPatternService(
                new AddAnimationPatternUseCase(patternsProvider),
                new ReplaceAnimationPatternUseCase(patternsProvider),
                new RemoveAnimationPatternUseCase(patternsProvider)
            ),
            new Mock<IFileSystem>().Object,
            new AvaloniaBitmapAdapter()
        );

        var sessionVM = new DrawingSessionViewModel(sessionProvider);
        var paletteVM = new PaletteContainerViewModel();
        var pictureIOService = new PictureIOService(
            new SavePictureUseCase(pictureRepo),
            new LoadPictureUseCase(pictureRepo)
        );

        return new MainViewModel(
            state, clipboard, bitmapAdapter, pictureRepo, drawStyleFactory,
            transformUseCase, new Mock<IScalingImageUseCase>().Object, transferToCanvas, transferFromCanvas,
            sessionProvider, drawableCanvasVM, animationVM, sessionVM,
            paletteVM, pictureIOService, new Mock<IThemeService>().Object,
            () => new DockPictureViewModel(state, animationVM, new AvaloniaBitmapAdapter(), pictureIOService),
            () => new NewPictureWindowViewModel()
        );
    }
}
