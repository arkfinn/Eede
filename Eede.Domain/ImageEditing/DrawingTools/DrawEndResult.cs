using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.DrawingTools;

public sealed record DrawEndResult(DrawingBuffer Buffer, PictureArea? AffectedArea);
