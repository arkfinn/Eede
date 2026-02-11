using Eede.Application.Infrastructure;
using Eede.Domain.Palettes;
using Eede.Infrastructure.Palettes.Persistence;
using Eede.Infrastructure.Palettes.Persistence.ActFileFormat;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Reactive;

namespace Eede.Presentation.ViewModels.DataEntry;

#nullable enable

public class PaletteContainerViewModel : ViewModelBase
{
    [Reactive] public Palette Palette { get; set; }

    public ReactiveCommand<int, Unit> ApplyColorCommand { get; }
    public ReactiveCommand<int, Unit> FetchColorCommand { get; }

    public event Action<ArgbColor>? OnFetchColor;
    public event Func<ArgbColor>? OnApplyColor;

    public ReactiveCommand<IFileStorage, Unit> LoadPaletteCommand { get; }
    public ReactiveCommand<IFileStorage, Unit> SavePaletteCommand { get; }

    public PaletteContainerViewModel()
    {
        Palette = Palette.Create();

        ApplyColorCommand = ReactiveCommand.Create<int>(ExecuteApplyColor);
        FetchColorCommand = ReactiveCommand.Create<int>(ExecuteFetchColor);

        LoadPaletteCommand = ReactiveCommand.Create<IFileStorage>(ExecuteLoadPalette);
        SavePaletteCommand = ReactiveCommand.Create<IFileStorage>(ExecuteSavePalette);
    }

    //パレットフォームのクリックイベントを拾って親に伝達できるようにする
    //保存読み込み処理は自前で処理する
    //このＶＭ自体は親ｖｍに持たせる


    private void ExecuteApplyColor(int number)
    {
        ArgbColor? color = OnApplyColor?.Invoke();
        if (color == null)
        {
            return;
        }
        Palette = Palette.Apply(number, color.Value);
    }

    private void ExecuteFetchColor(int number)
    {
        OnFetchColor?.Invoke(Palette.Fetch(number));
    }

    private async void ExecuteLoadPalette(IFileStorage storage)
    {
        Uri? result = await storage.OpenPaletteFilePickerAsync();
        if (result == null)
        {
            return;
        }

        Palette = new PaletteRepository().Find(result.LocalPath);
    }

    private async void ExecuteSavePalette(IFileStorage storage)
    {
        Uri? result = await storage.SavePaletteFilePickerAsync();
        if (result == null)
        {
            return;
        }

        await using FileStream stream = File.OpenWrite(result.LocalPath);
        new AlphaActFileWriter().Write(stream, Palette);
    }
}
