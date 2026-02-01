using Eede.Application.Animations;
using Eede.Domain.Animations;
using System;

namespace Eede.Application.UseCase.Animations;

public class AddAnimationPatternUseCase : IAddAnimationPatternUseCase
{
    private readonly IAnimationPatternsProvider _provider;

    public AddAnimationPatternUseCase(IAnimationPatternsProvider provider)
    {
        _provider = provider;
    }

    public void Execute(AnimationPattern pattern)
    {
        _provider.Update(_provider.Current.Add(pattern));
    }
}
