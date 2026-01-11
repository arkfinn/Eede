using Eede.Domain.SharedKernel;

namespace Eede.Domain.Animations;

public record GridSettings(PictureSize CellSize, Position Offset, int Padding);
