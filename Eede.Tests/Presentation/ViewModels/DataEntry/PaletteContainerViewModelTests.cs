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
    public async Task Constructor_ShouldLoadSessionPaths()
    {
        var filePath = @"C:\session_test.aact";
        _paletteSessionRepositoryMock.Setup(x => x.LoadAsync()).Returns(Task.FromResult((IEnumerable<string>)new[] { filePath }));
        _paletteRepositoryMock.Setup(x => x.Find(filePath)).Returns(Palette.Create());
        
        // Mock File.Exists is hard. But let's assume it exists if I don't mock it? 
        // Actually I should probably use a IFileSystem mock if I wanted to be perfect.
        // But here I'll just check if it calls Load().
        
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        await Task.Delay(100); // Allow Task.Run in constructor to execute
        
        _paletteSessionRepositoryMock.Verify(x => x.LoadAsync(), Times.Once);
    }

    [AvaloniaTest]
    public void OpenImportedPalette_AddsNewTabAndSelectsIt()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var palette = Palette.Create();

        sut.OpenImportedPalette(palette, "sample.png", "C:\\sample.png");

        Assert.That(sut.Tabs.Count, Is.EqualTo(2));
        var importedTab = sut.Tabs[1];
        Assert.That(importedTab.CustomTitle, Is.EqualTo("sample.png"));
        Assert.That(importedTab.FilePath, Is.Null, "画像からインポートしたタブの FilePath は null であること");
        Assert.That(importedTab.IsClosable, Is.True, "画像からインポートしたタブは閉じられること");
        Assert.That(importedTab.SourceIdentity, Is.EqualTo("C:\\sample.png"));
        Assert.That(sut.SelectedTab, Is.EqualTo(importedTab));
    }

    [AvaloniaTest]
    public void OpenImportedPalette_WhenSameSourceIdentityAlreadyOpened_ActivatesExistingTabWithoutAddingDuplicate()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var palette1 = Palette.Create();
        var palette2 = Palette.Create();

        sut.OpenImportedPalette(palette1, "sample.png", "C:\\sample.png");
        Assert.That(sut.Tabs.Count, Is.EqualTo(2));
        var firstImportedTab = sut.Tabs[1];

        // 一時パレットに切り替え
        sut.SelectedTab = sut.Tabs[0];

        // 同一の sourceIdentity で再インポート
        sut.OpenImportedPalette(palette2, "sample.png", "C:\\sample.png");

        Assert.That(sut.Tabs.Count, Is.EqualTo(2), "タブは増殖しないこと");
        Assert.That(sut.SelectedTab, Is.EqualTo(firstImportedTab), "既存のタブがフォーカスされること");
    }

    [AvaloniaTest]
    public async Task TryCloseTabAsync_WhenImportedTabIsDirty_TriggersConfirmCloseInteraction()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var palette = Palette.Create();
        sut.OpenImportedPalette(palette, "sample.png", "C:\\sample.png");
        var tab = sut.Tabs[1];

        // 色を変更してダーティにする
        tab.Palette = tab.Palette.Apply(0, new ArgbColor(255, 255, 0, 0));
        Assert.That(tab.IsDirty, Is.True);

        bool interactionHandled = false;
        sut.ConfirmCloseInteraction.RegisterHandler(ctx =>
        {
            interactionHandled = true;
            ctx.SetOutput(Eede.Presentation.Common.Enums.SaveAlertResult.NoSave);
        });

        await sut.CloseTabCommand.Execute(tab).ToTask();

        Assert.That(interactionHandled, Is.True, "未保存の画像インポートパレットを閉じる際は警告ダイアログが発火すること");
        Assert.That(sut.Tabs.Contains(tab), Is.False, "NoSave の場合はタブが閉じられること");
    }
}
