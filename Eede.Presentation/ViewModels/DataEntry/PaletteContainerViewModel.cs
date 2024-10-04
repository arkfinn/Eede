using Avalonia.Platform.Storage;
using Eede.Application.UseCase.Colors;
using Eede.Domain.Colors;
using Eede.Presentation.Common.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Web;

namespace Eede.Presentation.ViewModels.DataEntry;

public class PaletteContainerViewModel : ViewModelBase
{
    [Reactive] public Palette Palette { get; set; }

    public ReactiveCommand<int, Unit> ApplyColorCommand { get; }
    public ReactiveCommand<int, Unit> FetchColorCommand { get; }

    public event Action<ArgbColor> OnFetchColor;
    public event Func<ArgbColor> OnApplyColor;

    public ReactiveCommand<StorageService, Unit> LoadPaletteCommand { get; }
    public ReactiveCommand<StorageService, Unit> SavePaletteCommand { get; }

    public PaletteContainerViewModel()
    {
        Palette = Palette.Create();

        ApplyColorCommand = ReactiveCommand.Create<int>(ExecuteApplyColor);
        FetchColorCommand = ReactiveCommand.Create<int>(ExecuteFetchColor);

        LoadPaletteCommand = ReactiveCommand.Create<StorageService>(ExecuteLoadPalette);
        SavePaletteCommand = ReactiveCommand.Create<StorageService>(ExecuteSavePalette);
    }

    //パレットフォームのクリックイベントを拾って親に伝達できるようにする
    //保存読み込み処理は自前で処理する
    //このＶＭ自体は親ｖｍに持たせる


    private void ExecuteApplyColor(int number)
    {
        ArgbColor color = OnApplyColor.Invoke();
        if (color == null)
        {
            return;
        }
        Palette = Palette.Apply(number, color);
    }

    private void ExecuteFetchColor(int number)
    {
        OnFetchColor?.Invoke(Palette.Fetch(number));
    }

    private async void ExecuteLoadPalette(StorageService storage)
    {
        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            FileTypeFilter = [
                 new("Palette File")
                 {
                    Patterns = ["*.aact", "*.act"],
                    MimeTypes = ["image/*"]
                 },
                 new("Palette File (RGBA)")
                 {
                    Patterns = ["*.aact"],
                 },
                  new("Palette File (RGB)")
                 {
                    Patterns = ["*.act"],
                 },
            ]
            //        Title = Title,
        };

        IReadOnlyList<IStorageFile> result = await storage.StorageProvider.OpenFilePickerAsync(options);
        if (result == null || result.Count == 0)
        {
            return;
        }

        string filePath = HttpUtility.UrlDecode(result[0].Path.AbsolutePath);
        IPaletteFileReader reader = new FindPaletteFileReaderUseCase().Execute(filePath);

        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
        Palette = new LoadPaletteFileUseCase(reader).Execute(fs);
    }

    private async void ExecuteSavePalette(StorageService storage)
    {
        FilePickerSaveOptions options = new()
        {
            SuggestedFileName = "palette",
            DefaultExtension = "aact", // デフォルトの拡張子を設定
            FileTypeChoices = [
                 new("Palette File (RGBA)")
                 {
                    Patterns = ["*.aact"],
                 },
            ]
        };
        IStorageFile result = await storage.StorageProvider.SaveFilePickerAsync(options);
        if (result == null)
        {
            return;
        }

        await using Stream stream = await result.OpenWriteAsync();
        new SavePaletteFileUseCase(new AlphaActFileWriter()).Execute(stream, Palette);
    }
}
