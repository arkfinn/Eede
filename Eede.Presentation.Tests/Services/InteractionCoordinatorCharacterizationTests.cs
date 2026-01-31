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
    private CopySelectionUseCase _copySelectionUseCase;
    private CutSelectionUseCase _cutSelectionUseCase;
    private PasteFromClipboardUseCase _pasteFromClipboardUseCase;
    private InteractionCoordinator _coordinator;

        [SetUp]

        public void SetUp()

        {

            _globalState = new GlobalState();

            _addFrameProviderMock = new Mock<IAddFrameProvider>();

            _clipboardServiceMock = new Mock<IClipboard>();

            _bitmapAdapter = new AvaloniaBitmapAdapter();

            _drawingSessionProvider = new DrawingSessionProvider();

            _copySelectionUseCase = new CopySelectionUseCase(_clipboardServiceMock.Object);

            _cutSelectionUseCase = new CutSelectionUseCase(_clipboardServiceMock.Object);

            _pasteFromClipboardUseCase = new PasteFromClipboardUseCase(_clipboardServiceMock.Object);

            _coordinator = new InteractionCoordinator(_drawingSessionProvider); // provider 繧呈ｸ｡縺・
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

                _copySelectionUseCase,

                _cutSelectionUseCase,

                _pasteFromClipboardUseCase,

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

    

            

    

        

            // 32x32 縺ｮ襍､縺・判蜒・
            var red = new ArgbColor(255, 255, 0, 0);

            var initialPicture = CreateFilledPicture(new PictureSize(32, 32), red);

            var vm = CreateViewModel(initialPicture);

            vm.Magnification = new Magnification(1);

            

            // 1. (0,0)-(8,8) 繧帝∈謚・
            vm.DrawStyle = new RegionSelector();

            vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();

            vm.DrawEndCommand.Execute(new Position(8, 8)).Subscribe();

    

            // 2. 繝峨Λ繝・げ遘ｻ蜍・(0,0) -> (10,10)

            vm.DrawBeginCommand.Execute(new Position(4, 4)).Subscribe();

            vm.DrawingCommand.Execute(new Position(14, 14)).Subscribe();

            vm.DrawEndCommand.Execute(new Position(14, 14)).Subscribe();

    

            // 縺薙・譎らせ縺ｧ縺ｯ繝励Ξ繝薙Η繝ｼ迥ｶ諷具ｼ・0,0)縺ｯ騾乗・縲・10,10)縺ｯ繝励Ξ繝薙Η繝ｼ・・
            Assert.That(vm.PictureBuffer.Fetch().PickColor(new Position(0, 0)).Alpha, Is.EqualTo(0));

            Assert.That(vm.PreviewPixels, Is.Not.Null);

    

            // 3. 驕ｸ謚樒ｯ・峇螟・(20,20) 繧偵け繝ｪ繝・け縺励※遒ｺ螳・
            vm.DrawBeginCommand.Execute(new Position(20, 20)).Subscribe();

    

            // 遒ｺ螳壼ｾ鯉ｼ・10,10) 縺瑚ｵ､縺上↑縺｣縺ｦ縺翫ｊ縲√・繝ｬ繝薙Η繝ｼ縺ｯ豸医∴縺ｦ縺・ｋ縺ｯ縺・
            var result = vm.PictureBuffer.Fetch();

            Assert.That(result.PickColor(new Position(10, 10)), Is.EqualTo(red), "遒ｺ螳壼ｾ後・遘ｻ蜍募・縺ｫ逕ｻ蜒上′譖ｸ縺崎ｾｼ縺ｾ繧後※縺・ｋ");

            Assert.That(vm.PreviewPixels, Is.Null, "遒ｺ螳壼ｾ後・繝励Ξ繝薙Η繝ｼ縺後け繝ｪ繧｢縺輔ｌ縺ｦ縺・ｋ");

        }

    

        [AvaloniaTest]

        public void SelectionMove_And_Cancel_Test()

        {

            // 32x32 縺ｮ襍､縺・判蜒・
            var red = new ArgbColor(255, 255, 0, 0);

            var initialPicture = CreateFilledPicture(new PictureSize(32, 32), red);

            var vm = CreateViewModel(initialPicture);

            vm.Magnification = new Magnification(1);

            

            // 1. 遘ｻ蜍墓桃菴・
            vm.DrawStyle = new RegionSelector();

            vm.DrawBeginCommand.Execute(new Position(0, 0)).Subscribe();

            vm.DrawEndCommand.Execute(new Position(8, 8)).Subscribe();

            vm.DrawBeginCommand.Execute(new Position(4, 4)).Subscribe();

            vm.DrawingCommand.Execute(new Position(14, 14)).Subscribe();

            vm.DrawEndCommand.Execute(new Position(14, 14)).Subscribe();

    

            // 2. 蜿ｳ繧ｯ繝ｪ繝・け縺ｧ繧ｭ繝｣繝ｳ繧ｻ繝ｫ

            vm.PointerRightButtonPressedCommand.Execute(new Position(14, 14)).Subscribe();

    

            // 繧ｭ繝｣繝ｳ繧ｻ繝ｫ蠕鯉ｼ・0,0) 縺瑚ｵ､縺ｫ謌ｻ縺｣縺ｦ縺翫ｊ縲√・繝ｬ繝薙Η繝ｼ縺ｯ豸医∴縺ｦ縺・ｋ縺ｯ縺・
            var result = vm.PictureBuffer.Fetch();

            Assert.That(result.PickColor(new Position(0, 0)), Is.EqualTo(red), "繧ｭ繝｣繝ｳ繧ｻ繝ｫ蠕後・蜈・・菴咲ｽｮ縺ｫ逕ｻ蜒上′蠕ｩ蜈・＆繧後※縺・ｋ");

            Assert.That(vm.PreviewPixels, Is.Null, "繧ｭ繝｣繝ｳ繧ｻ繝ｫ蠕後・繝励Ξ繝薙Η繝ｼ縺後け繝ｪ繧｢縺輔ｌ縺ｦ縺・ｋ");

        }

    }

    

    
