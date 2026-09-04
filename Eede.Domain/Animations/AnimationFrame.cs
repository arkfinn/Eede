using System.Text.Json.Serialization;

namespace Eede.Domain.Animations;

public record AnimationFrame
{
    public int CellIndex { get; }
    public int Duration { get; }

    [JsonConstructor]
    public AnimationFrame(int cellIndex, int duration)
    {
        if (cellIndex < 0) throw new System.ArgumentOutOfRangeException(nameof(cellIndex), "CellIndex must be non-negative.");
        if (duration <= 0) throw new System.ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");

        CellIndex = cellIndex;
        Duration = duration;
    }

    public bool Validate() => CellIndex >= 0 && Duration > 0;
}
