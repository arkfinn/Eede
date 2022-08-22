using Eede.Domain.Pictures;
using System;

namespace Eede.Application.Drawings
{
    public class DrawEventArgs : EventArgs
    {
        public readonly Picture PreviousPicture;
        public readonly Picture NowPicture;

        public DrawEventArgs(Picture previousPicture, Picture nowPicture)
        {
            PreviousPicture = previousPicture ?? throw new ArgumentNullException(nameof(previousPicture));
            NowPicture = nowPicture ?? throw new ArgumentNullException(nameof(nowPicture));
        }
    }
}
