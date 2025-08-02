using Eede.Domain.Positions;
using System;

namespace Eede.Domain.Sizes
{
    public readonly record struct PictureSize
    {
        public int Width { get; }
        public int Height { get; }

        public PictureSize(int width, int height)
        {
            if (width < 0 || height < 0)
            {
                throw new ArgumentException($"invalid argument.width:{width}, height:{height}");
            }
            Width = width;
            Height = height;
        }

        /// <summary>
        /// 指定された位置がこのサイズの範囲内にあるかどうかを確認します。
        /// </summary>
        /// <param name="pos">確認する位置。</param>
        /// <returns>
        /// 指定された位置がこのサイズの範囲内にある場合は <c>true</c>、それ以外の場合は <c>false</c>。
        /// </returns>
        public bool Contains(Position pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;
        }

        /// <summary>
        /// 幅と高さの値を入れ替えた新しい <see cref="PictureSize"/> インスタンスを返します。
        /// </summary>
        /// <returns>
        /// 幅が元の高さ、高さが元の幅に設定された <see cref="PictureSize"/> オブジェクトを返します。
        /// </returns>
        public PictureSize Swap()
        {
            return new PictureSize(Height, Width);
        }
    }
}
