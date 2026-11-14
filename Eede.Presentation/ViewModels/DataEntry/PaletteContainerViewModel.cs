using Eede.Application.Infrastructure;
using Eede.Domain.Palettes;
using Eede.Presentation.Common.Enums;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

using RxVoid = ReactiveUI.Primitives.RxVoid;

namespace Eede.Presentation.ViewModels.DataEntry;

public partial class PaletteContainerViewModel : ViewModelBase
{
    public ObservableCollection<PaletteTabViewModel> Tabs { get; } = new();
    [Reactive] public partial PaletteTabViewModel? SelectedTab { get; set; }

    public ReactiveCommand<int, RxVoid> ApplyColorCommand { get; }
    public ReactiveCommand<int, RxVoid> FetchColorCommand { get; }
    public ReactiveCommand<(int SourceSlot, int TargetSlot), RxVoid> ReplaceColorCommand { get; }

    public event Action<ArgbColor>? OnFetchColor;
    public event Func<ArgbColor>? OnApplyColor;
    public event Action<ArgbColor, ArgbColor>? OnReplaceColor;

    public bool IsBrowserPlatform => OperatingSystem.IsBrowser();
    public bool IsDesktopPlatform => !OperatingSystem.IsBrowser();

    public ReactiveCommand<IFileStorage?, RxVoid> LoadPaletteCommand { get; }
    public ReactiveCommand<PaletteTabViewModel, RxVoid> SaveTabCommand { get; }
    public ReactiveCommand<IFileStorage?, RxVoid> SavePaletteAsCommand { get; }
    public ReactiveCommand<IFileStorage?, RxVoid> SavePaletteCommand { get; }
    public ReactiveCommand<PaletteTabViewModel, RxVoid> CloseTabCommand { get; }
    public ReactiveCommand<PaletteTabViewModel, RxVoid> CloseOthersCommand { get; }
    public ReactiveCommand<PaletteTabViewModel, RxVoid> SelectTabCommand { get; }

    private readonly IPaletteRepository _paletteRepository;
    private readonly IPaletteSessionRepository _sessionRepository;
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    public PaletteContainerViewModel(IPaletteRepository paletteRepository, IPaletteSessionRepository sessionRepository)
    {
        _paletteRepository = paletteRepository;
        _sessionRepository = sessionRepository;

        // 一時パレットを最初に追加
        Tabs.Add(new PaletteTabViewModel(Palette.Create()));

        _ = Task.Run(async () =>
        {
            var sessionPaths = await _sessionRepository.LoadAsync();
            foreach (var path in sessionPaths)
            {
                try
                {
                    if (System.IO.File.Exists(path))
                    {
                        var palette = _paletteRepository.Find(path);
                        global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            var tab = new PaletteTabViewModel(palette, path);
                            AddOrSelectTab(tab);
                        });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Failed to load session file '{path}': {ex.Message}");
                }
            }
        });

        SelectedTab = Tabs[0];

        ApplyColorCommand = ReactiveCommand.Create<int>(ExecuteApplyColor);
        FetchColorCommand = ReactiveCommand.Create<int>(ExecuteFetchColor);
        ReplaceColorCommand = ReactiveCommand.Create<(int SourceSlot, int TargetSlot)>(ExecuteReplaceColor);

        LoadPaletteCommand = ReactiveCommand.CreateFromTask<IFileStorage?>(ExecuteLoadPalette);
        SaveTabCommand = ReactiveCommand.CreateFromTask<PaletteTabViewModel>(ExecuteSaveTab);
        SavePaletteCommand = ReactiveCommand.CreateFromTask<IFileStorage?>(ExecuteSavePalette);
        SavePaletteAsCommand = ReactiveCommand.CreateFromTask<IFileStorage?>(ExecuteSavePaletteAs);
        CloseTabCommand = ReactiveCommand.CreateFromTask<PaletteTabViewModel>(tab => TryCloseTabAsync(tab));
        CloseOthersCommand = ReactiveCommand.CreateFromTask<PaletteTabViewModel>(ExecuteCloseOthers);
        SelectTabCommand = ReactiveCommand.Create<PaletteTabViewModel>(tab => { SelectedTab = tab; });

