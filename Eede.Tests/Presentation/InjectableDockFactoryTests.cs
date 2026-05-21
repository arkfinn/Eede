using System;
using Dock.Model.Core;
using Eede.Presentation;
using Moq;
using NUnit.Framework;

namespace Eede.Presentation.Tests;

[TestFixture]
public class InjectableDockFactoryTests
{
    private InjectableDockFactory _factory;

    [SetUp]
    public void Setup()
    {
        _factory = new InjectableDockFactory();
    }

    [Test]
    public void SetFocusedDockable_ShouldFireActiveDockableChanged()
    {
        // Arrange
        var mockDockable = new Mock<IDockable>();
        IDockable? receivedDockable = null;
        int callCount = 0;

        _factory.ActiveDockableChanged += (s, e) =>
        {
            receivedDockable = e;
            callCount++;
        };

        // Act
        _factory.SetFocusedDockable(null, mockDockable.Object);

        // Assert
        Assert.That(callCount, Is.EqualTo(1));
        Assert.That(receivedDockable, Is.SameAs(mockDockable.Object));
    }

    [Test]
    public void SetFocusedDockable_ShouldNotFireEventForSameDockable()
    {
        // Arrange
        var mockDockable = new Mock<IDockable>();
        int callCount = 0;

        _factory.SetFocusedDockable(null, mockDockable.Object);
        _factory.ActiveDockableChanged += (s, e) => callCount++;

        // Act
        _factory.SetFocusedDockable(null, mockDockable.Object);

        // Assert
        Assert.That(callCount, Is.EqualTo(0));
    }

    [Test]
    public void SetFocusedDockable_ShouldFireEventWhenChangingToNewDockable()
    {
        // Arrange
        var dockable1 = new Mock<IDockable>().Object;
        var dockable2 = new Mock<IDockable>().Object;
        int callCount = 0;

        _factory.SetFocusedDockable(null, dockable1);
        _factory.ActiveDockableChanged += (s, e) => callCount++;

        // Act
        _factory.SetFocusedDockable(null, dockable2);

        // Assert
        Assert.That(callCount, Is.EqualTo(1));
    }
}
