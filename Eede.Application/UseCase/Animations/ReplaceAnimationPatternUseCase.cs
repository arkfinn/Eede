using Eede.Application.Animations;
using Eede.Domain.Animations;
using System;

namespace Eede.Application.UseCase.Animations;

public class ReplaceAnimationPatternUseCase : IReplaceAnimationPatternUseCase
{
    private readonly IAnimationPatternsProvider _provider;

    public ReplaceAnimationPatternUseCase(IAnimationPatternsProvider provider)
    {
        _provider = provider;
    }

    public void Execute(int index, AnimationPattern pattern)
    {
        _provider.Update(_provider.Current.Replace(index, pattern));
    }
}
