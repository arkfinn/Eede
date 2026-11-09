using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Eede.Application.Animations;
using Eede.Application.Drawings;
using Eede.Application.Infrastructure;
using Eede.Application.Settings;
using Eede.Application.Pictures;
using Eede.Application.UseCase.Animations;
using Eede.Application.UseCase.Pictures;
using Eede.Application.UseCase.Settings;
using Eede.Domain.Animations;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.SelectionStates;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.GeometricTransformations;
using Eede.Domain.ImageEditing.Filters;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Coordinators;
using Eede.Presentation.Theming;
using Eede.Presentation.Settings;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.ViewModels.DataDisplay;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.General;
using Eede.Presentation.ViewModels.Pages;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Dock.Model.Avalonia.Controls;

namespace Eede.Presentation.Tests
{
    [TestFixture]
    public class RegressionTests_UndoDirtyFlag
    {
        private MainViewModel _mainViewModel = default!;
        private DrawingSessionProvider _sessionProvider = default!;
        private GlobalState _state = default!;

        [SetUp]
        public void Setup()
        {
            _sessionProvider = new DrawingSessionProvider();
            var initialPicture = Picture.CreateEmpty(new PictureSize(16, 16));
            _sessionProvider.Update(new DrawingSession(initialPicture));

            var coordinator = new InteractionCoordinator(_sessionProvider);
            _state = new GlobalState();
            var clipboard = new Mock<IClipboard>().Object;
            var bitmapAdapter = new AvaloniaBitmapAdapter();
            var pictureRepo = new Mock<IPictureRepository>().Object;
            var drawStyleFactory = new Mock<IDrawStyleFactory>().Object;
            var transformUseCaseMock = new Mock<ITransformImageUseCase>();
            transformUseCaseMock.Setup(x => x.Execute(It.IsAny<Picture>(), It.IsAny<PictureActions>(), It.IsAny<AntiAliasMode>()))
                .Returns((Picture p, PictureActions a, AntiAliasMode m) => p);
            transformUseCaseMock.Setup(x => x.Execute(It.IsAny<Picture>(), It.IsAny<PictureActions>(), It.IsAny<PictureArea>(), It.IsAny<AntiAliasMode>()))
                .Returns((Picture p, PictureActions a, PictureArea area, AntiAliasMode m) => p);
            var transformUseCase = transformUseCaseMock.Object;

            var transferToCanvas = new TransferImageToCanvasUseCase();
            var transferFromCanvas = new TransferImageFromCanvasUseCase();

            var selectionClipboard = new SelectionClipboard(
                new CopySelectionUseCase(clipboard),
                new CutSelectionUseCase(clipboard),
                new PasteFromClipboardUseCase(clipboard, _sessionProvider)
            );

            var drawableCanvasVM = new DrawableCanvasViewModel(
                _state,
                new Mock<IAddFrameProvider>().Object,
                clipboard,
                bitmapAdapter,
                _sessionProvider,
                selectionClipboard: selectionClipboard,
                coordinator
            );

            var patternsProvider = new AnimationPatternsProvider();
            var animationVM = new AnimationViewModel(
                patternsProvider,
                new AnimationPatternEditor(
                    new AddAnimationPatternUseCase(patternsProvider),
                    new ReplaceAnimationPatternUseCase(patternsProvider),
                    new RemoveAnimationPatternUseCase(patternsProvider)
                ),
                new Mock<IFileSystem>().Object,
                bitmapAdapter
            );

            var sessionVM = new DrawingSessionViewModel(_sessionProvider);
            var paletteVM = new PaletteContainerViewModel(new Mock<Eede.Application.Infrastructure.IPaletteRepository>().Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);
            var settingsRepo = new Mock<ISettingsRepository>();
            settingsRepo.Setup(x => x.LoadAsync()).ReturnsAsync(new AppSettings());

            var PictureFileIO = new PictureFileIO(
                new SavePictureUseCase(pictureRepo, settingsRepo.Object),
                new LoadPictureUseCase(pictureRepo, settingsRepo.Object)
            );

            var loadUseCase = new LoadSettingsUseCase(settingsRepo.Object);
            var saveUseCase = new SaveSettingsUseCase(settingsRepo.Object);

            _mainViewModel = new MainViewModel(
                _state, clipboard, bitmapAdapter, pictureRepo, drawStyleFactory,
                transformUseCase, new Mock<IScalingImageUseCase>().Object, transferToCanvas, transferFromCanvas,
                _sessionProvider, drawableCanvasVM, animationVM, sessionVM,
                paletteVM, PictureFileIO, new Mock<IThemeDetector>().Object,
                loadUseCase, saveUseCase,
                new WelcomeViewModel(settingsRepo.Object, new Mock<IExternalBrowserLauncher>().Object),
                () => new DockPictureViewModel(_state, animationVM, bitmapAdapter, PictureFileIO),
                () => new NewPictureWindowViewModel()
            );
        }

