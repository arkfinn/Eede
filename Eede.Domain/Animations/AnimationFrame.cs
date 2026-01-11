using System.Text.Json.Serialization;

namespace Eede.Domain.Animations;

public record AnimationFrame(int CellIndex, int Duration);
