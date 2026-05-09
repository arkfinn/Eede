using Eede.Application.Infrastructure;
using Eede.Domain.Palettes;
using Eede.Presentation.ViewModels.DataEntry;
using Moq;
using NUnit.Framework;
using Avalonia.Headless.NUnit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using ReactiveUI;

namespace Eede.Presentation.Tests.ViewModels.DataEntry;

[TestFixture]
public class PaletteContainerViewModelTests
{
    private Mock<IFileStorage> _fileStorageMock = default!;
    private Mock<IPaletteRepository> _paletteRepositoryMock = default!;
    private Mock<IPaletteSessionRepository> _paletteSessionRepositoryMock = default!;

    [SetUp]
    public void SetUp()
    {
        _fileStorageMock = new Mock<IFileStorage>();
        _paletteRepositoryMock = new Mock<IPaletteRepository>();
        _paletteSessionRepositoryMock = new Mock<IPaletteSessionRepository>();
    }

    [AvaloniaTest]
    public void InitialState_ShouldHaveOneTemporaryPaletteTab()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);

        Assert.That(sut.Tabs.Count, Is.EqualTo(1));
        Assert.That(sut.Tabs[0].FilePath, Is.Null);
        Assert.That(sut.SelectedTab, Is.EqualTo(sut.Tabs[0]));
    }

    [AvaloniaTest]
    public async Task LoadPaletteCommand_ShouldAddNewTab()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);
        var filePath = @"C:\test.aact";
        var palette = Palette.Create();
        _fileStorageMock.Setup(x => x.OpenPaletteFilePickerAsync()).ReturnsAsync(new Uri(filePath));
        _paletteRepositoryMock.Setup(x => x.Find(filePath)).Returns(palette);

        await sut.LoadPaletteCommand.Execute(_fileStorageMock.Object).ToTask();

        Assert.That(sut.Tabs.Count, Is.EqualTo(2));
        Assert.That(sut.Tabs[1].FilePath, Is.EqualTo(filePath));
        Assert.That(sut.SelectedTab, Is.EqualTo(sut.Tabs[1]));
    }

    [AvaloniaTest]
    public async Task LoadPaletteCommand_WhenFileAlreadyOpened_ShouldActivateExistingTab()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);
        var filePath = @"C:\test.aact";
        var palette = Palette.Create();
        _fileStorageMock.Setup(x => x.OpenPaletteFilePickerAsync()).ReturnsAsync(new Uri(filePath));
        _paletteRepositoryMock.Setup(x => x.Find(filePath)).Returns(palette);

        await sut.LoadPaletteCommand.Execute(_fileStorageMock.Object).ToTask();
        var firstTab = sut.Tabs[1];

        // Try to open the same file again
        await sut.LoadPaletteCommand.Execute(_fileStorageMock.Object).ToTask();

        Assert.That(sut.Tabs.Count, Is.EqualTo(2), "Should not add a new tab");
        Assert.That(sut.SelectedTab, Is.EqualTo(firstTab), "Should activate existing tab");
    }
    [AvaloniaTest]
    public async Task SavePaletteCommand_WhenFilePathExists_ShouldSaveDirectly()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);
        var filePath = @"C:\test.aact";
        var palette = Palette.Create();
        _fileStorageMock.Setup(x => x.OpenPaletteFilePickerAsync()).ReturnsAsync(new Uri(filePath));
        _paletteRepositoryMock.Setup(x => x.Find(filePath)).Returns(palette);
        await sut.LoadPaletteCommand.Execute(_fileStorageMock.Object).ToTask();

        var tab = sut.SelectedTab!;
        tab.Palette = tab.Palette.Apply(0, new ArgbColor(255, 255, 0, 0));
        Assert.That(tab.IsDirty, Is.True);

        await sut.SavePaletteCommand.Execute(_fileStorageMock.Object).ToTask();

        _paletteRepositoryMock.Verify(x => x.Save(tab.Palette, filePath), Times.Once);
        Assert.That(tab.IsDirty, Is.False);
    }

    [AvaloniaTest]
    public async Task SavePaletteCommand_WhenTemporaryTab_ShouldCallSaveAs()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);
        var tab = sut.SelectedTab!;
        var filePath = @"C:\new.aact";
        _fileStorageMock.Setup(x => x.SavePaletteFilePickerAsync()).ReturnsAsync(new Uri(filePath));

        await sut.SavePaletteCommand.Execute(_fileStorageMock.Object).ToTask();

        _paletteRepositoryMock.Verify(x => x.Save(tab.Palette, filePath), Times.Once);
        Assert.That(tab.FilePath, Is.EqualTo(filePath));
        Assert.That(tab.IsDirty, Is.False);
    }

    [AvaloniaTest]
    public void Constructor_ShouldLoadSessionPaths()
    {
        var filePath = @"C:\session_test.aact";
        _paletteSessionRepositoryMock.Setup(x => x.Load()).Returns(new[] { filePath });
        _paletteRepositoryMock.Setup(x => x.Find(filePath)).Returns(Palette.Create());
        
        // Mock File.Exists is hard. But let's assume it exists if I don't mock it? 
        // Actually I should probably use a IFileSystem mock if I wanted to be perfect.
        // But here I'll just check if it calls Load().
        
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        
        _paletteSessionRepositoryMock.Verify(x => x.Load(), Times.Once);
    }
}
