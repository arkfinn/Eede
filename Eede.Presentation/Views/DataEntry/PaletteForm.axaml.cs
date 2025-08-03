using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Eede.Domain.Colors;
using Eede.Domain.DrawStyles;
using Eede.Domain.ImageBlenders;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Sizes;
using Eede.Presentation.Common.Adapters;
using System;
using System.Windows.Input;

namespace Eede.Presentation.Views.DataEntry;

public partial class PaletteForm : UserControl
{
    private const int CELL_WIDTH = 17;
    private const int CELL_HEIGHT = 11;

    public static readonly StyledProperty<Palette> PaletteProperty =
        AvaloniaProperty.Register<PaletteForm, Palette>(nameof(Palette), Palette.Create());
    public Palette Palette
    {
        get => GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public static readonly StyledProperty<ICommand?> PointerLeftButtonPressedCommandProperty =
        AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerLeftButtonPressedCommand));
    public ICommand? PointerLeftButtonPressedCommand
    {
        get => GetValue(PointerLeftButtonPressedCommandProperty);
        set => SetValue(PointerLeftButtonPressedCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> PointerRightButtonPressedCommandProperty =
        AvaloniaProperty.Register<DrawableCanvas, ICommand?>(nameof(PointerRightButtonPressedCommand));
    public ICommand? PointerRightButtonPressedCommand
    {
        get => GetValue(PointerRightButtonPressedCommandProperty);
        set => SetValue(PointerRightButtonPressedCommandProperty, value);
    }

    public PaletteForm()
    {
        InitializeComponent();
        int width = CELL_WIDTH * 16;
        int height = CELL_HEIGHT * 16;
        PaletteBuffer = Picture.CreateEmpty(new PictureSize(width, height));
        panel.Width = width;
        panel.Height = height;
        image.Width = width;
        image.Height = height;
        PaletteBitmap = PictureBitmapAdapter.ConvertToBitmap(PaletteBuffer);
        image.Source = PaletteBitmap;
        Refresh();

        _ = this.GetObservable(PaletteProperty).Subscribe(_ =>
        {
            Refresh();
        });

        image.PointerPressed += OnCanvasPointerPressed;

    }

    private readonly Picture PaletteBuffer;
    public Bitmap PaletteBitmap;

    private void Refresh()
    {
        Picture Buffer = PaletteBuffer;
        Palette.ForEach((color, index) =>
        {
            Drawer drawer = new(Buffer, new PenStyle(new DirectImageBlender(), color, 1));
            int x = index % 16 * CELL_WIDTH;
            int y = index / 16 * CELL_HEIGHT;
            Buffer = drawer.DrawFillRectangle(new Position(x, y), new Position(x + CELL_WIDTH - 1, y + CELL_HEIGHT - 1));
        });
        PaletteBitmap?.Dispose();
        PaletteBitmap = PictureBitmapAdapter.ConvertToBitmap(Buffer);
        image.Source = PaletteBitmap;
    }

    private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Point pos = e.GetPosition(image);
        int number = (int)(pos.X / CELL_WIDTH) + ((int)(pos.Y / CELL_HEIGHT) * 16);
        PointerPointProperties pointer = e.GetCurrentPoint(image).Properties;
        if (pointer.IsLeftButtonPressed)
        {
            PointerLeftButtonPressedCommand?.Execute(number);
        }

        if (pointer.IsRightButtonPressed)
        {
            PointerRightButtonPressedCommand?.Execute(number);
        }
    }
}
