using Eede.Presentation.Common.Services;
using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
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

public class AnimationIntegrationTests
{
    [Test]
    public async Task Create_Edit_Export_Import_Workflow_ShouldWork()
    {
        // Arrange
        var mockService = new AnimationService();
        var mockStorage = new Mock<IFileStorage>();
        var mockFileSystem = new Mock<IFileSystem>();
        var viewModel = new AnimationViewModel(mockService, mockFileSystem.Object);

        // 1. Create Pattern
        viewModel.CreatePatternCommand.Execute("New Animation").Subscribe();
        Assert.That(viewModel.SelectedPattern, Is.Not.Null);
        Assert.That(viewModel.SelectedPattern!.Name, Is.EqualTo("New Animation"));

        // 2. Add Frames
        viewModel.WaitTime = 150;
        await viewModel.AddFrameCommand.Execute(10).FirstAsync();
        await viewModel.AddFrameCommand.Execute(20).FirstAsync();
        
        Assert.That(viewModel.SelectedPattern.Frames.Count, Is.EqualTo(2));
        Assert.That(viewModel.SelectedPattern.Frames[0].CellIndex, Is.EqualTo(10));
        Assert.That(viewModel.SelectedPattern.Frames[0].Duration, Is.EqualTo(150));

        // 3. Export
        var uri = new Uri("file:///C:/export.json");
        mockStorage.Setup(x => x.SaveAnimationFilePickerAsync()).ReturnsAsync(uri);
        string savedJson = "";
        mockFileSystem.Setup(x => x.WriteAllTextAsync(uri.LocalPath, It.IsAny<string>()))
            .Callback<string, string>((path, json) => savedJson = json)
            .Returns(Task.CompletedTask);

        await viewModel.ExportCommand.Execute(mockStorage.Object).FirstAsync();
        mockFileSystem.Verify(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        // 4. Import
        var importUri = new Uri("file:///C:/import.json");
        mockStorage.Setup(x => x.OpenFilePickerAsync()).ReturnsAsync(importUri);
        mockFileSystem.Setup(x => x.ReadAllTextAsync(importUri.LocalPath)).ReturnsAsync(savedJson);

        await viewModel.ImportCommand.Execute(mockStorage.Object).FirstAsync();

        // 5. Verify Import
        Assert.That(viewModel.Patterns.Count, Is.EqualTo(3)); // Initial(1) + Created(1) + Imported(1)
        var importedPattern = viewModel.SelectedPattern;
        Assert.That(importedPattern.Name, Is.EqualTo("New Animation"));
        Assert.That(importedPattern.Frames.Count, Is.EqualTo(2));
        Assert.That(importedPattern.Frames[0].CellIndex, Is.EqualTo(10));
    }
}