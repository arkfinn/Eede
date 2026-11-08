using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Eede.Presentation.ViewModels.Animations;
using System;

namespace Eede.Presentation.Views.Animations;

public partial class AnimationSequencerView : UserControl
{
    private sealed record FrameDragPayload(int FromIndex);

    private static readonly DataFormat<FrameDragPayload> FramePayloadFormat =
        DataFormat.CreateInProcessFormat<FrameDragPayload>("Eede.AnimationFramePayload");

    public AnimationViewModel? ViewModel => DataContext as AnimationViewModel;

    private bool _isDragHandlePressed;
    private Point _dragStartPoint;
    private PointerPressedEventArgs? _pressedEventArgs;
    private ListBoxItem? _dragSourceItem;

    public AnimationSequencerView()
    {
        InitializeComponent();

        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnDragHandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Visual visual)
        {
            _isDragHandlePressed = true;
            _dragStartPoint = e.GetPosition(this);
            _pressedEventArgs = e;
            _dragSourceItem = visual.FindAncestorOfType<ListBoxItem>();
        }
    }

    private async void OnDragHandlePointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragHandlePressed || _dragSourceItem == null || _pressedEventArgs == null) return;

        var currentPoint = e.GetPosition(this);
        var diff = currentPoint - _dragStartPoint;
        if (Math.Abs(diff.X) > 3 || Math.Abs(diff.Y) > 3)
        {
            _isDragHandlePressed = false;
            var listBox = this.FindControl<ListBox>("SequencerListBox");
            if (listBox != null && _dragSourceItem != null)
            {
                int fromIndex = listBox.IndexFromContainer(_dragSourceItem);
                if (fromIndex >= 0)
                {
                    var dataTransfer = new DataTransfer();
                    dataTransfer.Add(DataTransferItem.Create(FramePayloadFormat, new FrameDragPayload(fromIndex)));
                    await DragDrop.DoDragDropAsync(_pressedEventArgs, dataTransfer, DragDropEffects.Move);
                }
            }
            _dragSourceItem = null;
            _pressedEventArgs = null;
        }
    }

    private void OnDragHandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragHandlePressed = false;
        _dragSourceItem = null;
        _pressedEventArgs = null;
    }

    private void OnDragHandlePointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _isDragHandlePressed = false;
        _dragSourceItem = null;
        _pressedEventArgs = null;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(FramePayloadFormat)) return;

        e.DragEffects = DragDropEffects.Move;
        e.Handled = true;

        var (slotIndex, indicatorY) = CalculateDropSlot(e);
        var indicator = this.FindControl<Border>("DropIndicator");
        var listBox = this.FindControl<ListBox>("SequencerListBox");
        if (indicator != null && listBox != null)
        {
            indicator.IsVisible = true;
            Canvas.SetTop(indicator, indicatorY);
            Canvas.SetLeft(indicator, 4);
            indicator.Width = Math.Max(50, listBox.Bounds.Width - 8);
        }
    }

    private void OnDragLeave(object? sender, DragEventArgs e)
    {
        var indicator = this.FindControl<Border>("DropIndicator");
        if (indicator != null)
        {
            indicator.IsVisible = false;
        }
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        var indicator = this.FindControl<Border>("DropIndicator");
        if (indicator != null)
        {
            indicator.IsVisible = false;
        }

        if (!e.DataTransfer.Contains(FramePayloadFormat)) return;

        e.Handled = true;
        var payload = e.DataTransfer.TryGetValue(FramePayloadFormat);
        if (payload != null)
        {
            var (targetSlot, _) = CalculateDropSlot(e);
            ViewModel?.MoveFrameCommand.Execute((payload.FromIndex, targetSlot)).Subscribe();
        }
    }

    private (int SlotIndex, double IndicatorY) CalculateDropSlot(DragEventArgs e)
    {
        var listBox = this.FindControl<ListBox>("SequencerListBox");
        var canvas = this.FindControl<Canvas>("IndicatorCanvas");
        if (listBox == null || canvas == null) return (0, 0);

        int totalCount = ViewModel?.SelectedPattern?.Frames.Count ?? 0;
        if (totalCount == 0) return (0, 0);

        var posInListBox = e.GetPosition(listBox);
        var hitControl = listBox.InputHitTest(posInListBox) as Visual;
        var item = hitControl?.FindAncestorOfType<ListBoxItem>();

        if (item != null)
        {
            int index = listBox.IndexFromContainer(item);
            if (index >= 0)
            {
                var itemPos = e.GetPosition(item);
                var bounds = item.Bounds;
                bool isTopHalf = itemPos.Y < (bounds.Height / 2.0);

                int targetSlot = isTopHalf ? index : index + 1;
                Point relativePoint = isTopHalf ? new Point(0, 0) : new Point(0, bounds.Height);
                Point? canvasPoint = item.TranslatePoint(relativePoint, canvas);
                double y = canvasPoint?.Y ?? 0;
                return (targetSlot, y);
            }
        }

        // 余白領域またはアイテムが見つからない場合は末尾
        var lastContainer = listBox.ContainerFromIndex(totalCount - 1) as Visual;
        if (lastContainer != null)
        {
            Point? bottomPoint = lastContainer.TranslatePoint(new Point(0, lastContainer.Bounds.Height), canvas);
            return (totalCount, bottomPoint?.Y ?? 0);
        }

        return (totalCount, 0);
    }

    private void OnRemoveFrameClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Visual visual)
        {
            var listBox = this.FindControl<ListBox>("SequencerListBox");
            var item = visual.FindAncestorOfType<ListBoxItem>();
            if (listBox != null && item != null)
            {
                int index = listBox.IndexFromContainer(item);
                if (index >= 0)
                {
                    ViewModel?.RemoveFrameIndexCommand.Execute(index).Subscribe();
                }
            }
        }
    }
}
