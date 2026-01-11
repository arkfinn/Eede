using Eede.Domain.Animations;
using System;
using System.Collections.Generic;

namespace Eede.Application.Animations;

public class AnimationService : IAnimationService
{
    private readonly List<AnimationPattern> patterns = new();
    public IReadOnlyList<AnimationPattern> Patterns => patterns;

    public void Add(AnimationPattern pattern)
    {
        if (pattern == null) throw new ArgumentNullException(nameof(pattern));
        patterns.Add(pattern);
    }

    public void Replace(int index, AnimationPattern pattern)
    {
        if (index < 0 || index >= patterns.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (pattern == null) throw new ArgumentNullException(nameof(pattern));
        patterns[index] = pattern;
    }

    public void Remove(int index)
    {
        if (index < 0 || index >= patterns.Count) throw new ArgumentOutOfRangeException(nameof(index));
        patterns.RemoveAt(index);
    }
}
