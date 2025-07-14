using Eede.Presentation.Common.Services;
using Eede.Presentation.Files;
using System;

namespace Eede.Presentation.Events;

public class PictureSaveEventArgs(IImageFile file, StorageService storage) : EventArgs
{
    public readonly StorageService Storage = storage;
    public IImageFile File { get; private set; } = file;
    public bool IsCanceled { get; private set; } = false;

    public void UpdateFile(IImageFile file) => File = file;
    public void Cancel() => IsCanceled = true;
}

