using Eede.Domain.Palettes;
using Eede.Infrastructure.Palettes.Persistence.ActFileFormat;
using System;
using System.IO;

namespace Eede.Infrastructure.Palettes.Persistence
{
    public class PaletteRepository
    {
        public Palette Find(string filePath)
        {
            // Factory Methodのロジック
            IPaletteFileReader reader = CreateReader(filePath);
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            return reader.Read(fs);
        }

        // privateなFactory Methodとして責務を分離
        private IPaletteFileReader CreateReader(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            if (extension == ".act")
            {
                return new ActFileReader();
            }
            if (extension == ".aact")
            {
                return new AlphaActFileReader();
            }
            throw new NotSupportedException($"Unsupported file extension: {extension}");
        }
    }
}
