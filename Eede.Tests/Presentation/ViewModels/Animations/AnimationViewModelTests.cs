using Eede.Application.Animations;
using Eede.Domain.Animations;
using Eede.Application.Infrastructure;
using Eede.Presentation.ViewModels.Animations;
using Eede.Application.UseCase.Animations;
using Eede.Presentation.Common.Adapters;
using Moq;
using NUnit.Framework;
using Avalonia.Headless.NUnit;
using System;
using System.Collections.Generic;

namespace Eede.Presentation.Tests.ViewModels.Animations;

public class AnimationViewModelTests
{
    private Mock<IFileSystem> _fileSystemMock;
    private AnimationViewModel _viewModel;
    private AnimationPatternsProvider _patternsProvider;

    [SetUp]
    public void SetUp()
    {
        _fileSystemMock = new Mock<IFileSystem>();
        _patternsProvider = new AnimationPatternsProvider();
        var patternEditor = new AnimationPatternEditor(
            new AddAnimationPatternUseCase(_patternsProvider),
            new ReplaceAnimationPatternUseCase(_patternsProvider),
            new RemoveAnimationPatternUseCase(_patternsProvider));
        _viewModel = new AnimationViewModel(
            _patternsProvider,
            patternEditor,
            _fileSystemMock.Object,
            new AvaloniaBitmapAdapter());
    }

    [AvaloniaTest]
    public void ShouldInitializeWithDefaultValues()
    {
        Assert.That(_viewModel.GridWidth, Is.EqualTo(32));
        Assert.That(_viewModel.GridHeight, Is.EqualTo(32));
        Assert.That(_viewModel.WaitTime, Is.EqualTo(100));
        Assert.That(_viewModel.Patterns.Count, Is.EqualTo(1));
    }

    [AvaloniaTest]
    public void AddFrame_WhenNoPatterns_ShouldNotThrowException()
    {
        // First pattern is added automatically if Patterns is empty during initialization.
        // We simulate the removal of all patterns.
        _viewModel.RemovePatternCommand.Execute().Subscribe();

        Assert.That(_viewModel.Patterns.Count, Is.EqualTo(0), "Patterns collection should be empty.");
        Assert.That(_viewModel.SelectedPattern, Is.Null, "SelectedPattern should be null.");

        // Action under test
        // AddFrame in Eede.Presentation/ViewModels/Animations/AnimationViewModel.cs is public and takes an int.
        Assert.DoesNotThrow(() => _viewModel.AddFrame(0), "AddFrame should handle empty patterns gracefully.");
    }

    [AvaloniaTest]
    public void MoveFrameCommand_ForwardMove_ShouldReorderAndFollowCurrentFrame()
    {
        // 初期状態: 4フレーム (0, 1, 2, 1)
        _viewModel.CurrentFrameIndex = 1;
        // fromIndex 1 を targetSlot 3 (アイテム2の後ろ、toIndex=2) へ移動
        _viewModel.MoveFrameCommand.Execute((1, 3)).Subscribe();

        var frames = _viewModel.SelectedPattern!.Frames;
        Assert.Multiple(() =>
        {
            Assert.That(frames[0].CellIndex, Is.EqualTo(0));
            Assert.That(frames[1].CellIndex, Is.EqualTo(2));
            Assert.That(frames[2].CellIndex, Is.EqualTo(1));
            Assert.That(frames[3].CellIndex, Is.EqualTo(1));
            // 選択されていたコマ(1)のインデックスが toIndex (2) に追従
            Assert.That(_viewModel.CurrentFrameIndex, Is.EqualTo(2));
        });
    }

    [AvaloniaTest]
    public void MoveFrameCommand_BackwardMove_ShouldReorderAndFollowCurrentFrame()
    {
        // 初期状態: (0, 1, 2, 1)
        _viewModel.CurrentFrameIndex = 2; // CellIndex 2
        // fromIndex 2 を targetSlot 0 (先頭、toIndex=0) へ移動
        _viewModel.MoveFrameCommand.Execute((2, 0)).Subscribe();

        var frames = _viewModel.SelectedPattern!.Frames;
        Assert.Multiple(() =>
        {
            Assert.That(frames[0].CellIndex, Is.EqualTo(2));
            Assert.That(frames[1].CellIndex, Is.EqualTo(0));
            Assert.That(frames[2].CellIndex, Is.EqualTo(1));
            Assert.That(frames[3].CellIndex, Is.EqualTo(1));
            Assert.That(_viewModel.CurrentFrameIndex, Is.EqualTo(0));
        });
    }

