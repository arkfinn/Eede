using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Application.Pictures;

public class PicturePullEventArgs(Picture graphics, Position position) : EventArgs
{
    public readonly Picture Picture = graphics ?? throw new ArgumentNullException(nameof(graphics));
    public readonly Position Position = position;
}

