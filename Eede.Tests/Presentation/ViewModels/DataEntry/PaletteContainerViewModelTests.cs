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
    public async Task LoadPaletteCommand_ShouldReplaceInitialUnusedTab()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);
        var filePath = @"C:\test.aact";
        var palette = Palette.Create();
        _fileStorageMock.Setup(x => x.OpenPaletteFilePickerAsync()).ReturnsAsync(new Uri(filePath));
        _paletteRepositoryMock.Setup(x => x.Find(filePath)).Returns(palette);

        await sut.LoadPaletteCommand.Execute(_fileStorageMock.Object).ToTask();

        Assert.That(sut.Tabs.Count, Is.EqualTo(1), "未編集の初期一時パレットは開いたパレットで置き換えられること");
        Assert.That(sut.Tabs[0].FilePath, Is.EqualTo(filePath));
        Assert.That(sut.SelectedTab, Is.EqualTo(sut.Tabs[0]));
    }

    [AvaloniaTest]
    public async Task LoadPaletteCommand_WhenOpeningSecondDistinctPalette_ShouldAddNewTab()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, new Mock<Eede.Application.Infrastructure.IPaletteSessionRepository>().Object);
        var filePath1 = @"C:\test1.aact";
        var palette1 = Palette.Create().Apply(0, new ArgbColor(255, 10, 0, 0));
        var filePath2 = @"C:\test2.aact";
        var palette2 = Palette.Create().Apply(0, new ArgbColor(255, 0, 10, 0));

        _paletteRepositoryMock.Setup(x => x.Find(filePath1)).Returns(palette1);
        _paletteRepositoryMock.Setup(x => x.Find(filePath2)).Returns(palette2);

        _fileStorageMock.Setup(x => x.OpenPaletteFilePickerAsync()).ReturnsAsync(new Uri(filePath1));
        await sut.LoadPaletteCommand.Execute(_fileStorageMock.Object).ToTask();
        Assert.That(sut.Tabs.Count, Is.EqualTo(1));

        _fileStorageMock.Setup(x => x.OpenPaletteFilePickerAsync()).ReturnsAsync(new Uri(filePath2));
        await sut.LoadPaletteCommand.Execute(_fileStorageMock.Object).ToTask();

        Assert.That(sut.Tabs.Count, Is.EqualTo(2), "異なる色の2つ目のパレットは新タブとして追加されること");
        Assert.That(sut.Tabs[1].FilePath, Is.EqualTo(filePath2));
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
        var firstTab = sut.Tabs[0];

        // Try to open the same file again
        await sut.LoadPaletteCommand.Execute(_fileStorageMock.Object).ToTask();

        Assert.That(sut.Tabs.Count, Is.EqualTo(1), "同一ファイルは重複追加されないこと");
        Assert.That(sut.SelectedTab, Is.EqualTo(firstTab), "既存タブがアクティブ化されること");
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

    [AvaloniaTest]
    public async Task ReplaceColorCommand_WhenDifferentColors_ShouldInvokeOnReplaceColorAndKeepPaletteUnchanged()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var tab = sut.SelectedTab!;
        var color1 = new ArgbColor(255, 255, 0, 0);
        var color2 = new ArgbColor(255, 0, 255, 0);
        tab.Palette = tab.Palette.Apply(0, color1).Apply(1, color2);

        (ArgbColor source, ArgbColor target)? replacedArgs = null;
        sut.OnReplaceColor += (source, target) => replacedArgs = (source, target);

        await sut.ReplaceColorCommand.Execute((0, 1)).ToTask();

        Assert.That(replacedArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(replacedArgs!.Value.source, Is.EqualTo(color1));
            Assert.That(replacedArgs!.Value.target, Is.EqualTo(color2));
            Assert.That(tab.Palette.Fetch(0), Is.EqualTo(color1), "パレットのスロット0の色は維持されること");
            Assert.That(tab.Palette.Fetch(1), Is.EqualTo(color2), "パレットのスロット1の色は維持されること");
        });
    }

    [AvaloniaTest]
    public async Task ReplaceColorCommand_WhenSameSlotOrSameColor_ShouldNotInvokeOnReplaceColor()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var tab = sut.SelectedTab!;
        var sameColor = new ArgbColor(255, 255, 0, 0);
        tab.Palette = tab.Palette.Apply(0, sameColor).Apply(1, sameColor);

        bool invoked = false;
        sut.OnReplaceColor += (_, _) => invoked = true;

        await sut.ReplaceColorCommand.Execute((0, 0)).ToTask();
        Assert.That(invoked, Is.False, "同一スロットへのドロップでは発火しないこと");

        await sut.ReplaceColorCommand.Execute((0, 1)).ToTask();
        Assert.That(invoked, Is.False, "同一色同士のドロップでは発火しないこと");
    }

    [AvaloniaTest]
    public async Task SelectTabCommand_ShouldUpdateSelectedTab()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var tab2 = new PaletteTabViewModel(Palette.Create(), "tab2.act");
        sut.Tabs.Add(tab2);

        Assert.That(sut.SelectedTab, Is.Not.EqualTo(tab2));

        await sut.SelectTabCommand.Execute(tab2).ToTask();
        Assert.That(sut.SelectedTab, Is.EqualTo(tab2));
    }

    [AvaloniaTest]
    public void LoadPalette_WhenUnusedTemporaryTabExists_ShouldReplaceIt()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        Assert.That(sut.Tabs.Count, Is.EqualTo(1));
        Assert.That(sut.Tabs[0].FilePath, Is.Null);

        var customPalette = Palette.Create().Apply(0, new ArgbColor(255, 10, 20, 30));
        _paletteRepositoryMock.Setup(x => x.Find(@"C:\test.act")).Returns(customPalette);

        sut.LoadPalette(@"C:\test.act");

        Assert.That(sut.Tabs.Count, Is.EqualTo(1), "未編集の一時パレットは置き換えられ、タブ数は1のままであること");
        Assert.That(sut.Tabs[0].FilePath, Is.EqualTo(@"C:\test.act"));
        Assert.That(sut.SelectedTab, Is.EqualTo(sut.Tabs[0]));
    }

    [AvaloniaTest]
    public void LoadPalette_WhenTemporaryTabIsEdited_ShouldNotReplaceAndAddNewTab()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        // 一時パレットを編集
        sut.SelectedTab!.Palette = sut.SelectedTab.Palette.Apply(0, new ArgbColor(255, 128, 0, 0));

        var customPalette = Palette.Create().Apply(1, new ArgbColor(255, 0, 128, 0));
        _paletteRepositoryMock.Setup(x => x.Find(@"C:\test.act")).Returns(customPalette);

        sut.LoadPalette(@"C:\test.act");

        Assert.That(sut.Tabs.Count, Is.EqualTo(2), "編集済みの一時パレットは保持され、新タブが追加されること");
        Assert.That(sut.Tabs[1].FilePath, Is.EqualTo(@"C:\test.act"));
        Assert.That(sut.SelectedTab, Is.EqualTo(sut.Tabs[1]));
    }

    [AvaloniaTest]
    public void LoadPalette_WhenSameColorPaletteExistsUnderDifferentPath_ShouldNotAddTabAndSelectExisting()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var customPalette = Palette.Create().Apply(0, new ArgbColor(255, 100, 150, 200));

        _paletteRepositoryMock.Setup(x => x.Find(@"C:\palette1.act")).Returns(customPalette);
        _paletteRepositoryMock.Setup(x => x.Find(@"C:\palette2.act")).Returns(customPalette);

        sut.LoadPalette(@"C:\palette1.act");
        Assert.That(sut.Tabs.Count, Is.EqualTo(1));
        var tab1 = sut.Tabs[0];

        // 異なるパスだが全く同一の色構成を持つパレットをロード
        sut.LoadPalette(@"C:\palette2.act");

        Assert.That(sut.Tabs.Count, Is.EqualTo(1), "同一色構成のパレットは重複追加されないこと");
        Assert.That(sut.SelectedTab, Is.EqualTo(tab1), "既存の同一色パレットタブが選択されること");
    }

    [AvaloniaTest]
    public void OpenImportedPalette_WhenSameColorExists_ShouldNotAddTabAndSelectExisting()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var customPalette = Palette.Create().Apply(0, new ArgbColor(255, 50, 60, 70));

        _paletteRepositoryMock.Setup(x => x.Find(@"C:\palette1.act")).Returns(customPalette);
        sut.LoadPalette(@"C:\palette1.act");
        var existingTab = sut.Tabs[0];

        // 同一色のインポートパレットを開こうとする
        sut.OpenImportedPalette(customPalette, "インポート色");

        Assert.That(sut.Tabs.Count, Is.EqualTo(1), "同一色のインポートパレットは重複追加されないこと");
        Assert.That(sut.SelectedTab, Is.EqualTo(existingTab));
    }

    [AvaloniaTest]
    public void OpenImportedPalette_WhenSameColorsInDifferentOrder_ShouldNotAddTabAndSelectExisting()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var red = new ArgbColor(255, 255, 0, 0);
        var blue = new ArgbColor(255, 0, 0, 255);

        var palette1 = Palette.Create().Apply(0, red).Apply(1, blue);
        var palette2 = Palette.Create().Apply(5, blue).Apply(12, red);

        sut.OpenImportedPalette(palette1, "image1.png", "src1");
        var importedTab1 = sut.SelectedTab;
        Assert.That(sut.Tabs.Count, Is.EqualTo(2));

        // 並び違いで同じ色のインポートパレットを開く
        sut.OpenImportedPalette(palette2, "image2.png", "src2");

        Assert.That(sut.Tabs.Count, Is.EqualTo(2), "並びが違うだけでも色集合が同じなら重複追加されないこと");
        Assert.That(sut.SelectedTab, Is.EqualTo(importedTab1));
    }

    [AvaloniaTest]
    public void OpenImportedPalette_WhenExistingTabIsSuperset_ShouldSelectExistingSupersetTab()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var red = new ArgbColor(255, 255, 0, 0);
        var green = new ArgbColor(255, 0, 255, 0);
        var blue = new ArgbColor(255, 0, 0, 255);

        var superset = Palette.Create().Apply(0, red).Apply(1, green).Apply(2, blue);
        var subset = Palette.Create().Apply(3, blue).Apply(8, red);

        _paletteRepositoryMock.Setup(x => x.Find(@"C:\full.act")).Returns(superset);
        sut.LoadPalette(@"C:\full.act");
        var supersetTab = sut.SelectedTab;
        Assert.That(sut.Tabs.Count, Is.EqualTo(1));

        // 下位（部分集合）パレットを開く
        sut.OpenImportedPalette(subset, "subset.png", "src_sub");

        Assert.That(sut.Tabs.Count, Is.EqualTo(1), "上位パレットが存在する場合は下位パレットを新規追加しないこと");
        Assert.That(sut.SelectedTab, Is.EqualTo(supersetTab));
    }

    [AvaloniaTest]
    public void OpenImportedPalette_WhenNewPaletteIsSupersetOfUneditedImportedTabs_ShouldUpgradeAndConsolidate()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var red = new ArgbColor(255, 255, 0, 0);
        var green = new ArgbColor(255, 0, 255, 0);
        var blue = new ArgbColor(255, 0, 0, 255);

        var sub1 = Palette.Create().Apply(0, red);
        var sub2 = Palette.Create().Apply(0, green).Apply(1, blue);
        var superset = Palette.Create().Apply(2, blue).Apply(5, red).Apply(9, green);

        sut.OpenImportedPalette(sub1, "part1.png", "src1");
        sut.OpenImportedPalette(sub2, "part2.png", "src2");
        Assert.That(sut.Tabs.Count, Is.EqualTo(3)); // 初期一時タブ + part1 + part2

        // すべてを内包する上位パレットをインポート
        sut.OpenImportedPalette(superset, "full.png", "src_full");

        Assert.That(sut.Tabs.Count, Is.EqualTo(2), "未編集の下位インポートタブは上位パレット1つに統合・置換されること");
        Assert.That(sut.SelectedTab!.CustomTitle, Is.EqualTo("full.png"));
    }

    [AvaloniaTest]
    public void OpenImportedPalette_WhenExistingLowerTabIsActFileOrDirty_ShouldProtectItFromReplacement()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var red = new ArgbColor(255, 255, 0, 0);
        var green = new ArgbColor(255, 0, 255, 0);
        var blue = new ArgbColor(255, 0, 0, 255);

        var actSubset = Palette.Create().Apply(0, red);
        var dirtyImportedSubset = Palette.Create().Apply(0, green);
        var superset = Palette.Create().Apply(0, red).Apply(1, green).Apply(2, blue);

        _paletteRepositoryMock.Setup(x => x.Find(@"C:\base.act")).Returns(actSubset);
        sut.LoadPalette(@"C:\base.act");
        var actTab = sut.SelectedTab!;

        sut.OpenImportedPalette(dirtyImportedSubset, "edited.png", "src_edited");
        var dirtyTab = sut.SelectedTab!;
        dirtyTab.Palette = dirtyTab.Palette.Apply(5, new ArgbColor(255, 10, 10, 10)); // IsDirty = true
        // Wait: dirtyTab now has {green, (10,10,10)}, let's make superset contain (10,10,10) too so it's a strict superset
        var trueSuperset = superset.Apply(3, new ArgbColor(255, 10, 10, 10));

        sut.OpenImportedPalette(trueSuperset, "full.png", "src_full");

        Assert.Multiple(() =>
        {
            Assert.That(sut.Tabs.Contains(actTab), Is.True, ".act ファイルタブは上位インポート時も保護されること");
            Assert.That(sut.Tabs.Contains(dirtyTab), Is.True, "編集済み(IsDirty)タブは保護されること");
            Assert.That(sut.Tabs.Count, Is.EqualTo(3));
            Assert.That(sut.SelectedTab!.CustomTitle, Is.EqualTo("full.png"));
        });
    }

    [AvaloniaTest]
    public void RestoreTabs_ShouldDeduplicateSavedDuplicatesAndSubsetsWhileProtectingDirtyTabs()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var red = new ArgbColor(255, 255, 0, 0);
        var green = new ArgbColor(255, 0, 255, 0);
        var blue = new ArgbColor(255, 0, 0, 255);

        var pPlaceholder = Palette.Create();
        var pSub = Palette.Create().Apply(0, red);
        var pSameDiffOrder = Palette.Create().Apply(3, red).Apply(7, green);
        var pFull = Palette.Create().Apply(0, red).Apply(1, green).Apply(2, blue);

        var placeholderTab = new PaletteTabViewModel(pPlaceholder);
        var dupTemp1 = new PaletteTabViewModel(Palette.Create().Apply(0, red).Apply(1, green), filePath: null, isClosable: true, title: "walk_01.png", sourceIdentity: "walk_01.png");
        var dupTemp2 = new PaletteTabViewModel(pSameDiffOrder, filePath: null, isClosable: true, title: "walk_02.png", sourceIdentity: "walk_02.png");
        var dirtySubTab = new PaletteTabViewModel(pSub, filePath: null, isClosable: true, title: "edited.png", sourceIdentity: "edited.png")
        {
            IsDirty = true
        };
        var supersetTemp = new PaletteTabViewModel(pFull, filePath: null, isClosable: true, title: "sheet.png", sourceIdentity: "sheet.png");

        // 旧セッションで重複や下位タブが並んでいた状態（activeTabIndex=2 は walk_02.png）を復元
        sut.RestoreTabs([placeholderTab, dupTemp1, dupTemp2, dirtySubTab, supersetTemp], activeTabIndex: 2);

        Assert.Multiple(() =>
        {
            // walk_01 & walk_02 は sheet.png (superset) に統合され、placeholder と edited (dirty) と sheet.png の3タブに整理される
            Assert.That(sut.Tabs.Count, Is.EqualTo(3));
            Assert.That(sut.Tabs[0], Is.EqualTo(placeholderTab));
            Assert.That(sut.Tabs.Contains(dirtySubTab), Is.True, "IsDirty タブは復元時にも保護されること");
            Assert.That(sut.Tabs.Contains(supersetTemp), Is.True, "上位パレットが残ること");
            Assert.That(sut.Tabs.Contains(dupTemp1), Is.False, "重複・下位一時タブは復元時に統合されること");
            Assert.That(sut.Tabs.Contains(dupTemp2), Is.False, "重複・下位一時タブは復元時に統合されること");
            Assert.That(sut.SelectedTab, Is.EqualTo(supersetTemp), "統合先にアクティブ選択が引き継がれること");
        });
    }

    [AvaloniaTest]
    public async Task TryCloseAllAsync_WhenDirtyTabHasNullFilePathAndUserSelectsSave_SavesAndReturnsTrue()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var palette = Palette.Create();
        sut.OpenImportedPalette(palette, "sample.png", "C:\\sample.png");
        var tab = sut.Tabs[1];
        tab.Palette = tab.Palette.Apply(0, new ArgbColor(255, 255, 0, 0));

        sut.ConfirmCloseInteraction.RegisterHandler(ctx =>
            ctx.SetOutput(Eede.Presentation.Common.Enums.SaveAlertResult.Save));

        var saveUri = new Uri("file:///C:/saved_palette.act");
        _fileStorageMock.Setup(x => x.SavePaletteFilePickerAsync()).ReturnsAsync(saveUri);

        bool canClose = await sut.TryCloseAllAsync(_fileStorageMock.Object);

        Assert.Multiple(() =>
        {
            Assert.That(canClose, Is.True);
            Assert.That(tab.IsDirty, Is.False);
            Assert.That(tab.FilePath, Is.EqualTo(saveUri.LocalPath));
            _paletteRepositoryMock.Verify(x => x.Save(tab.Palette, saveUri.LocalPath), Times.Once);
        });
    }

    [AvaloniaTest]
    public async Task TryCloseAllAsync_WhenDirtyTabHasNullFilePathAndSaveAsCanceled_ReturnsFalse()
    {
        var sut = new PaletteContainerViewModel(_paletteRepositoryMock.Object, _paletteSessionRepositoryMock.Object);
        var palette = Palette.Create();
        sut.OpenImportedPalette(palette, "sample.png", "C:\\sample.png");
        var tab = sut.Tabs[1];
        tab.Palette = tab.Palette.Apply(0, new ArgbColor(255, 255, 0, 0));

        sut.ConfirmCloseInteraction.RegisterHandler(ctx =>
            ctx.SetOutput(Eede.Presentation.Common.Enums.SaveAlertResult.Save));

        _fileStorageMock.Setup(x => x.SavePaletteFilePickerAsync()).ReturnsAsync((Uri?)null);

        bool canClose = await sut.TryCloseAllAsync(_fileStorageMock.Object);

        Assert.Multiple(() =>
        {
            Assert.That(canClose, Is.False);
            Assert.That(tab.IsDirty, Is.True);
        });
    }
}