    [AvaloniaTest]
    public void MoveFrameCommand_OtherFrameMovedOverCurrent_ShouldAdjustIndex()
    {
        // 初期状態: (0, 1, 2, 1)
        _viewModel.CurrentFrameIndex = 2; // CellIndex 2
        // fromIndex 0 を targetSlot 3 (toIndex = 2) へ移動 (前方から後方へ抜き去る)
        _viewModel.MoveFrameCommand.Execute((0, 3)).Subscribe();

        var frames = _viewModel.SelectedPattern!.Frames;
        Assert.Multiple(() =>
        {
            Assert.That(frames[0].CellIndex, Is.EqualTo(1));
            Assert.That(frames[1].CellIndex, Is.EqualTo(2)); // 元の CellIndex 2 は index 1 へシフト
            Assert.That(frames[2].CellIndex, Is.EqualTo(0));
            Assert.That(frames[3].CellIndex, Is.EqualTo(1));
            // CurrentFrameIndex も 2 から 1 に追従
            Assert.That(_viewModel.CurrentFrameIndex, Is.EqualTo(1));
        });
    }

    [AvaloniaTest]
    public void MoveFrameCommand_SamePositionOrInvalid_ShouldNotChangeOrder()
    {
        _viewModel.CurrentFrameIndex = 1;
        // 同一位置への移動
        _viewModel.MoveFrameCommand.Execute((1, 1)).Subscribe();
        _viewModel.MoveFrameCommand.Execute((1, 2)).Subscribe(); // toIndex = 1
        // 不正なインデックス
        _viewModel.MoveFrameCommand.Execute((-1, 2)).Subscribe();
        _viewModel.MoveFrameCommand.Execute((1, 99)).Subscribe();

        var frames = _viewModel.SelectedPattern!.Frames;
        Assert.Multiple(() =>
        {
            Assert.That(frames.Count, Is.EqualTo(4));
            Assert.That(frames[0].CellIndex, Is.EqualTo(0));
            Assert.That(frames[1].CellIndex, Is.EqualTo(1));
            Assert.That(frames[2].CellIndex, Is.EqualTo(2));
            Assert.That(frames[3].CellIndex, Is.EqualTo(1));
            Assert.That(_viewModel.CurrentFrameIndex, Is.EqualTo(1));
        });
    }

    [AvaloniaTest]
    public void RemoveFrameIndexCommand_ShouldRemoveExactPhysicalIndex_AvoidingValueEqualityTrap()
    {
        // (0, 1, 2, 1) で末尾の 1 (物理インデックス 3) を削除
        _viewModel.CurrentFrameIndex = 3;
        _viewModel.RemoveFrameIndexCommand.Execute(3).Subscribe();

        var frames = _viewModel.SelectedPattern!.Frames;
        Assert.Multiple(() =>
        {
            Assert.That(frames.Count, Is.EqualTo(3));
            Assert.That(frames[0].CellIndex, Is.EqualTo(0));
            Assert.That(frames[1].CellIndex, Is.EqualTo(1)); // インデックス 1 の 1 は維持される
            Assert.That(frames[2].CellIndex, Is.EqualTo(2));
            Assert.That(_viewModel.CurrentFrameIndex, Is.EqualTo(2)); // 末尾削除でクランプ
        });
    }

    [AvaloniaTest]
    public void IsPreviewExpanded_WhenFalse_ShouldBypassPreviewBitmap()
    {
        Assert.That(_viewModel.IsPreviewExpanded, Is.True);
        _viewModel.IsPreviewExpanded = false;
        Assert.That(_viewModel.PreviewBitmap, Is.Null);
    }
}

