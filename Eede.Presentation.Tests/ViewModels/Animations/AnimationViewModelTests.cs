using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Services;
using Eede.Presentation.ViewModels.Animations;
using Microsoft.Reactive.Testing;
using Moq;
using ReactiveUI;
using ReactiveUI.Testing;

namespace Eede.Presentation.Tests.ViewModels.Animations;

public class AnimationViewModelTests
{
    [Test]
    public void CurrentFrameIndex_ShouldIncrementBasedOnDuration()
    {
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>
        {
            new AnimationFrame(0, 100),
            new AnimationFrame(1, 200),
            new AnimationFrame(2, 300)
        }, new GridSettings(new(32, 32), new(0, 0), 0));

        var mockService = new Mock<IAnimationService>();
        mockService.Setup(x => x.Patterns).Returns(new List<AnimationPattern> { pattern });

        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            
            var viewModel = new AnimationViewModel(mockService.Object, new Mock<IFileSystem>().Object);
            viewModel.SelectedPattern = pattern;

            Assert.That(viewModel.CurrentFrameIndex, Is.EqualTo(0));

            viewModel.IsPlaying = true;

            // Process IsPlaying change notification and start timer
            scheduler.AdvanceBy(1);

            // Timer starts. Frame 0 duration is 100ms.
            // Advance to just before 100ms
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(99).Ticks);
            Assert.That(viewModel.CurrentFrameIndex, Is.EqualTo(0));

            // Advance to 100ms
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(1).Ticks);
            Assert.That(viewModel.CurrentFrameIndex, Is.EqualTo(1));

            // Frame 1 duration is 200ms. Advance to 300ms
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(200).Ticks);
            Assert.That(viewModel.CurrentFrameIndex, Is.EqualTo(2));

            // Frame 2 duration is 300ms. Advance to 600ms
            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(300).Ticks);
            Assert.That(viewModel.CurrentFrameIndex, Is.EqualTo(0));
            scheduler.AdvanceBy(1); // Ensure OAPH updates
            Assert.That(viewModel.CurrentFrame, Is.Not.Null);
            Assert.That(viewModel.CurrentFrame!.CellIndex, Is.EqualTo(0));
        });
    }

    [Test]
    public void CurrentFrame_ShouldUpdateWhenIndexChanges()
    {
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>
        {
            new AnimationFrame(10, 100),
            new AnimationFrame(20, 100)
        }, new GridSettings(new(32, 32), new(0, 0), 0));

        var mockService = new Mock<IAnimationService>();
        mockService.Setup(x => x.Patterns).Returns(new List<AnimationPattern> { pattern });

        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new AnimationViewModel(mockService.Object, new Mock<IFileSystem>().Object);
            // ViewModel constructor might have set SelectedPattern already if Patterns was empty,
            // but here mockService returns pattern, so Patterns will have it.
            // The constructor will set SelectedPattern = Patterns[0] if Patterns is empty, 
            // wait, it only adds if Patterns.Count == 0.
            
            viewModel.SelectedPattern = pattern;
            scheduler.AdvanceBy(1); // Process SelectedPattern change

            Assert.That(viewModel.CurrentFrameIndex, Is.EqualTo(0));
            scheduler.AdvanceBy(1); // Process CurrentFrameIndex=0 and update OAPH
            Assert.That(viewModel.CurrentFrame, Is.Not.Null);
            Assert.That(viewModel.CurrentFrame!.CellIndex, Is.EqualTo(10));

            viewModel.CurrentFrameIndex = 1;
            scheduler.AdvanceBy(1); // Process index change and update OAPH
            Assert.That(viewModel.CurrentFrame, Is.Not.Null);
            Assert.That(viewModel.CurrentFrame!.CellIndex, Is.EqualTo(20));
        });
    }

    [Test]
    public void ActivePicture_Change_ShouldNotResetAnimationState()
    {
        var mockService = new Mock<IAnimationService>();
        mockService.Setup(x => x.Patterns).Returns(new List<AnimationPattern>());
        var mockFileSystem = new Mock<IFileSystem>();

        new TestScheduler().With(scheduler =>
        {
            RxApp.MainThreadScheduler = scheduler;
            var viewModel = new AnimationViewModel(mockService.Object, mockFileSystem.Object);
            viewModel.IsPlaying = true;
            viewModel.CurrentFrameIndex = 0;

            viewModel.ActivePicture = Picture.CreateEmpty(new PictureSize(32, 32));

            Assert.That(viewModel.IsPlaying, Is.True);
            Assert.That(viewModel.CurrentFrameIndex, Is.EqualTo(0));
        });
    }
}