        Tabs.CollectionChanged += (s, e) => SaveSession();
    }

    private async void SaveSession()
    {
        var paths = Tabs.Where(t => t.FilePath != null).Select(t => t.FilePath!).ToList();

        await _saveSemaphore.WaitAsync();
        try
        {
            await _sessionRepository.SaveAsync(paths);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine($"Failed to save session: {ex.Message}");
        }
        finally
        {
            _saveSemaphore.Release();
        }
    }

    private void ExecuteApplyColor(int number)
    {
        if (SelectedTab == null) return;

        ArgbColor? color = OnApplyColor?.Invoke();
        if (color == null) return;

        SelectedTab.Palette = SelectedTab.Palette.Apply(number, color.Value);
    }

    private void ExecuteFetchColor(int number)
    {
        if (SelectedTab == null) return;
        OnFetchColor?.Invoke(SelectedTab.Palette.Fetch(number));
    }

    private void ExecuteReplaceColor((int SourceSlot, int TargetSlot) args)
    {
        if (SelectedTab == null) return;
        if (args.SourceSlot < 0 || args.SourceSlot >= Palette.MAX_LENGTH ||
            args.TargetSlot < 0 || args.TargetSlot >= Palette.MAX_LENGTH ||
            args.SourceSlot == args.TargetSlot)
        {
            return;
        }

        ArgbColor sourceColor = SelectedTab.Palette.Fetch(args.SourceSlot);
        ArgbColor targetColor = SelectedTab.Palette.Fetch(args.TargetSlot);
        if (sourceColor.EqualsArgb(targetColor))
        {
            return;
        }

        OnReplaceColor?.Invoke(sourceColor, targetColor);
    }

    private void AddOrSelectTab(PaletteTabViewModel newTab)
    {
        // 1. 同一ファイルパスの既存タブがあればそれにフォーカス
        if (!string.IsNullOrEmpty(newTab.FilePath))
        {
            var samePathTab = Tabs.FirstOrDefault(t => string.Equals(t.FilePath, newTab.FilePath, StringComparison.OrdinalIgnoreCase));
            if (samePathTab != null)
            {
                SelectedTab = samePathTab;
                return;
            }
        }

        // 2. 同一 SourceIdentity の既存タブがあればそれにフォーカス
        if (!string.IsNullOrEmpty(newTab.SourceIdentity))
        {
            var sameIdentityTab = Tabs.FirstOrDefault(t => t.SourceIdentity == newTab.SourceIdentity);
            if (sameIdentityTab != null)
            {
                SelectedTab = sameIdentityTab;
                return;
            }
        }

        // 3. ファイルからのロード時、未編集の初期一時パレットが単独で存在する場合はそれを置き換える
        if (!string.IsNullOrEmpty(newTab.FilePath))
        {
            var unusedTempTab = Tabs.FirstOrDefault(t => IsPlaceholderTemporaryTab(t) && t.Palette.Equals(Palette.Create()));
            if (unusedTempTab != null && Tabs.Count == 1)
            {
                int index = Tabs.IndexOf(unusedTempTab);
                Tabs[index] = newTab;
                SelectedTab = newTab;
                return;
            }
        }

        // 4. 順不同の色集合一致、または既存タブが新しいパレットを完全に内包している場合は既存上位タブに揃える
        if (TrySelectOrUpgradeExistingSupersetTab(newTab))
        {
            return;
        }

        // 5. 新しいパレットが既存の未編集一時インポートタブを下位包含している場合、上位パレットへ統合・置換する
        if (TryReplaceSubordinateTemporaryTabs(newTab))
        {
            return;
        }

        // 6. 新規タブとして追加
        Tabs.Add(newTab);
        SelectedTab = newTab;
    }

    private static bool IsPlaceholderTemporaryTab(PaletteTabViewModel tab)
    {
        return tab.FilePath == null && !tab.IsClosable && !tab.IsDirty;
    }

    private static bool IsUpgradableTemporaryTab(PaletteTabViewModel tab)
    {
        return tab.FilePath == null && tab.IsClosable && !tab.IsDirty;
    }

    private bool TrySelectOrUpgradeExistingSupersetTab(PaletteTabViewModel newTab)
    {
        var supersetTab = Tabs
            .Where(t => !IsPlaceholderTemporaryTab(t) &&
                        (t.Palette.Equals(newTab.Palette) || t.Palette.ContainsAllColorsOf(newTab.Palette)))
            .OrderByDescending(t => t.FilePath != null)
            .ThenByDescending(t => t.Palette.GetEffectiveColorSet().Count)
            .FirstOrDefault();

        if (supersetTab == null)
        {
            return false;
        }

        // 同色セットで既存が未編集一時タブ・新規が保存先付きファイルならファイルタブへ昇格させる
        if (!string.IsNullOrEmpty(newTab.FilePath) &&
            IsUpgradableTemporaryTab(supersetTab) &&
            supersetTab.Palette.HasSameColorSet(newTab.Palette))
        {
            int index = Tabs.IndexOf(supersetTab);
            Tabs[index] = newTab;
            SelectedTab = newTab;
            return true;
        }

        SelectedTab = supersetTab;
        return true;
    }

    private bool TryReplaceSubordinateTemporaryTabs(PaletteTabViewModel newTab)
    {
        var subordinateTabs = Tabs
            .Where(t => IsUpgradableTemporaryTab(t) && newTab.Palette.IsProperSupersetColorSetOf(t.Palette))
            .ToList();

        if (subordinateTabs.Count == 0)
        {
            return false;
        }

        int firstIndex = Tabs.IndexOf(subordinateTabs[0]);
        Tabs[firstIndex] = newTab;
        for (int i = 1; i < subordinateTabs.Count; i++)
        {
            Tabs.Remove(subordinateTabs[i]);
        }

        SelectedTab = newTab;
        return true;
    }

    private async Task ExecuteLoadPalette(IFileStorage? storage)
    {
        if (storage == null) return;
        Uri? result = await storage.OpenPaletteFilePickerAsync();
        if (result == null) return;

        string pathStr = result.IsAbsoluteUri ? (result.IsFile ? result.LocalPath : result.ToString()) : result.OriginalString;

        if (!IsBrowserPlatform && (result.IsAbsoluteUri && result.IsFile || System.IO.File.Exists(pathStr)))
        {
            LoadPalette(pathStr);
            return;
        }

        try
        {
            string extension = System.IO.Path.GetExtension(pathStr);
            await using var stream = await storage.OpenReadStreamAsync(result);
            var palette = _paletteRepository.Find(stream, extension);
            var newTab = new PaletteTabViewModel(palette, pathStr);
            AddOrSelectTab(newTab);
        }
        catch (Exception ex) when (ex is ArgumentException or System.IO.IOException or UnauthorizedAccessException or NotSupportedException or InvalidOperationException or FormatException)
        {
            System.Diagnostics.Trace.WriteLine($"Failed to load palette: {ex.Message}");
        }
    }

    public void LoadPalette(string localPath)
    {
        var palette = _paletteRepository.Find(localPath);
        var newTab = new PaletteTabViewModel(palette, localPath);
        AddOrSelectTab(newTab);
    }

    public void OpenImportedPalette(Palette palette, string title, string? sourceIdentity = null)
    {
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(title);

        var newTab = new PaletteTabViewModel(palette, filePath: null, isClosable: true, title: title, sourceIdentity: sourceIdentity);
        AddOrSelectTab(newTab);
    }

    public void RestoreTabs(IReadOnlyList<PaletteTabViewModel> restoredTabs, int activeTabIndex)
    {
        ArgumentNullException.ThrowIfNull(restoredTabs);

        Tabs.Clear();

        if (restoredTabs.Count == 0)
        {
            var initial = new PaletteTabViewModel(Palette.Create());
            Tabs.Add(initial);
            SelectedTab = initial;
            return;
        }

        PaletteTabViewModel? resolvedActiveTab = null;
        for (int i = 0; i < restoredTabs.Count; i++)
        {
            var tab = restoredTabs[i];
            if (Tabs.Count == 0 || tab.IsDirty)
            {
                Tabs.Add(tab);
                SelectedTab = tab;
            }
            else if (IsPlaceholderTemporaryTab(tab) && Tabs.Any(IsPlaceholderTemporaryTab))
            {
                SelectedTab = Tabs.First(IsPlaceholderTemporaryTab);
            }
            else
            {
                AddOrSelectTab(tab);
            }

            if (i == activeTabIndex)
            {
                resolvedActiveTab = SelectedTab;
            }
        }

        if (Tabs.Count == 0)
        {
            var initial = new PaletteTabViewModel(Palette.Create());
            Tabs.Add(initial);
            SelectedTab = initial;
            return;
        }

        if (resolvedActiveTab != null && Tabs.Contains(resolvedActiveTab))
        {
            SelectedTab = resolvedActiveTab;
        }
        else if (SelectedTab == null || !Tabs.Contains(SelectedTab))
        {
            SelectedTab = Tabs[0];
        }
    }

    private async Task ExecuteSaveTab(PaletteTabViewModel tab)
    {
        if (tab.FilePath == null) return;
        
        _paletteRepository.Save(tab.Palette, tab.FilePath);
        tab.ResetDirty();
    }

    private async Task ExecuteSavePalette(IFileStorage? storage)
    {
        if (SelectedTab == null) return;

        if (string.IsNullOrEmpty(SelectedTab.FilePath) || IsBrowserPlatform)
        {
            await ExecuteSavePaletteAs(storage);
            return;
        }

        _paletteRepository.Save(SelectedTab.Palette, SelectedTab.FilePath);
        SelectedTab.ResetDirty();
    }

    private async Task<bool> SaveTabAsAsync(PaletteTabViewModel tab, IFileStorage? storage)
    {
        if (storage == null) return false;

        Uri? result = await storage.SavePaletteFilePickerAsync();
        if (result == null) return false;

        string pathStr = result.IsAbsoluteUri ? (result.IsFile ? result.LocalPath : result.ToString()) : result.OriginalString;

        if (!IsBrowserPlatform && (result.IsAbsoluteUri && result.IsFile))
        {
            _paletteRepository.Save(tab.Palette, pathStr);
            tab.FilePath = pathStr;
            tab.ResetDirty();
            return true;
        }

        string extension = System.IO.Path.GetExtension(pathStr);
        try
        {
            await using var stream = await storage.OpenWriteStreamAsync(result);
            _paletteRepository.Save(tab.Palette, stream, extension);
            tab.FilePath = pathStr;
            tab.ResetDirty();
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or System.IO.IOException or UnauthorizedAccessException or NotSupportedException or InvalidOperationException)
        {
            System.Diagnostics.Trace.WriteLine($"Failed to save palette: {ex.Message}");
            return false;
        }
    }

    private async Task ExecuteSavePaletteAs(IFileStorage? storage)
    {
        if (SelectedTab == null) return;
        await SaveTabAsAsync(SelectedTab, storage);
    }

    public Interaction<PaletteTabViewModel, SaveAlertResult> ConfirmCloseInteraction { get; } = new();

    private async Task<bool> TryCloseTabAsync(PaletteTabViewModel tab, IFileStorage? storage = null)
    {
        if (!tab.IsClosable) return false;

        if (tab.IsDirty)
        {
            var result = await ConfirmCloseInteraction.Handle(tab).ToTask();
            switch (result)
            {
                case SaveAlertResult.Save:
                    if (tab.FilePath != null && !IsBrowserPlatform)
                    {
                        _paletteRepository.Save(tab.Palette, tab.FilePath);
                        tab.ResetDirty();
                    }
                    else
                    {
                        if (!await SaveTabAsAsync(tab, storage))
                        {
                            return false;
                        }
                    }
                    break;
                case SaveAlertResult.NoSave:
                    break;
                case SaveAlertResult.Cancel:
                    return false;
            }
        }

        Tabs.Remove(tab);
        if (SelectedTab == null && Tabs.Count > 0)
        {
            SelectedTab = Tabs[0];
        }
        if (Tabs.Count == 0)
        {
            Tabs.Add(new PaletteTabViewModel(Palette.Create()));
            SelectedTab = Tabs[0];
        }
        return true;
    }

    private async Task ExecuteCloseOthers(PaletteTabViewModel tab)
    {
        var others = Tabs.Where(t => t != tab).ToList();
        foreach (var other in others)
        {
            if (Tabs.Count <= 1) break;
            bool closed = await TryCloseTabAsync(other);
            if (!closed) break;
        }
    }

    public async Task<bool> TryCloseAllAsync(IFileStorage? storage = null)
    {
        var closableTabs = Tabs.Where(t => t.IsClosable).ToList();
        foreach (var tab in closableTabs)
        {
            if (!await TryCloseTabAsync(tab, storage))
            {
                return false;
            }
        }
        return true;
    }
}
