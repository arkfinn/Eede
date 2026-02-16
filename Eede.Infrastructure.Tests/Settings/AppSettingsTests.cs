using NUnit.Framework;
using System.Text.Json;
using Eede.Application.Settings;

namespace Eede.Infrastructure.Tests.Settings;

[TestFixture]
public class AppSettingsTests
{
    [Test]
    public void SerializeAndDeserializeTest()
    {
        var settings = new AppSettings
        {
            GridWidth = 16,
            GridHeight = 24
        };

        var json = JsonSerializer.Serialize(settings);
        var deserialized = JsonSerializer.Deserialize<AppSettings>(json);

        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.GridWidth, Is.EqualTo(16));
        Assert.That(deserialized.GridHeight, Is.EqualTo(24));
    }
}
