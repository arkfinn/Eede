using Eede.Domain.Files;
using Eede.Domain.Pictures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
