using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;
using System.Threading.Tasks;

namespace Eede.Application.UseCase.Pictures;

public class PasteFromClipboardUseCase
{
    private readonly IClipboard _clipboard;
    private readonly IDrawingSessionProvider _drawingSessionProvider;

    public PasteFromClipboardUseCase(IClipboard clipboard, IDrawingSessionProvider drawingSessionProvider)
    {
        _clipboard = clipboard;
        _drawingSessionProvider = drawingSessionProvider;
    }

    public virtual async Task ExecuteAsync()
    {
        var picture = await _clipboard.GetPictureAsync();
        if (picture == null) return;

        var position = _drawingSessionProvider.CurrentSession.CurrentSelectingArea?.Position ?? new Position(0, 0);
        var nextSession = _drawingSessionProvider.CurrentSession.PushPastePreview(picture, position);
        _drawingSessionProvider.Update(nextSession);
    }
}
