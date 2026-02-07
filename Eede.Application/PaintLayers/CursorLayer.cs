using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eede.Application.PaintLayers
{
    public class CursorLayer : IPaintLayer
    {
        private readonly MagnifiedSize PaintSize;
        private readonly Picture Source;
        private readonly PenStyle PenStyle;
        private readonly Position Position;
        private readonly IImageTransfer ImageTransfer;

        public CursorLayer(MagnifiedSize paintSize, Picture source, PenStyle penStyle, Position position, IImageTransfer imageTransfer)
        {
            PaintSize = paintSize;
            Source = source;
            PenStyle = penStyle;
            Position = position;
            ImageTransfer = imageTransfer;
        }

        public void Paint(Graphics destination)
        {
            Drawer drawer = new(Source, PenStyle);
            var result = drawer.DrawPoint(Position);
            Picture data = result.Picture.Transfer(ImageTransfer, PaintSize.Magnification);
            using Bitmap dest = BitmapConverter.Convert(data);
            destination.PixelOffsetMode = PixelOffsetMode.Half;
            destination.InterpolationMode = InterpolationMode.NearestNeighbor;
            destination.DrawImage(dest, new Point(0, 0));
        }

        public Picture Painted(Picture destination)
        {
            // 1. 背景をトーン変換する（拡大は View 側の GPU スケーリングに任せるため、ここでは等倍で行う）
            Picture source = destination.Transfer(ImageTransfer, new Magnification(1));

            // 2. 表示用の色を決定する
            ArgbColor originalColor = PenStyle.Color;
            ArgbColor displayColor = originalColor;
            if (ImageTransfer is AlphaToneImageTransfer)
            {
                // アルファモードなら、ペンのアルファ値を輝度にする
                displayColor = new ArgbColor(255, originalColor.Alpha, originalColor.Alpha, originalColor.Alpha);
            }
            else if (ImageTransfer is RGBToneImageTransfer)
            {
                // RGBモードなら、ペンのRGB値を使い、アルファは不透明にする
                displayColor = new ArgbColor(255, originalColor.Red, originalColor.Green, originalColor.Blue);
            }

            // 3. 背景と同じサイズの透明なレイヤーを用意し、そこに等倍のペン先を描く
            Picture cursorLayer = Picture.CreateEmpty(source.Size);
            // カーソル描画自体は上書き(Direct)で行う
            PenStyle displayPenStyle = new(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), displayColor, PenStyle.Width);
            Drawer drawer = new(cursorLayer, displayPenStyle);

            // 描画位置は元の座標を使用
            var result = drawer.DrawPoint(Position);

            // 4. トーン変換済みの背景に対して、ペン先のみをアルファブレンドで重ねる
            return source.Blend(new Eede.Domain.ImageEditing.Blending.AlphaImageBlender(), result.Picture, new Position(0, 0));
        }
    }
}