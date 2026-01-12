using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Services;
using Eede.Presentation.ViewModels.Animations;
using Moq;
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
        var mockStorage = new Mock<IStorageService>();
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
        mockFileSystem.Verify(x => x.WriteAllTextAsync(uri.LocalPath, It.IsAny<string>()), Times.Once);

        // 4. Import into a fresh ViewModel
        var freshService = new AnimationService();
        var freshViewModel = new AnimationViewModel(freshService, mockFileSystem.Object);
        mockStorage.Setup(x => x.OpenAnimationFilePickerAsync()).ReturnsAsync(uri);
        mockFileSystem.Setup(x => x.ReadAllTextAsync(uri.LocalPath)).ReturnsAsync(savedJson);

        await freshViewModel.ImportCommand.Execute(mockStorage.Object).FirstAsync();

        // 5. Verify imported state
        Assert.That(freshViewModel.Patterns.Count, Is.EqualTo(2)); // Default "Test Run" + Imported
        var imported = freshViewModel.Patterns[1];
        Assert.That(imported.Name, Is.EqualTo("New Animation"));
        Assert.That(imported.Frames.Count, Is.EqualTo(2));
        Assert.That(imported.Frames[1].CellIndex, Is.EqualTo(20));
        Assert.That(imported.Frames[1].Duration, Is.EqualTo(150));
    }
}
