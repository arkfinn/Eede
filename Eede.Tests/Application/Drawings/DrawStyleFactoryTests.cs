using NUnit.Framework;
using Eede.Application.Drawings;
using Eede.Domain.ImageEditing.DrawingTools;
using System;

namespace Eede.Application.Tests.Drawings
{
    [TestFixture]
    public class DrawStyleFactoryTests
    {
        private DrawStyleFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new DrawStyleFactory();
        }

        [TestCase(DrawStyleType.RegionSelect, typeof(RegionSelector))]
        [TestCase(DrawStyleType.FreeCurve, typeof(FreeCurve))]
        [TestCase(DrawStyleType.Line, typeof(Line))]
        [TestCase(DrawStyleType.Fill, typeof(Fill))]
        [TestCase(DrawStyleType.Rectangle, typeof(Rectangle))]
        [TestCase(DrawStyleType.FilledRectangle, typeof(FilledRectangle))]
        [TestCase(DrawStyleType.Ellipse, typeof(Ellipse))]
        [TestCase(DrawStyleType.FilledEllipse, typeof(FilledEllipse))]
        public void Create_WithValidType_ReturnsExpectedImplementation(DrawStyleType type, Type expectedType)
        {
            var result = _factory.Create(type);
            Assert.That(result, Is.InstanceOf(expectedType));
        }

        [Test]
        public void Create_WithInvalidType_ThrowsArgumentOutOfRangeException()
        {
            var invalidType = (DrawStyleType)999;
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _factory.Create(invalidType));
            Assert.That(ex.ParamName, Is.EqualTo("type"));
        }
    }
}