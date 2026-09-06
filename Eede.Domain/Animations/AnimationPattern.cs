using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace Eede.Domain.Animations;

public class AnimationPattern
{
    private readonly ImmutableArray<AnimationFrame> _frames;

    public string Name { get; }
    public IReadOnlyList<AnimationFrame> Frames => _frames.IsDefault ? ImmutableArray<AnimationFrame>.Empty : _frames;
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
        _frames = frames is ImmutableArray<AnimationFrame> immutableArray
            ? (immutableArray.IsDefault ? ImmutableArray<AnimationFrame>.Empty : immutableArray)
            : frames.ToImmutableArray();
        Grid = grid;
    }

    public AnimationPattern(string name, IEnumerable<AnimationFrame> frames, GridSettings grid)
        : this(name, (frames as IReadOnlyList<AnimationFrame>) ?? frames?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(frames)), grid)
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
        if (index < 0 || index >= _frames.Length) throw new ArgumentOutOfRangeException(nameof(index));
        return new AnimationPattern(Name, _frames.RemoveAt(index), Grid);
    }

    public AnimationPattern MoveFrame(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _frames.Length) throw new ArgumentOutOfRangeException(nameof(fromIndex));
        if (toIndex < 0 || toIndex >= _frames.Length) throw new ArgumentOutOfRangeException(nameof(toIndex));

        if (fromIndex == toIndex) return this;

        var item = _frames[fromIndex];
        var newFrames = _frames.RemoveAt(fromIndex).Insert(toIndex, item);
        return new AnimationPattern(Name, newFrames, Grid);
    }

    public AnimationPattern UpdateFrame(int index, AnimationFrame frame)
    {
        if (index < 0 || index >= _frames.Length) throw new ArgumentOutOfRangeException(nameof(index));
        if (frame == null) throw new ArgumentNullException(nameof(frame));
        if (!frame.Validate()) throw new ArgumentException("Invalid animation frame.", nameof(frame));
        return new AnimationPattern(Name, _frames.SetItem(index, frame), Grid);
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 100) return false;
        if (Grid == null || !Grid.Validate()) return false;
        if (_frames.IsDefault) return false;

        for (int i = 0; i < _frames.Length; i++)
        {
            var frame = _frames[i];
            if (frame == null || !frame.Validate()) return false;
        }

        return true;
    }
}
