using Eede.Presentation.Common.Services;
using Eede.Application.Animations;
using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using Eede.Application.Infrastructure;
using Eede.Presentation.ViewModels.Animations;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Eede.Presentation.Tests.ViewModels.Animations;

public class AnimationIOViewModelTests
{
    private AnimationPatternsProvider _patternsProvider;
    private Mock<IFileSystem> _fileSystemMock;

    [SetUp]
    public void Setup()
    {
        _patternsProvider = new AnimationPatternsProvider();
        _fileSystemMock = new Mock<IFileSystem>();
    }

    private AnimationViewModel CreateViewModel()
    {
        return new AnimationViewModel(
            _patternsProvider,
            new AddAnimationPatternUseCase(_patternsProvider),
            new ReplaceAnimationPatternUseCase(_patternsProvider),
            new RemoveAnimationPatternUseCase(_patternsProvider),
            _fileSystemMock.Object);
    }

    [Test]
    public async Task ExportCommand_ShouldSaveJsonToFile()
    {
        // Arrange
        var mockStorage = new Mock<IFileStorage>();
        var pattern = new AnimationPattern("Test Pattern", new List<AnimationFrame>(), new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0));
        var viewModel = CreateViewModel();
        viewModel.CreatePatternCommand.Execute("Test Pattern").Subscribe();
        viewModel.SelectedPattern = viewModel.Patterns[1]; // Index 0 is default pattern

        var uri = new Uri("file:///C:/test.json");
        mockStorage.Setup(x => x.SaveAnimationFilePickerAsync()).ReturnsAsync(uri);

        string savedJson = "";
        _fileSystemMock.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((path, json) => savedJson = json)
            .Returns(Task.CompletedTask);

        // Act
        await viewModel.ExportCommand.Execute(mockStorage.Object).FirstAsync();

        // Assert
        mockStorage.Verify(x => x.SaveAnimationFilePickerAsync(), Times.Once);
        _fileSystemMock.Verify(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        Assert.That(savedJson, Does.Contain("Test Pattern"));
    }

    [Test]
    public async Task ImportCommand_ShouldLoadJsonFromFile()
    {
        // Arrange
        var mockStorage = new Mock<IFileStorage>();
        var viewModel = CreateViewModel();

        var uri = new Uri("file:///C:/test.json");
        mockStorage.Setup(x => x.OpenFilePickerAsync()).ReturnsAsync(uri);

        string json = "{\"Name\":\"Test Pattern\",\"Frames\":[],\"Grid\":{\"CellSize\":{\"Width\":32,\"Height\":32},\"Offset\":{\"X\":0,\"Y\":0},\"Spacing\":0}}";
        _fileSystemMock.Setup(x => x.ReadAllTextAsync(It.IsAny<string>())).ReturnsAsync(json);

        // Act
        await viewModel.ImportCommand.Execute(mockStorage.Object).FirstAsync();

        // Assert
        mockStorage.Verify(x => x.OpenFilePickerAsync(), Times.Once);
        _fileSystemMock.Verify(x => x.ReadAllTextAsync(It.IsAny<string>()), Times.Once);
        Assert.That(viewModel.Patterns.Count, Is.EqualTo(2)); // Initial + Imported
        Assert.That(viewModel.Patterns[1].Name, Is.EqualTo("Test Pattern"));
    }
}