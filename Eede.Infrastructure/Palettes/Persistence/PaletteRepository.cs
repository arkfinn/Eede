using Eede.Application.Infrastructure;
using Eede.Domain.Palettes;
using Eede.Infrastructure.Palettes.Persistence.ActFileFormat;
using System;
using System.IO;

namespace Eede.Infrastructure.Palettes.Persistence
{
    public class PaletteRepository : IPaletteRepository
    {
        public Palette Find(string filePath)
        {
            // Factory Methodのロジック
            IPaletteFileReader reader = CreateReader(Path.GetExtension(filePath));
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            return reader.Read(fs);
        }

        public Palette Find(Stream stream, string extension)
        {
            IPaletteFileReader reader = CreateReader(extension);
            return reader.Read(stream);
        }

        public void Save(Palette palette, string filePath)
        {
            IPaletteFileWriter writer = CreateWriter(Path.GetExtension(filePath));
            using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write);
            writer.Write(fs, palette);
        }

        public void Save(Palette palette, Stream stream, string extension)
        {
            IPaletteFileWriter writer = CreateWriter(extension);
            writer.Write(stream, palette);
        }

        // privateなFactory Methodとして責務を分離
        private IPaletteFileReader CreateReader(string extension)
        {
            string ext = extension.ToLower();
            if (ext == ".act")
            {
                return new ActFileReader();
            }
            if (ext == ".aact")
            {
                return new AlphaActFileReader();
            }
            throw new NotSupportedException($"Unsupported file extension: {extension}");
        }

        private IPaletteFileWriter CreateWriter(string extension)
        {
            string ext = extension.ToLower();
            if (ext == ".act")
            {
                return new ActFileWriter();
            }
            if (ext == ".aact")
            {
                return new AlphaActFileWriter();
            }
            throw new NotSupportedException($"Unsupported file extension: {extension}");
        }
    }
}
