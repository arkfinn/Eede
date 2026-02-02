using Eede.Application.Animations;
using System;

namespace Eede.Application.UseCase.Animations;

public class RemoveAnimationPatternUseCase : IRemoveAnimationPatternUseCase
{
    private readonly IAnimationPatternsProvider _provider;

    public RemoveAnimationPatternUseCase(IAnimationPatternsProvider provider)
    {
        _provider = provider;
    }

    public void Execute(int index)
    {
        _provider.Update(_provider.Current.RemoveAt(index));
    }
}
