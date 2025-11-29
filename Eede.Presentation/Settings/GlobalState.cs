using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;

namespace Eede.Presentation.Settings
{
    public class GlobalState
    {
        public HalfBoxArea CursorArea { get; set; }

        public GlobalState()
        {
            CursorArea = HalfBoxArea.Create(new Position(0, 0), new PictureSize(32, 32));
        }
    }
}
