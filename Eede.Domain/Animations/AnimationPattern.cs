using System.Collections.Generic;
using System.Linq;

namespace Eede.Domain.Animations;

public class AnimationPattern
{
    public string Name { get; }
    public IReadOnlyList<AnimationFrame> Frames { get; }

    public AnimationPattern(string name, IEnumerable<AnimationFrame> frames)
    {
        Name = name;
        Frames = frames.ToList().AsReadOnly();
    }
}
