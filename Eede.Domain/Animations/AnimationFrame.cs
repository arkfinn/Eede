using System.Text.Json.Serialization;

namespace Eede.Domain.Animations;

public record AnimationFrame(int CellIndex, int Duration)
{
    public bool Validate()
    {
        return CellIndex >= 0 && Duration > 0;
    }
}
