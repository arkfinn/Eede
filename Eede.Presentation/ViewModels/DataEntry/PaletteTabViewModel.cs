using Eede.Domain.Palettes;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive.Linq;

namespace Eede.Presentation.ViewModels.DataEntry;

public partial class PaletteTabViewModel : ViewModelBase
{
    private Palette _basePalette;
    public bool IsClosable => FilePath != null;

    [Reactive] public partial Palette Palette { get; set; }
    [Reactive] public partial string? FilePath { get; set; }
    [Reactive] public partial bool IsDirty { get; set; }

    public PaletteTabViewModel(Palette palette, string? filePath = null)
    {
        _basePalette = palette;
        Palette = palette;
        FilePath = filePath;

        this.WhenAnyValue(x => x.Palette)
            .Subscribe(p =>
            {
                IsDirty = FilePath != null && !p.Equals(_basePalette);
                this.RaisePropertyChanged(nameof(Title));
            });
    }

    public string Title => (FilePath == null ? "一時パレット" : System.IO.Path.GetFileNameWithoutExtension(FilePath)) + (IsDirty ? "*" : "");

    public void ResetDirty()
    {
        _basePalette = Palette;
        IsDirty = false;
        this.RaisePropertyChanged(nameof(Title));
    }
}
