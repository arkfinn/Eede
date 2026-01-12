using Eede.Domain.Animations;
using System.Collections.Generic;

namespace Eede.Application.Animations;

public interface IAnimationService
{
    IReadOnlyList<AnimationPattern> Patterns { get; }
    void Add(AnimationPattern pattern);
    void Replace(int index, AnimationPattern pattern);
    void Remove(int index);
}
