using Eede.Presentation.Common.Services;
using Eede.Presentation.Files;
using System;

namespace Eede.Presentation.Events;

public class PictureSaveEventArgs(BitmapFile file, StorageService storage) : EventArgs
{
    public readonly BitmapFile File = file;
    public readonly StorageService Storage = storage;
}
