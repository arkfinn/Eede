using Eede.Domain.ImageBlenders;
using Eede.Domain.Positions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Eede
{
    public class AlphaPicture : IDisposable
    {
        private Bitmap mBitmap;

        public Bitmap Bmp
        {
            get { return mBitmap; }
            set
            {
                if (value.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    throw new ArgumentException(
                        "AlphaPictureではPixelFormat.Format32bppArgbしか使えません:"
                        + value.PixelFormat.ToString() + "が設定されています"
                    );
                }
                EndAccess();
                mBitmap = value;
            }
        }

        #region コンストラクタ

        public AlphaPicture(Bitmap bitmap)
        {
            //newすると強制的にPixelFormat.Format32bppArgbになる
            Bmp = new Bitmap(bitmap);
        }

        ///// <summary>
        ///// 指定した既存のイメージを使用して、 ABitmap クラスの新しいインスタンスを初期化します。
        ///// </summary>
        ///// <param name="img"></param>
        //public AlphaPicture(Image img)
        //{
        //    Bmp = new Bitmap(img);
        //}

        ///// <summary>
        ///// 指定したデータ ストリームを使用して、 ABitmap クラスの新しいインスタンスを初期化します。
        ///// </summary>
        ///// <param name="stm"></param>
        //public AlphaPicture(Stream stm)
        //{
        //    Bmp = new Bitmap(stm);
        //}

        /// <summary>
        /// 指定したファイルで ABitmap クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="s"></param>
        public AlphaPicture(string s)
        {
            //画像によってはpixelformatが違うため二回作る
            using (Bitmap bmp = new Bitmap(s))
            {
                Bmp = new Bitmap(bmp);
            }
        }

        ///// <summary>
        ///// 指定したサイズを使用して、指定した既存のイメージで ABitmap クラスの新しいインスタンスを初期化します。

        /////// </summary>
        /////// <param name="img"></param>
        /////// <param name="sz"></param>
        ////public AlphaPicture(Image img, Size sz)
        ////{
        ////    Bmp = new Bitmap(img, sz);
        ////}

        /// <summary>
        /// 指定したサイズを使用して、 ABitmap クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public AlphaPicture(int x, int y)
        {
            Bmp = new Bitmap(x, y);
        }

        ///// <summary>
        ///// 指定したデータ ストリームを使用して、 ABitmap クラスの新しいインスタンスを初期化します。
        ///// </summary>
        ///// <param name="stm"></param>
        ///// <param name="b"></param>
        //public AlphaPicture(Stream stm, bool b)
        //{
        //    Bmp = new Bitmap(stm, b);
        //}

        ///// <summary>
        ///// 指定したファイルで ABitmap クラスの新しいインスタンスを初期化します。
        ///// </summary>
        ///// <param name="s"></param>
        ///// <param name="b"></param>
        //public AlphaPicture(string s, bool b)
        //{
        //    Bmp = new Bitmap(s, b);
        //}

        ///// <summary>
        ///// 指定したリソースで Bitmap クラスの新しいインスタンスを初期化します。
        ///// </summary>
        ///// <param name="t"></param>
        ///// <param name="s"></param>
        //public AlphaPicture(Type t, string s)
        //{
        //    Bmp = new Bitmap(t, s);
        //}

        ///// <summary>
        ///// 指定したサイズを使用して、指定した既存のイメージで Bitmap クラスの新しいインスタンスを初期化します。
        ///// </summary>
        ///// <param name="img"></param>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        //public AlphaPicture(Image img, int x, int y)
        //{
        //    Bmp = new Bitmap(img, x, y);
        //}

        ///// <summary>
        ///// 指定したサイズと指定した Graphics オブジェクトの解像度を使用して、 Bitmap クラスの新しいインスタンスを初期化します。
        ///// </summary>
        ///// <param name="x"></param>
        ///// <param name="y"></param>
        ///// <param name="gpc"></param>
        //public AlphaPicture(int x, int y, Graphics gpc)
        //{
        //    Bmp = new Bitmap(x, y, gpc);
        //}

        /* pixelFormat固定のため以下は割愛
指定したサイズと形式を使用して、 Bitmap クラスの新しいインスタンスを初期化します。
public ABitmap(int x, int y, PixelFormat pxf)
                {
    Bitmap = new Bitmap(x,y,pxf        );
}

[C++] public: Bitmap(int, int, PixelFormat);
[JScript] public function Bitmap(int, int, PixelFormat);
指定したサイズ、ピクセル形式、およびピクセル データを使用して、 Bitmap クラスの新しいインスタンスを初期化します。

[Visual Basic] Public Sub New(Integer, Integer, Integer, PixelFormat, IntPtr)
[C#] public Bitmap(int, int, int, PixelFormat, IntPtr);
[C++] public: Bitmap(int, int, int, PixelFormat, IntPtr);
[JScript] public function Bitmap(int, int, int, PixelFormat, IntPtr);
        */

        #endregion コンストラクタ

        #region Bitmapのラッププロパティ

        //Flags (Image から継承されます) この Image オブジェクトの属性フラグを取得します。
        //FrameDimensionsList (Image から継承されます) この Image オブジェクト内のフレームのディメンションを表す GUID の配列を取得します。

        /// <summary>
        /// このイメージの高さを取得します。
        /// </summary>
        public int Height
        {
            get { return Bmp.Height; }
        }

        //HorizontalResolution (Image から継承されます) この Image オブジェクトの水平方向の解像度を 1 インチあたりのピクセル数で取得します。
        //Palette (Image から継承されます) この Image オブジェクトに使用する、カラー パレットを取得または設定します。
        //PhysicalDimension (Image から継承されます) このイメージの幅と高さを取得します。
        //PixelFormat (Image から継承されます) この Image オブジェクトのピクセル形式を取得します。
        //PropertyIdList (Image から継承されます) この Image オブジェクトに格納されたプロパティ項目の ID を取得します。
        //PropertyItems (Image から継承されます) この Image オブジェクトに格納されたすべてのプロパティ項目 (メタデータの一部) を取得します。
        //RawFormat (Image から継承されます) この Image オブジェクトの形式を取得します。

        /// <summary>
        /// このイメージの幅と高さ (ピクセル単位) を取得します。
        /// </summary>
        public Size Size
        {
            get { return Bmp.Size; }
        }

        //VerticalResolution (Image から継承されます) この Image オブジェクトの垂直方向の解像度を 1 インチあたりのピクセル数で取得します。

        /// <summary>
        ///  このイメージの幅を取得します。
        /// </summary>
        public int Width
        {
            get { return Bmp.Width; }
        }

        #endregion Bitmapのラッププロパティ

        #region Bitmapのラップメソッド

        //

        //Clone オーバーロード。 指定された PixelFormat で定義されたこの Bitmap のセクションのコピーを作成します。
        //CreateObjRef (MarshalByRefObject から継承されます)
        //リモート オブジェクトとの通信に使用するプロキシの生成に必要な情報をすべて格納しているオブジェクトを作成します。

        #region IDisposable Support

        private bool IsDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                EndAccess();
                Bmp.Dispose();
            }
            IsDisposed = true;
        }

        ~AlphaPicture()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        #endregion Bitmapのラップメソッド

        #region BitmapDataアクセス関数

        private BitmapData tempBitmapData = null;

        //---------------------------------------------
        // 直接アクセス開始
        //---------------------------------------------
        protected BitmapData BeginAccess()
        {
            if (tempBitmapData != null)
            {
                return tempBitmapData;
                //throw new InvalidOperationException("EndAccess呼び出し前に次のBeginAccessが実行されました");
            }
            tempBitmapData = Bmp.LockBits(new Rectangle(0, 0, Bmp.Width, Bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            return tempBitmapData;
        }

        //---------------------------------------------
        // 直接アクセス終了
        //---------------------------------------------
        protected void EndAccess()
        {
            if (tempBitmapData == null)
            {
                return;
                ////throw new InvalidOperationException("BeginAccess呼び出し前にEndAccessが実行されました");
            }
            Bmp.UnlockBits(tempBitmapData);
            tempBitmapData = null;
        }

        public bool IsFastMode()
        {
            return tempBitmapData != null;
        }

        #endregion BitmapDataアクセス関数

        public Graphics GetGraphics()
        {
            EndAccess();
            var g = Graphics.FromImage(Bmp);

            g.CompositingMode = CompositingMode.SourceCopy;
            return g;
        }

        #region 描画命令

        private Bitmap CreateTempBmp()
        {
            var tmp = new Bitmap(Bmp.Width, Bmp.Height);
            var d = new DirectImageBlender();
            d.Blend(Bmp, tmp);
            return tmp;
        }

        public void DrawPoint(PenCase p, Position pos)
        {
            int x = pos.X;
            int y = pos.Y;
            EndAccess();
            var tmp = CreateTempBmp();
            try
            {
                using (var g = Graphics.FromImage(tmp))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawLine(p.PreparePen(), new PointF((float)x, (float)y), new PointF((float)x + 0.001f, (float)y + 0.01f));
                }
                p.Blender.Blend(tmp, Bmp);
            }
            finally
            {
                tmp.Dispose();
            }
        }

        public void DrawLine(PenCase p, Position beginPos, Position endPos)
        {
            EndAccess();
            var tmp = CreateTempBmp();
            try
            {
                using (var g = Graphics.FromImage(tmp))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawLine(p.PreparePen(), beginPos.ToPoint(), endPos.ToPoint());
                }
                p.Blender.Blend(tmp, Bmp);
            }
            finally
            {
                tmp.Dispose();
            }
        }

        /// <summary>
        /// 指定位置と同色部分を指定色で塗りつぶし
        /// </summary>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        /// <param name="col"></param>
        public void Fill(Position pos, Color col)
        {
            int sx = pos.X;
            int sy = pos.Y;
            int cWidth = Width;
            int cHeight = Height;
            Color baseCol = GetPixel(sx, sy);

            if (GetPixel(sx, sy) == col)
            {
                return;
            }
            Stack<Point> buffer = new Stack<Point>();
            buffer.Push(new Point(sx, sy));
            Pen p = new Pen(col);
            while (buffer.Count > 0)
            {
                Point point = buffer.Pop();
                /* skip already painted */
                if (GetPixelFast(point.X, point.Y) == col) continue;

                int leftX = point.X;
                int rightX = point.X;
                /* search left point */
                for (; 0 < leftX; leftX--)
                {
                    if (GetPixelFast(leftX - 1, point.Y) != baseCol) break;
                }
                /* search right point */
                for (; rightX < cWidth - 1; rightX++)
                {
                    if (GetPixelFast(rightX + 1, point.Y) != baseCol) break;
                }
                /* paint from leftX to rightX */
                if (leftX == rightX)
                {
                    SetPixel(leftX, point.Y, col);
                }
                else
                {
                    //一時的に
                    //DrawLine(p, new Point(leftX, point.Y), new Point(rightX, point.Y));
                }
                /* search next lines */
                if (point.Y + 1 < cHeight)
                {
                    scanLine(leftX, rightX, point.Y + 1, buffer, baseCol);
                }
                if (point.Y - 1 >= 0)
                {
                    scanLine(leftX, rightX, point.Y - 1, buffer, baseCol);
                }
            }
            EndAccess();
        }

        private void scanLine(int leftX, int rightX, int y, Stack<Point> buffer, Color baseCol)
        {
            while (leftX <= rightX)
            {
                for (; leftX <= rightX; leftX++)
                {
                    if (GetPixelFast(leftX, y) == baseCol)
                    {
                        break;
                    }
                }
                if (rightX < leftX)
                {
                    break;
                }
                for (; leftX <= rightX; leftX++)
                {
                    if (GetPixelFast(leftX, y) != baseCol)
                    {
                        break;
                    }
                }
                buffer.Push(new Point(leftX - 1, y));
            }
        }

        #endregion 描画命令

        #region Pixel単位描画命令

        /// <summary>
        /// x,yの色を取得する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            if (tempBitmapData == null) return Bmp.GetPixel(x, y);
            if (!IsInnerBitmap(new Position(x, y)))
                throw new ArgumentOutOfRangeException("x,yが範囲外です");

            return GetPixelFast(x, y);
        }

        /// <summary>
        /// x,yの色を取得。範囲チェックが走らない分高速
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected Color GetPixelFast(int x, int y)
        {
            BitmapData img = BeginAccess();
            Color result;
            unsafe
            {
                int* adr = (int*)img.Scan0;
                int pos = GetScanLinePos(x, y, Bmp.Width);
                result = Color.FromArgb(adr[pos]);
            }
            return result;
        }

        /// <summary>
        /// x,yにcolをセットする
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="col"></param>
        public void SetPixel(int x, int y, Color col)
        {
            if (!IsInnerBitmap(new Position(x, y)))
                throw new ArgumentOutOfRangeException("x,yが範囲外です");

            BitmapData img = BeginAccess();
            unsafe
            {
                int* adr = (int*)img.Scan0;
                int pos = GetScanLinePos(x, y, Bmp.Width);
                adr[pos] = col.ToArgb();
            }
            EndAccess();
        }

        public void SetPixel(int x, int y, Color col, byte alpha)
        {
            float tgta = GetAlphaRate(alpha);
            float srca = 1 - tgta;

            PixelOperate(x, y, col, delegate (uint src)
            {
                return GetAlphaBlend(Color.FromArgb((int)src), col, srca, tgta);
            });
        }

        /// <summary>
        /// 値を加算する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="col"></param>
        public void AddPixel(int x, int y, Color col)
        {
            PixelOperate(x, y, col, delegate (uint src)
            {
                return GetAddColor(src, (uint)col.ToArgb());
            });
        }

        public void AddPixel(int x, int y, Color col, byte alpha)
        {
            PixelOperate(x, y, col, delegate (uint src)
            {
                return GetAddColor(src, GetTransparency(col, GetAlphaRate(alpha)));
            });
        }

        /// <summary>
        /// 値を減算する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="col"></param>
        public void SubPixel(int x, int y, Color col)
        {
            PixelOperate(x, y, col, delegate (uint src)
            {
                return GetSubColor(src, (uint)col.ToArgb());
            });
        }

        public void SubPixel(int x, int y, Color col, byte alpha)
        {
            PixelOperate(x, y, col, delegate (uint src)
            {
                return GetSubColor(src, GetTransparency(col, GetAlphaRate(alpha)));
            });
        }

        private delegate uint PixelOperateCallback(uint src);

        private void PixelOperate(int x, int y, Color col, PixelOperateCallback psc)
        {
            if (!IsInnerBitmap(new Position(x, y)))
                throw new ArgumentOutOfRangeException("x,yが範囲外です");

            BitmapData img = BeginAccess();
            unsafe
            {
                uint* adr = (uint*)img.Scan0;
                int pos = GetScanLinePos(x, y, Bmp.Width);

                adr[pos] = psc(adr[pos]);
            }
            EndAccess();
        }

        #endregion Pixel単位描画命令

        #region 画像合成

        /// <summary>
        /// 合成する
        /// </summary>
        /// <param name="toPt">書き込み先（自身）の開始位置</param>
        /// <param name="copySize">書き込みサイズ</param>
        /// <param name="ap"></param>
        /// <param name="fromPt">書き込み元の開始位置</param>
        public void CopyPicture(Point toPt, Size copySize, AlphaPicture ap, Point fromPt)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    return tgt;
                }
            );
        }

        /// <summary>
        /// 指定の透明度で合成する
        /// </summary>
        /// <param name="toPt">書き込み先（自身）の開始位置</param>
        /// <param name="copySize">書き込みサイズ</param>
        /// <param name="ap"></param>
        /// <param name="fromPt">書き込み元の開始位置</param>
        /// <param name="alpha">透明度</param>
        public void CopyPicture(Point toPt, Size copySize, AlphaPicture ap, Point fromPt, byte alpha)
        {
            float tgta = GetAlphaRate(alpha);
            float srca = 1 - tgta;
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    return GetAlphaBlend(Color.FromArgb((int)src), Color.FromArgb((int)tgt), srca, tgta);
                }
            );
        }

        /// <summary>
        /// 画像を加算合成する
        /// </summary>
        /// <param name="toPt">書き込み先（自身）の開始位置</param>
        /// <param name="copySize">書き込みサイズ</param>
        /// <param name="ap"></param>
        /// <param name="fromPt">書き込み元の開始位置</param>
        public void AddPicture(Point toPt, Size copySize, AlphaPicture ap, Point fromPt)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    return GetAddColor(src, tgt);
                }
            );
        }

        /// <summary>
        /// 画像を透明度付きで加算合成する
        /// </summary>
        /// <param name="toPt">書き込み先（自身）の開始位置</param>
        /// <param name="copySize">書き込みサイズ</param>
        /// <param name="ap"></param>
        /// <param name="fromPt">書き込み元の開始位置</param>
        public void AddPicture(Point toPt, Size copySize, AlphaPicture ap, Point fromPt, byte alpha)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    return GetAddColor(src, GetTransparency(Color.FromArgb((int)tgt), GetAlphaRate(alpha)));
                }
            );
        }

        /// <summary>
        /// 画像を減算合成する
        /// </summary>
        /// <param name="toPt">書き込み先（自身）の開始位置</param>
        /// <param name="copySize">書き込みサイズ</param>
        /// <param name="ap"></param>
        /// <param name="fromPt">書き込み元の開始位置</param>
        public void SubPicture(Point toPt, Size copySize, AlphaPicture ap, Point fromPt)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    return GetSubColor(src, tgt);
                }
            );
        }

        /// <summary>
        /// 画像を透明度付きで減算合成する
        /// </summary>
        /// <param name="toPt">書き込み先（自身）の開始位置</param>
        /// <param name="copySize">書き込みサイズ</param>
        /// <param name="ap"></param>
        /// <param name="fromPt">書き込み元の開始位置</param>
        public void SubPicture(Point toPt, Size copySize, AlphaPicture ap, Point fromPt, byte alpha)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    return GetSubColor(src, GetTransparency(Color.FromArgb((int)tgt), GetAlphaRate(alpha)));
                }
            );
        }

        private delegate uint AllPixelOperateWithTargetCallback(uint src, uint tgt);

        //２つの対象に対して指定部分を全て走査する
        private void AllPixelOperateWithTarget(Point toPt, Size copySize, AlphaPicture ap, Point fromPt, AllPixelOperateWithTargetCallback atc)
        {
            //範囲チェック（右下の）位置で判定する
            if (!IsInnerBitmap(new Position(toPt.X + copySize.Width, toPt.Y + copySize.Height))
                && !ap.IsInnerBitmap(new Position(fromPt.X + copySize.Width, fromPt.Y + copySize.Height)))
                throw new ArgumentOutOfRangeException("x,yが範囲外です");

            BitmapData source = BeginAccess();
            BitmapData target = ap.BeginAccess();
            unsafe
            {
                uint* src = (uint*)source.Scan0;
                uint* tgt = (uint*)target.Scan0;

                int nowY = toPt.Y;
                int nowFromY = fromPt.Y;
                int startX = toPt.X;
                int startFromX = fromPt.X;
                int maxYCount = copySize.Height;
                int maxXCount = copySize.Width - startX;
                int srcWidth = Bmp.Width;
                int tgtWidth = ap.Width;

                while (nowY < maxYCount)
                {
                    int nowX = 0;
                    int pos = GetScanLinePos(startX, nowY, srcWidth);
                    int fromPos = GetScanLinePos(startFromX, nowFromY, tgtWidth);
                    while (nowX < maxXCount)
                    {
                        int nowPos = pos + nowX;
                        src[nowPos] = atc(src[nowPos],
                            tgt[fromPos + nowX]);
                        ++nowX;
                    }
                    ++nowFromY;
                    ++nowY;
                }
            }
            EndAccess();
            ap.EndAccess();
        }

        #endregion 画像合成

        #region アルファブレンド

        /// <summary>
        /// アルファブレンドする
        /// </summary>
        /// <param name="toPt">書き込み先（自身）の開始位置</param>
        /// <param name="copySize">書き込みサイズ</param>
        /// <param name="ap"></param>
        /// <param name="fromPt">書き込み元の開始位置</param>
        public void AlphaBlend(Point toPt, Size copySize, AlphaPicture ap, Point fromPt)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    //tgtのα値を使う
                    float tgta = GetAlphaRate((int)GetAFromUint(tgt));
                    float srca = 1 - tgta;
                    return GetAlphaBlendWithoutA(Color.FromArgb((int)src), Color.FromArgb((int)tgt), srca, tgta);
                }
            );
        }

        public void AlphaBlend(Point toPt, Size copySize, AlphaPicture ap, Point fromPt, byte alpha)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    //tgtのα値を使う
                    float tgta = GetAlphaRate((int)GetAFromUint(tgt)) * GetAlphaRate(alpha);
                    float srca = 1 - tgta;
                    return GetAlphaBlendWithoutA(Color.FromArgb((int)src), Color.FromArgb((int)tgt), srca, tgta);
                }
            );
        }

        //AddAlphaBlend

        public void AddAlphaBlend(Point toPt, Size copySize, AlphaPicture ap, Point fromPt)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    return (
                        GetAddColor(src,
                            GetTransparency(Color.FromArgb((int)tgt),
                            GetAlphaRate((int)GetAFromUint(tgt))))
                            & 0x00FFFFFF)
                        | (src & 0xFF000000);
                }
            );
        }

        public void AddAlphaBlend(Point toPt, Size copySize, AlphaPicture ap, Point fromPt, byte alpha)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    float nowAlpha = GetAlphaRate((int)GetAFromUint(tgt)) * GetAlphaRate(alpha);
                    return (
                            GetAddColor(src,
                                GetTransparency(Color.FromArgb((int)tgt),
                                nowAlpha))
                            & 0x00FFFFFF)
                       | (src & 0xFF000000);
                }
            );
        }

        //SubAlphaBlend

        public void SubAlphaBlend(Point toPt, Size copySize, AlphaPicture ap, Point fromPt)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    return (
                        GetSubColor(src,
                            GetTransparency(Color.FromArgb((int)tgt),
                            GetAlphaRate((int)GetAFromUint(tgt))))
                        & 0x00FFFFFF)
                        | (src & 0xFF000000);
                }
            );
        }

        public void SubAlphaBlend(Point toPt, Size copySize, AlphaPicture ap, Point fromPt, byte alpha)
        {
            AllPixelOperateWithTarget(toPt, copySize, ap, fromPt,
                delegate (uint src, uint tgt)
                {
                    float nowAlpha = GetAlphaRate((int)GetAFromUint(tgt)) * GetAlphaRate(alpha);

                    return (
                            GetSubColor(src,
                                GetTransparency(Color.FromArgb((int)tgt),
                                nowAlpha))
                            & 0x00FFFFFF)
                        | (src & 0xFF000000);
                }
            );
        }

        #endregion アルファブレンド

        /// <summary>
        /// 一次元配列上の二次配列の位置を、x,y,wから求める
        /// </summary>
        /// <param name="x">0とする事で各行の先頭が取れる</param>
        /// <param name="y">行番号</param>
        /// <param name="stride">一行のバイト数</param>
        /// <returns></returns>
        private int GetScanLinePos(int x, int y, int w)
        {
            return x + (w * y);
        }

        private float GetAlphaRate(int alpha)
        {
            return (float)alpha / byte.MaxValue;
        }

        public bool IsInnerBitmap(Position pos)
        {
            return pos.IsInnerOf(Bmp.Size);
        }

        #region 演算

        /// <summary>
        /// 飽和演算用のマスクを取得します
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private uint GetMask(uint a, uint b)
        {
            uint tmp = (((a & b) + (((a ^ b) >> 1) & 0x7f7f7f7f)) & 0x80808080);
            return (tmp << 1) - (tmp >> 7);
        }

        /// <summary>
        /// aとbを加算した数を返します
        /// </summary>
        /// <param name="a">ARGB値</param>
        /// <param name="b">ARGB値</param>
        /// <returns>ARGB値</returns>
        private uint GetAddColor(uint a, uint b)
        {
            uint mask = GetMask(a, b);
            return (((a + b) - mask) | mask);
        }

        /// <summary>
        /// aとbを減算した数を返します
        /// </summary>
        /// <param name="a">ARGB値</param>
        /// <param name="b">ARGB値</param>
        /// <returns>ARGB値</returns>
        private uint GetSubColor(uint a, uint b)
        {
            uint mask = GetMask(a, b);
            return (((a - b) - mask) | mask);
        }

        private uint GetAlphaBlend(Color src, Color tgt, float srca, float tgta)
        {
            float a = (float)src.A * srca + ((float)tgt.A * tgta);
            float r = (float)src.R * srca + ((float)tgt.R * tgta);
            float g = (float)src.G * srca + ((float)tgt.G * tgta);
            float b = (float)src.B * srca + ((float)tgt.B * tgta);

            //uint mask_a = GetMask((uint)src.ToArgb(), 0);
            //uint a = ((((uint)((float)src.ToArgb() * srca)) - mask_a) | mask_a);

            //uint mask_b = GetMask((uint)tgt.ToArgb(), 0);
            //uint b= ((((uint)((float)tgt.ToArgb() * tgta)) - mask_b) | mask_b);

            //uint mask = GetMask(a,b);
            //return (int)(((a+b) - mask) | mask);

            // return (uint)Color.FromArgb((int)a, (int)r, (int)g, (int)b).ToArgb();

            //uint a = (uint)((float)src.A * srca + ((float)tgt.A * tgta));
            //uint r = (uint)((float)src.R * srca + ((float)tgt.R * tgta));
            //uint g = (uint)((float)src.G * srca + ((float)tgt.G * tgta));
            //uint b = (uint)((float)src.B * srca + ((float)tgt.B * tgta));

            return ((uint)a << 24) + ((uint)r << 16) + ((uint)g << 8) + (uint)b;
        }

        //A値を重ねない（重ね合わせ時)の合成
        private uint GetAlphaBlendWithoutA(Color src, Color tgt, float srca, float tgta)
        {
            float r = (float)src.R * srca + ((float)tgt.R * tgta);
            float g = (float)src.G * srca + ((float)tgt.G * tgta);
            float b = (float)src.B * srca + ((float)tgt.B * tgta);
            //            return (uint)Color.FromArgb(src.A, (int)r, (int)g, (int)b).ToArgb();
            return ((uint)src.A << 24) + ((uint)r << 16) + ((uint)g << 8) + (uint)b;
        }

        private uint GetTransparency(Color src, float alpha)
        {
            //int a = (int)((float)src.A * alpha);
            //int r = (float)src.R * alpha;
            //float g = (float)src.G * alpha;
            //float b = (float)src.B * alpha;
            //return (uint)Color.FromArgb((int)a, (int)r, (int)g, (int)b).ToArgb();
            uint a = (uint)((float)src.A * alpha);
            uint r = (uint)((float)src.R * alpha);
            uint g = (uint)((float)src.G * alpha);
            uint b = (uint)((float)src.B * alpha);
            return (a << 24) + (r << 16) + (g << 8) + b;
        }

        private uint GetAFromUint(uint val)
        {
            return (val & 0xFF000000) >> 24;
        }

        #endregion 演算
    }
}