using Eede.Presentation.Common.Services;
using Eede.Presentation.Files;
using System;

namespace Eede.Presentation.Events;

public class PictureSaveEventArgs(BitmapFile file, StorageService storage) : EventArgs
{
    public readonly StorageService Storage = storage;
    public BitmapFile File { get; private set; } = file;

    public void UpdateFile(BitmapFile file) => File = file;
}
