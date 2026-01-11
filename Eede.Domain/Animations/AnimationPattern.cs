using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Eede.Domain.Animations;

public class AnimationPattern
{
    public string Name { get; }
    public IReadOnlyList<AnimationFrame> Frames { get; }

    [JsonConstructor]
    public AnimationPattern(string name, IReadOnlyList<AnimationFrame> frames)
    {
        Name = name;
        Frames = frames.ToList();
    }

    public AnimationPattern(string name, IEnumerable<AnimationFrame> frames)
        : this(name, frames.ToList())
    {
    }
}
