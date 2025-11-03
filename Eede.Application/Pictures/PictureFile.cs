using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using System;

namespace Eede.Application.Pictures
{
    public class PictureFile
    {
        public readonly Picture Picture;
        public readonly FilePath FilePath;

        public PictureFile(FilePath filePath, Picture picture)
        {
            Picture = picture ?? throw new ArgumentNullException(nameof(picture));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }
    }
}
