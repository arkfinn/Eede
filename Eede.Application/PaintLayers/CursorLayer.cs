using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.ImageEditing.Transformation;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;

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

            // 3. ペン先を描画する
            PenStyle displayPenStyle = new(PenStyle.Blender, displayColor, PenStyle.Width);

            if (PenStyle.Blender is Eede.Domain.ImageEditing.Blending.AlphaImageBlender)
            {
                // アルファブレンドペンの場合、背景全体の二重ブレンドを防ぐため空レイヤー経由で合成
                Picture cursorLayer = Picture.CreateEmpty(source.Size);
                Drawer drawer = new(cursorLayer, displayPenStyle);
                var result = drawer.DrawPoint(Position);
                return source.Blend(new Eede.Domain.ImageEditing.Blending.AlphaImageBlender(), result.Picture, new Position(0, 0));
            }
            else
            {
                // 直接上書き系（Direct, RGBOnly, AlphaOnly等）の場合、背景に直接描くことで
                // A=0 の透明色や A=1 などのアルファ値がそのまま正しく反映される（消去プレビュー含む）
                Drawer drawer = new(source, displayPenStyle);
                return drawer.DrawPoint(Position).Picture;
            }
        }
    }
}