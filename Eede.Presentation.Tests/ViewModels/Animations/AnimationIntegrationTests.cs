using Eede.Application.Animations;
using Eede.Application.Infrastructure;
using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using Eede.Presentation.ViewModels.Animations;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Eede.Presentation.Tests.ViewModels.Animations;

public class AnimationIntegrationTests
{
    [Test]
    public async Task CreateAndAddFrameIntegration()
    {
        var patternsProvider = new AnimationPatternsProvider();
        var mockFileSystem = new Mock<IFileSystem>();
        var viewModel = new AnimationViewModel(
            patternsProvider,
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider),
            mockFileSystem.Object);

        var initialCount = viewModel.Patterns.Count;
        await viewModel.CreatePatternCommand.Execute("NewPattern");
        
        Assert.That(viewModel.Patterns.Count, Is.EqualTo(initialCount + 1));
        Assert.That(viewModel.SelectedPattern?.Name, Is.EqualTo("NewPattern"));

        var selectedPattern = viewModel.SelectedPattern;
        await viewModel.AddFrameCommand.Execute(5);

        Assert.That(viewModel.SelectedPattern?.Frames.Count, Is.EqualTo(1));
        Assert.That(viewModel.SelectedPattern?.Frames[0].CellIndex, Is.EqualTo(5));
    }

    [Test]
    public async Task DeletePatternIntegration()
    {
        var patternsProvider = new AnimationPatternsProvider();
        var viewModel = new AnimationViewModel(
            patternsProvider,
            new AddAnimationPatternUseCase(patternsProvider),
            new ReplaceAnimationPatternUseCase(patternsProvider),
            new RemoveAnimationPatternUseCase(patternsProvider),
            new Mock<IFileSystem>().Object);

        await viewModel.CreatePatternCommand.Execute("ToDelete");
        var countAfterAdd = viewModel.Patterns.Count;
        
        viewModel.SelectedPattern = viewModel.Patterns.Last();
        await viewModel.RemovePatternCommand.Execute();

        Assert.That(viewModel.Patterns.Count, Is.EqualTo(countAfterAdd - 1));
        Assert.That(viewModel.Patterns.Any(p => p.Name == "ToDelete"), Is.False);
    }
}