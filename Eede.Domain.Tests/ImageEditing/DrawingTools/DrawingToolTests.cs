using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Moq;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing.DrawingTools;

public class DrawingToolTests
{
    private Mock<IDrawStyle> _mockStyle;
    private PenStyle _penStyle;
    private ArgbColor _color = new(255, 0, 0, 0);

    [SetUp]
    public void Setup()
    {
        _mockStyle = new Mock<IDrawStyle>();
        _penStyle = new PenStyle(new DirectImageBlender(), _color, 1);
    }

    [Test]
    public void DrawingTool_DelegatesToStyle()
    {
        var tool = new DrawingTool(_mockStyle.Object, _penStyle);
        var buffer = new DrawingBuffer(Picture.CreateEmpty(new PictureSize(32, 32)));
        var history = new CoordinateHistory(new CanvasCoordinate(0, 0));
        
        tool.DrawStart(buffer, history, false);
        _mockStyle.Verify(s => s.DrawStart(buffer, _penStyle, history, false), Times.Once);

        tool.Drawing(buffer, history, false);
        _mockStyle.Verify(s => s.Drawing(buffer, _penStyle, history, false), Times.Once);

        tool.DrawEnd(buffer, history, false);
        _mockStyle.Verify(s => s.DrawEnd(buffer, _penStyle, history, false), Times.Once);
    }

    [Test]
    public void DrawingTool_Updates_ReturnsNewInstance()
    {
        var tool = new DrawingTool(_mockStyle.Object, _penStyle);
        var newColor = new ArgbColor(255, 255, 0, 0);
        
        var updatedTool = tool.WithColor(newColor);
        
        Assert.Multiple(() =>
        {
            Assert.That(updatedTool.PenStyle.Color, Is.EqualTo(newColor));
            Assert.That(tool.PenStyle.Color, Is.EqualTo(_color), "Original tool should not be changed");
            Assert.That(updatedTool.Style, Is.EqualTo(tool.Style));
        });
    }
}
