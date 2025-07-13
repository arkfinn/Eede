using Eede.Presentation.Common.Services;
using Eede.Presentation.Files;
using System;

namespace Eede.Presentation.Events;

public class PictureSaveEventArgs(IImageFile file, StorageService storage) : EventArgs
{
    public readonly StorageService Storage = storage;
    public IImageFile File { get; private set; } = file;

    public void UpdateFile(IImageFile file) => File = file;
}

