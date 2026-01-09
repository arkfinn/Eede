using Eede.Domain.ImageEditing;
using System;

namespace Eede.Application.Pictures;

public class PictureUpdateEventArgs(Picture updated) : EventArgs
{
    public readonly Picture Updated = updated ?? throw new ArgumentNullException(nameof(updated));
}
