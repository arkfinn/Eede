using Eede.Domain.ImageEditing.Filters;
using NUnit.Framework;
using System;
using System.Reflection;

namespace Eede.Domain.Tests.ImageEditing.Filters
{
    [TestFixture]
    public class AntiAliasFilterFactoryTests
    {
        [Test]
        [TestCase(AntiAliasMode.Rgb, typeof(RgbAntiAliasStrategy))]
        [TestCase(AntiAliasMode.Alpha, typeof(AlphaAntiAliasStrategy))]
        [TestCase(AntiAliasMode.Argb, typeof(ArgbAntiAliasStrategy))]
        public void Create_WithValidMode_ReturnsFilterWithCorrectStrategy(AntiAliasMode mode, Type expectedStrategyType)
        {
            var filter = AntiAliasFilterFactory.Create(mode);

            Assert.That(filter, Is.Not.Null);

            var strategyField = typeof(AntiAliasFilter).GetField("Strategy", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(strategyField, Is.Not.Null, "Private field 'Strategy' not found.");

            var strategy = strategyField.GetValue(filter);
            Assert.That(strategy, Is.InstanceOf(expectedStrategyType));
        }

        [Test]
        public void Create_WithValidMode_SetsDefaultMagnificationFactorToOne()
        {
            var filter = AntiAliasFilterFactory.Create(AntiAliasMode.Rgb);

            var magField = typeof(AntiAliasFilter).GetField("MagnificationFactor", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(magField, Is.Not.Null, "Private field 'MagnificationFactor' not found.");

            var magValue = magField.GetValue(filter);
            Assert.That(magValue, Is.EqualTo(1));
        }

        [Test]
        public void Create_WithMagnificationFactor_SetsMagnificationFactorCorrectly()
        {
            int expectedFactor = 3;
            var filter = AntiAliasFilterFactory.Create(AntiAliasMode.Rgb, expectedFactor);

            var magField = typeof(AntiAliasFilter).GetField("MagnificationFactor", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(magField, Is.Not.Null, "Private field 'MagnificationFactor' not found.");

            var magValue = magField.GetValue(filter);
            Assert.That(magValue, Is.EqualTo(expectedFactor));
        }

        [Test]
        public void Create_WithInvalidMode_ThrowsArgumentException()
        {
            AntiAliasMode invalidMode = (AntiAliasMode)999;

            var ex = Assert.Throws<ArgumentException>(() => AntiAliasFilterFactory.Create(invalidMode));
            Assert.That(ex.ParamName, Is.EqualTo("mode"));
        }
    }
}
