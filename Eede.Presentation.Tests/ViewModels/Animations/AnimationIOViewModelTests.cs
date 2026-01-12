using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Services;
using Eede.Presentation.ViewModels.Animations;
using Moq;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Eede.Presentation.Tests.ViewModels.Animations;

public class AnimationIOViewModelTests
{
    [Test]
    public async Task ExportCommand_ShouldSaveJsonToFile()
    {
        // Arrange
        var mockService = new Mock<IAnimationService>();
        mockService.Setup(x => x.Patterns).Returns(new List<AnimationPattern>());
        var mockStorage = new Mock<IStorageService>();
        var mockFileSystem = new Mock<IFileSystem>();

        var pattern = new AnimationPattern("Test Pattern", new List<AnimationFrame>(), new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0));
        var viewModel = new AnimationViewModel(mockService.Object, mockFileSystem.Object);
        viewModel.Patterns.Add(pattern);
        viewModel.SelectedPattern = pattern;

        var uri = new Uri("file:///C:/test.json");
        mockStorage.Setup(x => x.SaveAnimationFilePickerAsync()).ReturnsAsync(uri);

        string savedJson = "";
        mockFileSystem.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((path, json) => savedJson = json)
            .Returns(Task.CompletedTask);

        // Act
        await viewModel.ExportCommand.Execute(mockStorage.Object).FirstAsync();

        // Assert
        mockStorage.Verify(x => x.SaveAnimationFilePickerAsync(), Times.Once);
        mockFileSystem.Verify(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        Assert.That(savedJson, Does.Contain("Test Pattern"));
    }

    [Test]
    public async Task ImportCommand_ShouldLoadJsonFromFile()
    {
        // Arrange
        var mockService = new Mock<IAnimationService>();
        mockService.Setup(x => x.Patterns).Returns(new List<AnimationPattern>());
        var mockStorage = new Mock<IStorageService>();
        var mockFileSystem = new Mock<IFileSystem>();

        var viewModel = new AnimationViewModel(mockService.Object, mockFileSystem.Object);

        var uri = new Uri("file:///C:/test.json");
        mockStorage.Setup(x => x.OpenAnimationFilePickerAsync()).ReturnsAsync(uri);

        var json = "{\"Name\":\"Imported Pattern\",\"Frames\":[],\"Grid\":{\"CellSize\":{\"Width\":16,\"Height\":16},\"Offset\":{\"X\":0,\"Y\":0},\"Padding\":0}}";
        mockFileSystem.Setup(x => x.ReadAllTextAsync(It.IsAny<string>())).ReturnsAsync(json);

        // Act
        await viewModel.ImportCommand.Execute(mockStorage.Object).FirstAsync();

        // Assert
        mockService.Verify(x => x.Add(It.Is<AnimationPattern>(p => p.Name == "Imported Pattern")), Times.Once);
        Assert.That(viewModel.Patterns.Count, Is.EqualTo(2));
        Assert.That(viewModel.Patterns[1].Name, Is.EqualTo("Imported Pattern"));
        Assert.That(viewModel.SelectedPattern, Is.Not.Null);
        Assert.That(viewModel.SelectedPattern!.Name, Is.EqualTo("Imported Pattern"));
    }
}
