using Eede.Application.Animations;
using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Eede.Application.Tests.Animations;

[TestFixture]
public class AnimationPatternEditorTests
{
    private Mock<IAddAnimationPatternUseCase> _addUseCaseMock;
    private Mock<IReplaceAnimationPatternUseCase> _replaceUseCaseMock;
    private Mock<IRemoveAnimationPatternUseCase> _removeUseCaseMock;
    private AnimationPatternEditor _editor;

    [SetUp]
    public void Setup()
    {
        _addUseCaseMock = new Mock<IAddAnimationPatternUseCase>();
        _replaceUseCaseMock = new Mock<IReplaceAnimationPatternUseCase>();
        _removeUseCaseMock = new Mock<IRemoveAnimationPatternUseCase>();

        _editor = new AnimationPatternEditor(
            _addUseCaseMock.Object,
            _replaceUseCaseMock.Object,
            _removeUseCaseMock.Object);
    }

    [Test]
    public void Add_CallsExecuteOnAddUseCase()
    {
        // Arrange
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0));

        // Act
        _editor.Add(pattern);

        // Assert
        _addUseCaseMock.Verify(m => m.Execute(pattern), Times.Once);
    }

    [Test]
    public void Replace_CallsExecuteOnReplaceUseCase()
    {
        // Arrange
        int index = 1;
        var pattern = new AnimationPattern("Test", new List<AnimationFrame>(), new GridSettings(new PictureSize(16, 16), new Position(0, 0), 0));

        // Act
        _editor.Replace(index, pattern);

        // Assert
        _replaceUseCaseMock.Verify(m => m.Execute(index, pattern), Times.Once);
    }

    [Test]
    public void Remove_CallsExecuteOnRemoveUseCase()
    {
        // Arrange
        int index = 2;

        // Act
        _editor.Remove(index);

        // Assert
        _removeUseCaseMock.Verify(m => m.Execute(index), Times.Once);
    }
}

