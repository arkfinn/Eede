using Eede.Domain.Animations;
using System;

namespace Eede.Application.Animations;

public interface IAnimationPatternsProvider
{
    AnimationPatterns Current { get; }
    void Update(AnimationPatterns next);
    event Action<AnimationPatterns> Changed;
}

public class AnimationPatternsProvider : IAnimationPatternsProvider
{
    public AnimationPatterns Current { get; private set; } = new AnimationPatterns();

    public event Action<AnimationPatterns>? Changed;

    public void Update(AnimationPatterns next)
    {
        if (next == null) throw new ArgumentNullException(nameof(next));
        Current = next;
        Changed?.Invoke(Current);
    }
}
