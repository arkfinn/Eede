using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.SharedKernel;
using NUnit.Framework;
using System;

namespace Eede.Domain.Tests.ImageEditing;

[TestFixture]
public class CharacterizationSafetyTests
{
    [Test]
    public void DrawingSession_Constructor_ThrowsArgumentNullException_WhenInitialPictureIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new DrawingSession(null!);
        });
    }

    [Test]
    public void DrawingSession_Push_ThrowsArgumentNullException_WhenNextPictureIsNull()
    {
        var picture = Picture.CreateEmpty(new PictureSize(1, 1));
        var session = new DrawingSession(picture);
        Assert.Throws<ArgumentNullException>(() =>
        {
            session.Push(null!);
        });
    }

    [Test]
    public void DrawingBuffer_Constructor_ThrowsArgumentNullException_WhenPreviousIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = new DrawingBuffer(null!);
        });
    }

    [Test]
    public void Picture_Create_ThrowsException_WhenImageDataIsNull()
    {
        // 現状はガード節がないため NullReferenceException が発生するはず
        Assert.Throws<NullReferenceException>(() =>
        {
            Picture.Create(new PictureSize(1, 1), null!);
        });
    }
}