        [AvaloniaTest]
        public void Pull_Undo_ShouldRevertEditedFlag()
        {
            var picture = Picture.CreateEmpty(new PictureSize(10, 10));
            var vm = new DockPictureViewModel(_state, _mainViewModel.AnimationViewModel, new AvaloniaBitmapAdapter(), Mock.Of<IPictureFileIO>());
            vm.Initialize(picture, new FilePath("test.png"));
            _mainViewModel.Pictures.Add(vm);
            _mainViewModel.ActiveDockable = new Document { DataContext = vm };

            Assert.That(vm.Edited, Is.False, "Initially not edited");

            // Perform Push to dock
            vm.OnPicturePush.Execute(new Position(0, 0)).Subscribe();
            Assert.That(vm.Edited, Is.True, "Marked as edited after Push");

            // Undo
            _mainViewModel.UndoCommand.Execute().Subscribe();

            Assert.That(vm.Edited, Is.False, "Edited flag should be reverted after Undo");

            // 4. Perform Redo
            _mainViewModel.RedoCommand.Execute().Subscribe();
            Assert.That(vm.Edited, Is.True, "Edited flag should be restored after Redo");
        }

        [AvaloniaTest]
        public void CanvasOperations_ShouldNotMarkActiveDockAsEdited_UntilPush()
        {
            var picture = Picture.CreateEmpty(new PictureSize(10, 10));
            var vm = new DockPictureViewModel(_state, _mainViewModel.AnimationViewModel, new AvaloniaBitmapAdapter(), Mock.Of<IPictureFileIO>());
            vm.Initialize(picture, new FilePath("test.png"));
            _mainViewModel.Pictures.Add(vm);
            _mainViewModel.ActiveDockable = new Document { DataContext = vm };

            vm.OnPicturePull.Execute(new PictureArea(new Position(0, 0), picture.Size)).Subscribe();
            Assert.That(vm.Edited, Is.False, "Not edited after Pull");

            // Perform PictureAction (Canvas operation)
            _mainViewModel.PictureActionCommand.Execute(PictureActions.RotateLeft).Subscribe();

            // 作業エリアでの操作では、ドック側画像は変更されないため Edited は false のままであるべき
            Assert.That(vm.Edited, Is.False, "Active dock item should NOT be marked as edited after canvas operation");

            // Undo canvas operation
            _mainViewModel.UndoCommand.Execute().Subscribe();
            Assert.That(vm.Edited, Is.False, "Edited flag should remain false after undoing canvas operation");

            // Push to dock
            vm.OnPicturePush.Execute(new Position(0, 0)).Subscribe();
            Assert.That(vm.Edited, Is.True, "Active dock item should be marked as edited only after Push to dock");

            // Undo Push
            _mainViewModel.UndoCommand.Execute().Subscribe();
            Assert.That(vm.Edited, Is.False, "Edited flag should be reverted to false after undoing Push");
        }

        [AvaloniaTest]
        public void CanvasDraw_ShouldNotMarkActiveDockAsEdited_UntilPush()
        {
            var picture = Picture.CreateEmpty(new PictureSize(10, 10));
            var vm = new DockPictureViewModel(_state, _mainViewModel.AnimationViewModel, new AvaloniaBitmapAdapter(), Mock.Of<IPictureFileIO>());
            vm.Initialize(picture, new FilePath("test.png"));
            _mainViewModel.Pictures.Add(vm);
            _mainViewModel.ActiveDockable = new Document { DataContext = vm };

            vm.OnPicturePull.Execute(new PictureArea(new Position(0, 0), picture.Size)).Subscribe();
            Assert.That(vm.Edited, Is.False, "Not edited after Pull");

            // Simulate draw event on canvas
            var prevPic = Picture.CreateEmpty(new PictureSize(10, 10));
            _mainViewModel.DrawableCanvasViewModel.OnDrew.Execute(prevPic).Subscribe();

            // 作業エリアで描画しても、ドックに Push されるまでは Edited は false のままであるべき
            Assert.That(vm.Edited, Is.False, "Active dock item should NOT be marked as edited when drawing on canvas");

            // Push to dock
            vm.OnPicturePush.Execute(new Position(0, 0)).Subscribe();
            Assert.That(vm.Edited, Is.True, "Active dock item should be marked as edited after Push");

            // Undo Push
            _mainViewModel.UndoCommand.Execute().Subscribe();
            Assert.That(vm.Edited, Is.False, "Edited flag should be reverted after Undo");
        }
    }
}






