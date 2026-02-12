using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Application.UseCase.Pictures
{
    public class ScalingImageUseCase : IScalingImageUseCase
    {
        private readonly IImageResampler Resampler;

        public ScalingImageUseCase(IImageResampler resampler)
        {
            Resampler = resampler;
        }

        public DrawingSession Execute(DrawingSession session, ResizeContext context)
        {
            if (session.CurrentSelectingArea is PictureArea selection)
            {
                return ExecuteSelectionResize(session, selection, context);
            }
            else
            {
                return ExecuteCanvasResize(session, context);
            }
        }

        private DrawingSession ExecuteSelectionResize(DrawingSession session, PictureArea selection, ResizeContext context)
        {
            Picture region = session.Buffer.Fetch().CutOut(selection);
            Picture resized = Resampler.Resize(region, context.TargetSize);

            // 選択範囲のリサイズ時は「切り取り＆移動」プレビュー状態へ
            Position offset = context.CalculateOffset();
            Position newPosition = selection.Position + offset;

            SelectionPreviewInfo preview = new(resized, newPosition, SelectionPreviewType.CutAndMove, selection);
            return session.UpdatePreviewContent(preview);
        }

        private DrawingSession ExecuteCanvasResize(DrawingSession session, ResizeContext context)
        {
            Picture source = session.Buffer.Fetch();
            Picture resized = Resampler.Resize(source, context.TargetSize);

            // 拡大時はキャンバスを大きくする、縮小時は維持する
            int nextWidth = Math.Max(source.Width, context.TargetSize.Width);
            int nextHeight = Math.Max(source.Height, context.TargetSize.Height);
            PictureSize nextSize = new(nextWidth, nextHeight);

            Position offset = context.CalculateOffset();
            // 新しいキャンバス（透明または背景色）にリサイズ後の画像を配置
            Picture newPicture = Picture.CreateEmpty(nextSize).Blend(new DirectImageBlender(), resized, offset);

            return session.Push(newPicture);
        }
    }
}
