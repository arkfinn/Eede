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
            GridHeight = 24,
            RecentFiles = new()
            {
                new RecentFile { Path = "test.png", LastAccessed = new System.DateTime(2026, 2, 17) }
            }
        };

        var json = JsonSerializer.Serialize(settings);
        var deserialized = JsonSerializer.Deserialize<AppSettings>(json);

        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.GridWidth, Is.EqualTo(16));
        Assert.That(deserialized.GridHeight, Is.EqualTo(24));
        Assert.That(deserialized.RecentFiles.Count, Is.EqualTo(1));
        Assert.That(deserialized.RecentFiles[0].Path, Is.EqualTo("test.png"));
        Assert.That(deserialized.RecentFiles[0].LastAccessed, Is.EqualTo(new System.DateTime(2026, 2, 17)));
    }

    [Test]
    public void AddRecentFile_AddsNewFileToBeginning_WhenFileDoesNotExist()
    {
        var settings = new AppSettings();
        settings.RecentFiles = new System.Collections.Generic.List<RecentFile>
        {
            new RecentFile { Path = "test1.png", LastAccessed = new System.DateTime(2026, 2, 17) }
        };

        var newTime = new System.DateTime(2026, 2, 18);
        settings.AddRecentFile("test2.png", newTime);

        Assert.That(settings.RecentFiles.Count, Is.EqualTo(2));
        Assert.That(settings.RecentFiles[0].Path, Is.EqualTo("test2.png"));
        Assert.That(settings.RecentFiles[0].LastAccessed, Is.EqualTo(newTime));
        Assert.That(settings.RecentFiles[1].Path, Is.EqualTo("test1.png"));
    }

    [Test]
    public void AddRecentFile_MovesExistingFileToBeginning_WhenFileExists()
    {
        var settings = new AppSettings();
        settings.RecentFiles = new System.Collections.Generic.List<RecentFile>
        {
            new RecentFile { Path = "test1.png", LastAccessed = new System.DateTime(2026, 2, 17) },
            new RecentFile { Path = "test2.png", LastAccessed = new System.DateTime(2026, 2, 16) }
        };

        var newTime = new System.DateTime(2026, 2, 18);
        settings.AddRecentFile("test2.png", newTime);

        Assert.That(settings.RecentFiles.Count, Is.EqualTo(2));
        Assert.That(settings.RecentFiles[0].Path, Is.EqualTo("test2.png"));
        Assert.That(settings.RecentFiles[0].LastAccessed, Is.EqualTo(newTime));
        Assert.That(settings.RecentFiles[1].Path, Is.EqualTo("test1.png"));
    }

    [Test]
    public void AddRecentFile_LimitsListTo10Items_WhenMoreThan10Added()
    {
        var settings = new AppSettings();
        settings.RecentFiles = new System.Collections.Generic.List<RecentFile>();

        for (int i = 0; i < 10; i++)
        {
            settings.RecentFiles.Add(new RecentFile { Path = $"test{i}.png", LastAccessed = new System.DateTime(2026, 2, 17) });
        }

        Assert.That(settings.RecentFiles.Count, Is.EqualTo(10));
        var newTime = new System.DateTime(2026, 2, 18);
        settings.AddRecentFile("test10.png", newTime);

        Assert.That(settings.RecentFiles.Count, Is.EqualTo(10));
        Assert.That(settings.RecentFiles[0].Path, Is.EqualTo("test10.png"));
        Assert.That(settings.RecentFiles[0].LastAccessed, Is.EqualTo(newTime));
        Assert.That(settings.RecentFiles[9].Path, Is.EqualTo("test8.png"));
    }
}
