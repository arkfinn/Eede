using System;
using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing
{
    public record ResizeContext
    {
        public PictureSize OriginalSize { get; init; }
        public PictureSize TargetSize { get; init; }
        public bool IsLockAspectRatio { get; init; }
        public HorizontalAlignment HorizontalAlignment { get; init; }
        public VerticalAlignment VerticalAlignment { get; init; }

        public ResizeContext(PictureSize originalSize, PictureSize targetSize, bool isLockAspectRatio, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            OriginalSize = originalSize;
            TargetSize = targetSize;
            IsLockAspectRatio = isLockAspectRatio;
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
        }

        public ResizeContext UpdateWidth(int width)
        {
            int height = IsLockAspectRatio && OriginalSize.Width > 0
                ? (int)Math.Round((double)width * OriginalSize.Height / OriginalSize.Width)
                : TargetSize.Height;
            return this with { TargetSize = new PictureSize(width, height) };
        }

        public ResizeContext UpdateHeight(int height)
        {
            int width = IsLockAspectRatio && OriginalSize.Height > 0
                ? (int)Math.Round((double)height * OriginalSize.Width / OriginalSize.Height)
                : TargetSize.Width;
            return this with { TargetSize = new PictureSize(width, height) };
        }

        public ResizeContext UpdateWidthPercent(double percent)
        {
            return UpdateWidth((int)Math.Round(OriginalSize.Width * percent / 100.0));
        }

        public ResizeContext UpdateHeightPercent(double percent)
        {
            return UpdateHeight((int)Math.Round(OriginalSize.Height * percent / 100.0));
        }

        public double GetWidthPercent()
        {
            return OriginalSize.Width > 0 ? (double)TargetSize.Width * 100.0 / OriginalSize.Width : 100.0;
        }

        public double GetHeightPercent()
        {
            return OriginalSize.Height > 0 ? (double)TargetSize.Height * 100.0 / OriginalSize.Height : 100.0;
        }

        public Position CalculateOffset()
        {
            int x = HorizontalAlignment switch
            {
                HorizontalAlignment.Left => 0,
                HorizontalAlignment.Center => (OriginalSize.Width - TargetSize.Width) / 2,
                HorizontalAlignment.Right => OriginalSize.Width - TargetSize.Width,
                _ => 0
            };

            int y = VerticalAlignment switch
            {
                VerticalAlignment.Top => 0,
                VerticalAlignment.Center => (OriginalSize.Height - TargetSize.Height) / 2,
                VerticalAlignment.Bottom => OriginalSize.Height - TargetSize.Height,
                _ => 0
            };

            return new Position(x, y);
        }
    }
}
