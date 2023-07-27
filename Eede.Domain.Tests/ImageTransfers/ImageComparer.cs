using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Eede.Domain.ImageTransfers
{
    internal class ImageComparer
    {
        public static bool Equals(Image image1, Image image2)
        {
            Bitmap img1 = (Bitmap)image1;
            Bitmap img2 = (Bitmap)image2;

            //高さが違えばfalse
            if (img1.Width != img2.Width || img1.Height != img2.Height) return false;
            //BitmapData取得
            BitmapData bd1 = img1.LockBits(new Rectangle(0, 0, img1.Width, img1.Height), ImageLockMode.ReadOnly, img1.PixelFormat);
            BitmapData bd2 = img2.LockBits(new Rectangle(0, 0, img2.Width, img2.Height), ImageLockMode.ReadOnly, img2.PixelFormat);
            try
            {
                //スキャン幅が違う場合はfalse
                if (bd1.Stride != bd2.Stride)
                {
                    return false;
                }
                int bsize = bd1.Stride * img1.Height;
                byte[] byte1 = new byte[bsize];
                byte[] byte2 = new byte[bsize];
                //バイト配列にコピー
                Marshal.Copy(bd1.Scan0, byte1, 0, bsize);
                Marshal.Copy(bd2.Scan0, byte2, 0, bsize);

                //MD5ハッシュを取る
                var md5 = MD5.Create();
                byte[] hash1 = md5.ComputeHash(byte1);
                byte[] hash2 = md5.ComputeHash(byte2);

                //ハッシュを比較
                return hash1.SequenceEqual(hash2);
            }
            finally
            {
                img1.UnlockBits(bd1);
                img2.UnlockBits(bd2);
            }

        }
    }
}
