using Eede.Domain.SharedKernel;

#nullable enable
namespace Eede.Domain.ImageEditing.DrawingTools;

public sealed record DrawEndResult(DrawingBuffer Buffer, PictureRegion AffectedArea);
