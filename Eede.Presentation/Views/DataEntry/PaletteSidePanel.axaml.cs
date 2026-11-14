using Avalonia.Controls;
using Avalonia.Data.Converters;
using Eede.Domain.ImageEditing.DrawingTools;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Eede.Presentation.Views.DataEntry;

#nullable enable

public partial class PaletteSidePanel : UserControl
{
    public PaletteSidePanel()
    {
        InitializeComponent();
    }
}

public class TilePatternHighlightConverter : IMultiValueConverter
{
    public static readonly TilePatternHighlightConverter Instance = new();

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return values.Count >= 2 &&
               values[0] is TilePattern item &&
               values[1] is TilePattern selected &&
               item.Type == selected.Type;
    }
}
