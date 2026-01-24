using Eede.Application.Drawings;
using Eede.Domain.Animations;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Application.Tests.Drawings;

public class CanvasServiceTests
{
    private CanvasService _service;

    [SetUp]
    public void Setup()
    {
        _service = new CanvasService();
    }

    [Test]
    public void Minify_CorrectlyCalculatesPosition()
    {
        var displayPos = new Position(10, 20);
        var magnification = new Magnification(4);
        
        var result = _service.Minify(displayPos, magnification);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.X, Is.EqualTo(2));
            Assert.That(result.Y, Is.EqualTo(5));
        });
    }

    [Test]
    public void GetCurrentHalfBoxArea_NormalMode()
    {
        var displayPos = new Position(10, 20);
        var magnification = new Magnification(1);
        var defaultGridSize = new PictureSize(16, 16);
        
        var result = _service.GetCurrentHalfBoxArea(displayPos, magnification, false, null, defaultGridSize);
        
        Assert.That(result.BoxSize, Is.EqualTo(defaultGridSize));
    }

    [Test]
    public void GetCurrentHalfBoxArea_AnimationMode()
    {
        var displayPos = new Position(10, 20);
        var magnification = new Magnification(1);
        var gridSettings = new GridSettings(new PictureSize(32, 32), new Position(0, 0), 0);
        
        var result = _service.GetCurrentHalfBoxArea(displayPos, magnification, true, gridSettings, new PictureSize(16, 16));
        
        Assert.That(result.BoxSize, Is.EqualTo(new PictureSize(32, 32)));
    }
}
