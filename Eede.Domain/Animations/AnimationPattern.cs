using System;
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
        if (frames == null) throw new ArgumentNullException(nameof(frames));
        Name = name;
        Frames = frames.ToList();
        Grid = grid;
    }

    public AnimationPattern(string name, IEnumerable<AnimationFrame> frames, GridSettings grid)
        : this(name, frames?.ToList() ?? throw new ArgumentNullException(nameof(frames)), grid)
    {
    }

    public AnimationPattern AddFrame(AnimationFrame frame)
    {
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        var newFrames = new List<AnimationFrame>(Frames);
        newFrames.Add(frame);
        return new AnimationPattern(Name, newFrames, Grid);
    }

    public AnimationPattern RemoveFrame(int index)
    {
        if (index < 0 || index >= Frames.Count) throw new ArgumentOutOfRangeException(nameof(index));
        var newFrames = new List<AnimationFrame>(Frames);
        newFrames.RemoveAt(index);
        return new AnimationPattern(Name, newFrames, Grid);
    }

    public AnimationPattern MoveFrame(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= Frames.Count) throw new ArgumentOutOfRangeException(nameof(fromIndex));
        if (toIndex < 0 || toIndex >= Frames.Count) throw new ArgumentOutOfRangeException(nameof(toIndex));

        var newFrames = new List<AnimationFrame>(Frames);
        var item = newFrames[fromIndex];
        newFrames.RemoveAt(fromIndex);
        newFrames.Insert(toIndex, item);
        return new AnimationPattern(Name, newFrames, Grid);
    }

    public AnimationPattern UpdateFrame(int index, AnimationFrame frame)
    {
        if (index < 0 || index >= Frames.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        var newFrames = new List<AnimationFrame>(Frames);
        newFrames[index] = frame;
        return new AnimationPattern(Name, newFrames, Grid);
    }
}
