﻿using Eede.Domain.Colors;
using System;
using System.IO;

namespace Eede.Application.UseCase.Colors;

public class FindPaletteFileReaderUseCase
{
    public IPaletteFileReader Execute(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        if (extension == ".act")
        {
            return new ActFileReader();
        }
        return extension == ".pal" ? (IPaletteFileReader)new PaletteFileReader() : throw new ArgumentException(extension);
    }
}