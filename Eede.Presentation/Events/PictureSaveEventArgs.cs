using Eede.Presentation.Files;
using System;

namespace Eede.Presentation.Events;

public class PictureSaveEventArgs(IImageFile file, bool isSaveAs = false) : EventArgs
{
    public IImageFile File { get; private set; } = file;
    public bool IsCanceled { get; private set; } = false;
    public bool IsSaveAs { get; } = isSaveAs;

    public void UpdateFile(IImageFile file) => File = file;
    public void Cancel() => IsCanceled = true;
}