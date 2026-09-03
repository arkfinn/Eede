using Eede.Domain.Palettes;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Reactive.Linq;

namespace Eede.Presentation.ViewModels.DataEntry;

public partial class PaletteTabViewModel : ViewModelBase
{
    private Palette _basePalette;
    public bool IsClosable { get; }
    public string CustomTitle { get; }
    public string? SourceIdentity { get; }

    [Reactive] public partial Palette Palette { get; set; }
    [Reactive] public partial string? FilePath { get; set; }
    [Reactive] public partial bool IsDirty { get; set; }

    public PaletteTabViewModel(Palette palette, string? filePath = null)
        : this(palette, filePath, filePath != null, filePath == null ? "一時パレット" : System.IO.Path.GetFileNameWithoutExtension(filePath), filePath)
    {
    }

    public PaletteTabViewModel(Palette palette, string? filePath, bool isClosable, string title, string? sourceIdentity = null)
    {
        _basePalette = palette;
        Palette = palette;
        FilePath = filePath;
        IsClosable = isClosable;
        CustomTitle = title;
        SourceIdentity = sourceIdentity;

        this.WhenAnyValue(x => x.Palette)
            .Subscribe(p =>
            {
                IsDirty = (FilePath != null || IsClosable) && !p.Equals(_basePalette);
                this.RaisePropertyChanged(nameof(Title));
            });
    }

    public string Title => CustomTitle + (IsDirty ? "*" : "");

    public void ResetDirty()
    {
        _basePalette = Palette;
        IsDirty = false;
        this.RaisePropertyChanged(nameof(Title));
    }
}
