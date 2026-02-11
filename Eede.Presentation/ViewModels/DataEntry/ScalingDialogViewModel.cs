using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Eede.Presentation.ViewModels.DataEntry
{
#nullable enable

    public class ScalingDialogViewModel : ReactiveObject
    {
        private readonly PictureSize OriginalSize;

        [Reactive] public int Width { get; set; }
        [Reactive] public int Height { get; set; }
        [Reactive] public double WidthPercent { get; set; }
        [Reactive] public double HeightPercent { get; set; }

        [Reactive] public bool IsLockAspectRatio { get; set; } = true;
        [Reactive] public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        [Reactive] public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

        [Reactive] public bool IsHorizontalShrink { get; private set; }
        [Reactive] public bool IsVerticalShrink { get; private set; }

        public bool IsTopLeftEnabled => true;
        public bool IsTopCenterEnabled => IsHorizontalShrink;
        public bool IsTopRightEnabled => IsHorizontalShrink;
        public bool IsCenterLeftEnabled => IsVerticalShrink;
        public bool IsCenterCenterEnabled => IsHorizontalShrink && IsVerticalShrink;
        public bool IsCenterRightEnabled => IsHorizontalShrink && IsVerticalShrink;
        public bool IsBottomLeftEnabled => IsVerticalShrink;
        public bool IsBottomCenterEnabled => IsHorizontalShrink && IsVerticalShrink;
        public bool IsBottomRightEnabled => IsHorizontalShrink && IsVerticalShrink;

        public ReactiveCommand<Unit, ResizeContext> OkCommand { get; }
        public ReactiveCommand<double, Unit> ApplyPreset { get; }
        public ReactiveCommand<string, Unit> SetAnchorCommand { get; }

        private bool isUpdating = false;

        public ScalingDialogViewModel(PictureSize originalSize)
        {
            OriginalSize = originalSize;
            Width = originalSize.Width;
            Height = originalSize.Height;
            WidthPercent = 100.0;
            HeightPercent = 100.0;
            RefreshStates();

            this.WhenAnyValue(x => x.Width)
                .Subscribe(UpdateFromWidth);

            this.WhenAnyValue(x => x.Height)
                .Subscribe(UpdateFromHeight);

            this.WhenAnyValue(x => x.WidthPercent)
                .Subscribe(UpdateFromWidthPercent);

            this.WhenAnyValue(x => x.HeightPercent)
                .Subscribe(UpdateFromHeightPercent);

            ApplyPreset = ReactiveCommand.Create<double>(factor =>
            {
                var percent = factor * 100.0;
                WidthPercent = percent;
                if (IsLockAspectRatio)
                {
                    HeightPercent = percent;
                }
            });

            SetAnchorCommand = ReactiveCommand.Create<string>(anchorKey =>
            {
                switch (anchorKey)
                {
                    case "TopLeft": HorizontalAlignment = HorizontalAlignment.Left; VerticalAlignment = VerticalAlignment.Top; break;
                    case "TopCenter": HorizontalAlignment = HorizontalAlignment.Center; VerticalAlignment = VerticalAlignment.Top; break;
                    case "TopRight": HorizontalAlignment = HorizontalAlignment.Right; VerticalAlignment = VerticalAlignment.Top; break;
                    case "CenterLeft": HorizontalAlignment = HorizontalAlignment.Left; VerticalAlignment = VerticalAlignment.Center; break;
                    case "CenterCenter": HorizontalAlignment = HorizontalAlignment.Center; VerticalAlignment = VerticalAlignment.Center; break;
                    case "CenterRight": HorizontalAlignment = HorizontalAlignment.Right; VerticalAlignment = VerticalAlignment.Center; break;
                    case "BottomLeft": HorizontalAlignment = HorizontalAlignment.Left; VerticalAlignment = VerticalAlignment.Bottom; break;
                    case "BottomCenter": HorizontalAlignment = HorizontalAlignment.Center; VerticalAlignment = VerticalAlignment.Bottom; break;
                    case "BottomRight": HorizontalAlignment = HorizontalAlignment.Right; VerticalAlignment = VerticalAlignment.Bottom; break;
                }
            });

            OkCommand = ReactiveCommand.Create(() => new ResizeContext(
                OriginalSize,
                new PictureSize(Width, Height),
                IsLockAspectRatio,
                HorizontalAlignment,
                VerticalAlignment
            ));
        }

        private void RefreshStates()
        {
            IsHorizontalShrink = Width < OriginalSize.Width;
            IsVerticalShrink = Height < OriginalSize.Height;

            if (!IsHorizontalShrink) HorizontalAlignment = HorizontalAlignment.Left;
            if (!IsVerticalShrink) VerticalAlignment = VerticalAlignment.Top;

            this.RaisePropertyChanged(nameof(IsTopCenterEnabled));
            this.RaisePropertyChanged(nameof(IsTopRightEnabled));
            this.RaisePropertyChanged(nameof(IsCenterLeftEnabled));
            this.RaisePropertyChanged(nameof(IsCenterCenterEnabled));
            this.RaisePropertyChanged(nameof(IsCenterRightEnabled));
            this.RaisePropertyChanged(nameof(IsBottomLeftEnabled));
            this.RaisePropertyChanged(nameof(IsBottomCenterEnabled));
            this.RaisePropertyChanged(nameof(IsBottomRightEnabled));
        }

        private void UpdateFromWidth(int w)
        {
            if (isUpdating) return;
            isUpdating = true;
            try
            {
                if (w < 1) { Width = 1; }
                if (IsLockAspectRatio && OriginalSize.Width > 0)
                {
                    Height = (int)Math.Max(1, Math.Round((double)Width * OriginalSize.Height / OriginalSize.Width));
                }
                UpdatePercents();
                RefreshStates();
            }
            finally { isUpdating = false; }
        }

        private void UpdateFromHeight(int h)
        {
            if (isUpdating) return;
            isUpdating = true;
            try
            {
                if (h < 1) { Height = 1; }
                if (IsLockAspectRatio && OriginalSize.Height > 0)
                {
                    Width = (int)Math.Max(1, Math.Round((double)Height * OriginalSize.Width / OriginalSize.Height));
                }
                UpdatePercents();
                RefreshStates();
            }
            finally { isUpdating = false; }
        }

        private void UpdateFromWidthPercent(double wp)
        {
            if (isUpdating) return;
            isUpdating = true;
            try
            {
                if (wp < 0.1) { WidthPercent = 0.1; }
                Width = (int)Math.Max(1, Math.Round(OriginalSize.Width * WidthPercent / 100.0));
                if (IsLockAspectRatio)
                {
                    HeightPercent = WidthPercent;
                    Height = (int)Math.Max(1, Math.Round(OriginalSize.Height * WidthPercent / 100.0));
                }
                else
                {
                    UpdatePercents();
                }
                RefreshStates();
            }
            finally { isUpdating = false; }
        }

        private void UpdateFromHeightPercent(double hp)
        {
            if (isUpdating) return;
            isUpdating = true;
            try
            {
                if (hp < 0.1) { HeightPercent = 0.1; }
                Height = (int)Math.Max(1, Math.Round(OriginalSize.Height * HeightPercent / 100.0));
                if (IsLockAspectRatio)
                {
                    WidthPercent = HeightPercent;
                    Width = (int)Math.Max(1, Math.Round(OriginalSize.Width * HeightPercent / 100.0));
                }
                else
                {
                    UpdatePercents();
                }
                RefreshStates();
            }
            finally { isUpdating = false; }
        }

        private void UpdatePercents()
        {
            WidthPercent = OriginalSize.Width > 0 ? (double)Width * 100.0 / OriginalSize.Width : 100.0;
            HeightPercent = OriginalSize.Height > 0 ? (double)Height * 100.0 / OriginalSize.Height : 100.0;
        }
    }
}
