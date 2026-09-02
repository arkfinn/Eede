using System;
using Eede.Presentation;
using Eede.Presentation.Common.MarkupExtensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Eede.Tests.Presentation.Common.MarkupExtensions;

[TestFixture]
public class ResolveExtensionTests
{
    private IServiceProvider? _originalServices;

    [SetUp]
    public void SetUp()
    {
        _originalServices = App.Services;
    }

    [TearDown]
    public void TearDown()
    {
        App.Services = _originalServices;
    }

    [Test]
    public void Constructor_SetsTypeProperty()
    {
        var ext = new ResolveExtension(typeof(string));
        Assert.That(ext.Type, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void ProvideValue_WhenAppServicesIsNull_ThrowsInvalidOperationException()
    {
        App.Services = null;
        var ext = new ResolveExtension(typeof(string));

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ext.ProvideValue(Mock.Of<IServiceProvider>());
        });

        Assert.That(ex!.Message, Does.Contain("App.Services is not initialized"));
    }

    [Test]
    public void ProvideValue_WhenAppServicesIsInitialized_ReturnsResolvedInstance()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton("Hello World");
        App.Services = serviceCollection.BuildServiceProvider();

        var ext = new ResolveExtension(typeof(string));
        var result = ext.ProvideValue(Mock.Of<IServiceProvider>());

        Assert.That(result, Is.EqualTo("Hello World"));
    }
}
