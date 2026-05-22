using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using Eede.Presentation.ViewModels.Animations;
using Eede.Presentation.Common.Adapters;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Eede.Presentation.Tests.ViewModels.Animations;

public class AnimationIOViewModelTests
{
    private Mock<IFileSystem> _fileSystemMock;
    private AnimationViewModel _viewModel;
    private AnimationPatternsProvider _patternsProvider;

    [SetUp]
    public void SetUp()
    {
        _fileSystemMock = new Mock<IFileSystem>();
        _patternsProvider = new AnimationPatternsProvider();
        var patternService = new AnimationPatternService(
            new AddAnimationPatternUseCase(_patternsProvider),
            new ReplaceAnimationPatternUseCase(_patternsProvider),
            new RemoveAnimationPatternUseCase(_patternsProvider));
        _viewModel = new AnimationViewModel(
            _patternsProvider,
            patternService,
            _fileSystemMock.Object,
            new AvaloniaBitmapAdapter());
    }

    [Test]
    public async Task ExportCommand_ShouldSaveToFile()
    {
        var mockStorage = new Mock<IFileStorage>();
        var uri = new Uri("file:///path/to/animation.json");
        mockStorage.Setup(s => s.SaveAnimationFilePickerAsync()).ReturnsAsync(uri);

        _viewModel.SelectedPattern = new AnimationPattern("ExportTest", new List<AnimationFrame>(), new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0));

        await _viewModel.ExportCommand.Execute(mockStorage.Object);

        _fileSystemMock.Verify(fs => fs.WriteAllTextAsync(uri.LocalPath, It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ImportCommand_ShouldLoadFromFile()
    {
        var mockStorage = new Mock<IFileStorage>();
        var uri = new Uri("file:///path/to/animation.json");
        mockStorage.Setup(s => s.OpenAnimationFilePickerAsync()).ReturnsAsync(uri);

        var json = "{\"Name\":\"ImportTest\",\"Frames\":[],\"Grid\":{\"CellSize\":{\"Width\":32,\"Height\":32},\"Offset\":{\"X\":0,\"Y\":0},\"Padding\":0}}";
        _fileSystemMock.Setup(fs => fs.ReadAllTextAsync(uri.LocalPath)).ReturnsAsync(json);

        int initialCount = _viewModel.Patterns.Count;

        await _viewModel.ImportCommand.Execute(mockStorage.Object);

        Assert.That(_viewModel.Patterns.Count, Is.EqualTo(initialCount + 1));
        Assert.That(_viewModel.Patterns.Last().Name, Is.EqualTo("ImportTest"));
    }
}
