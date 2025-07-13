using System;
using System.Windows.Forms;
using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Domain.Pictures;
using Eede.Presentation.Common.Adapters; // 追加

namespace Eede.Presentation.Files
{
    public record NewFile(Bitmap Bitmap) : IImageFile
    {
        public FilePath Path { get; } = FilePath.Empty();
        public bool IsNewFile() => true; // 新規ファイルは常に新規である
        public string GetPathString() => "";
        public bool ShouldPromptForSaveAs() => true; // 新規ファイルは常に名前を付けて保存を促す
        public IImageFile WithFilePath(FilePath filePath)
        {
            if (filePath.IsEmpty() || filePath.GetExtension() != ".png")
            {
                throw new ArgumentException("新規ファイルはPNG形式でなければなりません。");
            }
            return new PngFile(Bitmap, filePath);
        }
        public IImageFile WithBitmap(Bitmap bitmap) => this with { Bitmap = bitmap };
        public string Subject() => "新しいファイル"; // 新規ファイルのサブジェクトは固定
    }
}
