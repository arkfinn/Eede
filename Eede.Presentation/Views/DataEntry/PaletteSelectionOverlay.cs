using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Eede.Domain.Palettes;
using System;

namespace Eede.Presentation.Views.DataEntry;

public sealed class PaletteSelectionOverlay : Control
{
    public static readonly StyledProperty<Palette> PaletteProperty =
        AvaloniaProperty.Register<PaletteSelectionOverlay, Palette>(nameof(Palette), Palette.Create());

    public Palette Palette
    {
        get => GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public static readonly StyledProperty<ArgbColor> SelectedColorProperty =
        AvaloniaProperty.Register<PaletteSelectionOverlay, ArgbColor>(nameof(SelectedColor), default);

    public ArgbColor SelectedColor
    {
        get => GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly StyledProperty<bool> IsDraggingProperty =
        AvaloniaProperty.Register<PaletteSelectionOverlay, bool>(nameof(IsDragging), false);

    public bool IsDragging
    {
        get => GetValue(IsDraggingProperty);
        set => SetValue(IsDraggingProperty, value);
    }

    public static readonly StyledProperty<int> DragStartSlotProperty =
        AvaloniaProperty.Register<PaletteSelectionOverlay, int>(nameof(DragStartSlot), -1);

    public int DragStartSlot
    {
        get => GetValue(DragStartSlotProperty);
        set => SetValue(DragStartSlotProperty, value);
    }

    public static readonly StyledProperty<int> HoverSlotProperty =
        AvaloniaProperty.Register<PaletteSelectionOverlay, int>(nameof(HoverSlot), -1);

    public int HoverSlot
    {
        get => GetValue(HoverSlotProperty);
        set => SetValue(HoverSlotProperty, value);
    }

    private static readonly Color FallbackThemeColor = Color.Parse("#00C853");

    private IPen? _outerBlackPen;
    private IPen? _innerThemePen;

    static PaletteSelectionOverlay()
    {
        AffectsRender<PaletteSelectionOverlay>(
            PaletteProperty,
            SelectedColorProperty,
            IsDraggingProperty,
            DragStartSlotProperty,
            HoverSlotProperty);
    }

    public PaletteSelectionOverlay()
    {
        IsHitTestVisible = false;
        ActualThemeVariantChanged += (_, _) =>
        {
            _innerThemePen = null;
            InvalidateVisual();
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _innerThemePen = null;
        InvalidateVisual();
    }

    internal static bool IsSlotSelected(
        Palette? palette,
        int slot,
        ArgbColor selectedColor,
        bool isDragging,
        int dragStartSlot,
        int hoverSlot)
    {
        if (palette == null || selectedColor.Alpha == 0)
        {
            return false;
        }
        if (slot < 0 || slot >= Palette.MAX_LENGTH)
        {
            return false;
        }
        if (isDragging && (slot == dragStartSlot || slot == hoverSlot))
        {
            return false;
        }
        return palette.Fetch(slot).EqualsArgb(selectedColor);
    }

    private IPen GetOuterBlackPen()
    {
        return _outerBlackPen ??= new Pen(Brushes.Black, 1);
    }

    private IPen GetInnerThemePen()
    {
        return _innerThemePen ??= CreateInnerThemePen();
    }

    private IPen CreateInnerThemePen()
    {
        if (this.TryFindResource("ToolBorderedBorderChecked", ActualThemeVariant, out var res))
        {
            if (res is IBrush brush)
            {
                return new Pen(brush, 1);
            }
            if (res is Color color)
            {
                return new Pen(new SolidColorBrush(color), 1);
            }
        }

        if (this.TryFindResource("SystemAccentColor", ActualThemeVariant, out var accentRes))
        {
            if (accentRes is Color accentColor)
            {
                return new Pen(new SolidColorBrush(accentColor), 1);
            }
            if (accentRes is IBrush accentBrush)
            {
                return new Pen(accentBrush, 1);
            }
        }

        return new Pen(new SolidColorBrush(FallbackThemeColor), 1);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var selectedColor = SelectedColor;
        var palette = Palette;
        if (palette == null || selectedColor.Alpha == 0)
        {
            return;
        }

        var outerPen = GetOuterBlackPen();
        var innerPen = GetInnerThemePen();
        bool isDragging = IsDragging;
        int dragStart = DragStartSlot;
        int hover = HoverSlot;

        for (int i = 0; i < Palette.MAX_LENGTH; i++)
        {
            if (!IsSlotSelected(palette, i, selectedColor, isDragging, dragStart, hover))
            {
                continue;
            }

            int x = (i % 16) * PaletteForm.CELL_WIDTH;
            int y = (i / 16) * PaletteForm.CELL_HEIGHT;
            context.DrawRectangle(null, outerPen, new Rect(x + 0.5, y + 0.5, PaletteForm.CELL_WIDTH - 1, PaletteForm.CELL_HEIGHT - 1));
            context.DrawRectangle(null, innerPen, new Rect(x + 1.5, y + 1.5, PaletteForm.CELL_WIDTH - 3, PaletteForm.CELL_HEIGHT - 3));
        }
    }
}
