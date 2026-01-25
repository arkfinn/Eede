using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Application.Pictures;

public class DrawingSessionProvider : IDrawingSessionProvider
{
    public DrawingSession CurrentSession { get; private set; }

    public event Action<DrawingSession>? SessionChanged;

    public DrawingSessionProvider()
    {
        CurrentSession = new DrawingSession(Picture.CreateEmpty(new PictureSize(32, 32)));
    }

    public void Update(DrawingSession session)
    {
        CurrentSession = session;
        SessionChanged?.Invoke(CurrentSession);
    }
}
