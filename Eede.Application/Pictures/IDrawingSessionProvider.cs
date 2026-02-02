using Eede.Domain.ImageEditing;
using System;

namespace Eede.Application.Pictures;

public interface IDrawingSessionProvider
{
    DrawingSession CurrentSession { get; }
    void Update(DrawingSession session);
    event Action<DrawingSession> SessionChanged;
}
