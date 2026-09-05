using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Eede.Domain.Animations;

public class AnimationPattern
{
    private readonly ImmutableList<AnimationFrame> _frames;

    public string Name { get; }
    public IReadOnlyList<AnimationFrame> Frames => _frames;
    public GridSettings Grid { get; }

    [JsonConstructor]
    public AnimationPattern(string name, IReadOnlyList<AnimationFrame> frames, GridSettings grid)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters.", nameof(name));
        if (frames == null)
            throw new ArgumentNullException(nameof(frames));
        if (grid == null)
            throw new ArgumentNullException(nameof(grid));

        Name = name;

        if (frames is ImmutableList<AnimationFrame> immutableList)
        {
            _frames = immutableList;
        }
        else
        {
            _frames = frames.ToImmutableList();
        }

        Grid = grid;
    }

    public AnimationPattern(string name, IEnumerable<AnimationFrame> frames, GridSettings grid)
        : this(name, frames is ImmutableList<AnimationFrame> immutableList ? immutableList : (frames?.ToImmutableList() ?? throw new ArgumentNullException(nameof(frames))), grid)
    {
    }

    public AnimationPattern AddFrame(AnimationFrame frame)
    {
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        if (!frame.Validate()) throw new ArgumentException("Invalid animation frame.", nameof(frame));
        return new AnimationPattern(Name, _frames.Add(frame), Grid);
    }

    public AnimationPattern RemoveFrame(int index)
    {
        if (index < 0 || index >= _frames.Count) throw new ArgumentOutOfRangeException(nameof(index));
        return new AnimationPattern(Name, _frames.RemoveAt(index), Grid);
    }

    public AnimationPattern MoveFrame(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _frames.Count) throw new ArgumentOutOfRangeException(nameof(fromIndex));
        if (toIndex < 0 || toIndex >= _frames.Count) throw new ArgumentOutOfRangeException(nameof(toIndex));

        var item = _frames[fromIndex];
        var newFrames = _frames.RemoveAt(fromIndex).Insert(toIndex, item);
        return new AnimationPattern(Name, newFrames, Grid);
    }

    public AnimationPattern UpdateFrame(int index, AnimationFrame frame)
    {
        if (index < 0 || index >= _frames.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        if (!frame.Validate()) throw new ArgumentException("Invalid animation frame.", nameof(frame));
        return new AnimationPattern(Name, _frames.SetItem(index, frame), Grid);
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 100) return false;
        if (Grid == null || !Grid.Validate()) return false;
        if (Frames == null) return false;

        for (int i = 0; i < Frames.Count; i++)
        {
            var frame = Frames[i];
            if (frame == null || !frame.Validate()) return false;
        }

        return true;
    }
}
