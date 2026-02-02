using Eede.Domain.ImageEditing;
using Eede.Domain.SharedKernel;

namespace Eede.Application.UseCase.Pictures;

public class TransferImageToCanvasUseCase : ITransferImageToCanvasUseCase
{
    public Picture Execute(Picture source, PictureArea rect)
    {
        return source.CutOut(rect);
    }
}
