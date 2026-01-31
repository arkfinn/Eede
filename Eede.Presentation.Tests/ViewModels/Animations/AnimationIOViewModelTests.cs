using Eede.Presentation.Common.Services;
using Eede.Application.Animations;
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
    [Test]
    public async Task ExportCommand_ShouldSaveJsonToFile()
    {
        // Arrange
        var mockService = new Mock<IAnimationService>();
        mockService.Setup(x => x.Patterns).Returns(new List<AnimationPattern>());
        var mockStorage = new Mock<IFileStorage>();
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
        var mockStorage = new Mock<IFileStorage>();
        var mockFileSystem = new Mock<IFileSystem>();

        var viewModel = new AnimationViewModel(mockService.Object, mockFileSystem.Object);

        var uri = new Uri("file:///C:/test.json");
        mockStorage.Setup(x => x.OpenFilePickerAsync()).ReturnsAsync(uri);

        string json = "{\"Name\":\"Test Pattern\",\"Frames\":[],\"Grid\":{\"CellSize\":{\"Width\":32,\"Height\":32},\"Offset\":{\"X\":0,\"Y\":0},\"Spacing\":0}}";
        mockFileSystem.Setup(x => x.ReadAllTextAsync(It.IsAny<string>())).ReturnsAsync(json);

        // Act
        await viewModel.ImportCommand.Execute(mockStorage.Object).FirstAsync();

                // Assert

                mockStorage.Verify(x => x.OpenFilePickerAsync(), Times.Once);

                mockFileSystem.Verify(x => x.ReadAllTextAsync(It.IsAny<string>()), Times.Once);

                mockService.Verify(x => x.Add(It.IsAny<AnimationPattern>()), Times.Exactly(2));

            }

        }

        