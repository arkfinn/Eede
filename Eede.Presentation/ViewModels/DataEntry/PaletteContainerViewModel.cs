using Eede.Application.Infrastructure;
using Eede.Domain.Palettes;
using Eede.Presentation.Common.Enums;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
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

    public event Action<ArgbColor>? OnFetchColor;
    public event Func<ArgbColor>? OnApplyColor;

    public ReactiveCommand<IFileStorage?, RxVoid> LoadPaletteCommand { get; }
    public ReactiveCommand<PaletteTabViewModel, RxVoid> SaveTabCommand { get; }
    public ReactiveCommand<IFileStorage?, RxVoid> SavePaletteAsCommand { get; }
    public ReactiveCommand<IFileStorage?, RxVoid> SavePaletteCommand { get; }
    public ReactiveCommand<PaletteTabViewModel, RxVoid> CloseTabCommand { get; }
    public ReactiveCommand<PaletteTabViewModel, RxVoid> CloseOthersCommand { get; }

    private readonly IPaletteRepository _paletteRepository;
    private readonly IPaletteSessionRepository _sessionRepository;
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    public PaletteContainerViewModel(IPaletteRepository paletteRepository, IPaletteSessionRepository sessionRepository)
    {
        _paletteRepository = paletteRepository;
        _sessionRepository = sessionRepository;

        // 一時パレットを最初に追加
        Tabs.Add(new PaletteTabViewModel(Palette.Create()));

        var sessionPaths = _sessionRepository.Load();
        _ = Task.Run(() =>
        {
            foreach (var path in sessionPaths)
            {
                try
                {
                    if (System.IO.File.Exists(path))
                    {
                        var palette = _paletteRepository.Find(path);
                        global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            Tabs.Add(new PaletteTabViewModel(palette, path));
                        });
                    }
                }
                catch { /* Ignore invalid session files */ }
            }
        });

        SelectedTab = Tabs[0];

        ApplyColorCommand = ReactiveCommand.Create<int>(ExecuteApplyColor);
        FetchColorCommand = ReactiveCommand.Create<int>(ExecuteFetchColor);

        LoadPaletteCommand = ReactiveCommand.CreateFromTask<IFileStorage?>(ExecuteLoadPalette);
        SaveTabCommand = ReactiveCommand.CreateFromTask<PaletteTabViewModel>(ExecuteSaveTab);
        SavePaletteCommand = ReactiveCommand.CreateFromTask<IFileStorage?>(ExecuteSavePalette);
        SavePaletteAsCommand = ReactiveCommand.CreateFromTask<IFileStorage?>(ExecuteSavePaletteAs);
        CloseTabCommand = ReactiveCommand.CreateFromTask<PaletteTabViewModel>(TryCloseTabAsync);
        CloseOthersCommand = ReactiveCommand.CreateFromTask<PaletteTabViewModel>(ExecuteCloseOthers);

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

    private async Task ExecuteLoadPalette(IFileStorage? storage)
    {
        if (storage == null) return;
        Uri? result = await storage.OpenPaletteFilePickerAsync();
        if (result == null) return;

        LoadPalette(result.LocalPath);
    }

    public void LoadPalette(string localPath)
    {
        var existingTab = Tabs.FirstOrDefault(t => t.FilePath == localPath);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var palette = _paletteRepository.Find(localPath);
        var newTab = new PaletteTabViewModel(palette, localPath);
        Tabs.Add(newTab);
        SelectedTab = newTab;
    }

    private async Task ExecuteSaveTab(PaletteTabViewModel tab)
    {
        if (tab.FilePath == null) return; // 一時パレットはコンテキストメニューからは保存させない（または何も言わずに無視）
        
        _paletteRepository.Save(tab.Palette, tab.FilePath);
        tab.ResetDirty();
    }

    private async Task ExecuteSavePalette(IFileStorage? storage)
    {
        if (SelectedTab == null) return;

        if (string.IsNullOrEmpty(SelectedTab.FilePath))
        {
            await ExecuteSavePaletteAs(storage);
            return;
        }

        _paletteRepository.Save(SelectedTab.Palette, SelectedTab.FilePath);
        SelectedTab.ResetDirty();
    }

    private async Task ExecuteSavePaletteAs(IFileStorage? storage)
    {
        if (SelectedTab == null || storage == null) return;

        Uri? result = await storage.SavePaletteFilePickerAsync();
        if (result == null) return;

        string localPath = result.LocalPath;
        _paletteRepository.Save(SelectedTab.Palette, localPath);
        SelectedTab.FilePath = localPath;
        SelectedTab.ResetDirty();
    }

    public Interaction<PaletteTabViewModel, SaveAlertResult> ConfirmCloseInteraction { get; } = new();

    private async Task<bool> TryCloseTabAsync(PaletteTabViewModel tab)
    {
        if (!tab.IsClosable) return false;

        if (tab.IsDirty && tab.FilePath != null)
        {
            var result = await ConfirmCloseInteraction.Handle(tab).ToTask();
            switch (result)
            {
                case SaveAlertResult.Save:
                    _paletteRepository.Save(tab.Palette, tab.FilePath);
                    tab.ResetDirty();
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

    public async Task<bool> TryCloseAllAsync()
    {
        var closableTabs = Tabs.Where(t => t.IsClosable).ToList();
        foreach (var tab in closableTabs)
        {
            if (!await TryCloseTabAsync(tab))
            {
                return false;
            }
        }
        return true;
    }
}
