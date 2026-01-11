using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Eede.Domain.Animations;

public class AnimationPattern
{
    public string Name { get; }
    public IReadOnlyList<AnimationFrame> Frames { get; }
    public GridSettings Grid { get; }

    [JsonConstructor]
    public AnimationPattern(string name, IReadOnlyList<AnimationFrame> frames, GridSettings grid)
    {
        Name = name;
        Frames = frames.ToList();
        Grid = grid;
    }

    public AnimationPattern(string name, IEnumerable<AnimationFrame> frames, GridSettings grid)
        : this(name, frames.ToList(), grid)
    {
    }
}
