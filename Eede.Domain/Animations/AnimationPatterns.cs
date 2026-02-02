using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Eede.Domain.Animations;

public class AnimationPatterns
{
    private readonly ImmutableList<AnimationPattern> _items;

    public IReadOnlyList<AnimationPattern> Items => _items;

    public AnimationPatterns() : this(ImmutableList<AnimationPattern>.Empty)
    {
    }

    private AnimationPatterns(ImmutableList<AnimationPattern> items)
    {
        _items = items;
    }

    public AnimationPatterns Add(AnimationPattern pattern)
    {
        if (pattern == null) throw new ArgumentNullException(nameof(pattern));
        return new AnimationPatterns(_items.Add(pattern));
    }

    public AnimationPatterns Replace(int index, AnimationPattern pattern)
    {
        if (index < 0 || index >= _items.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (pattern == null) throw new ArgumentNullException(nameof(pattern));
        return new AnimationPatterns(_items.SetItem(index, pattern));
    }

    public AnimationPatterns RemoveAt(int index)
    {
        if (index < 0 || index >= _items.Count) throw new ArgumentOutOfRangeException(nameof(index));
        return new AnimationPatterns(_items.RemoveAt(index));
    }
}
