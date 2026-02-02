using Eede.Domain.Animations;

namespace Eede.Application.UseCase.Animations;

public interface IReplaceAnimationPatternUseCase
{
    void Execute(int index, AnimationPattern pattern);
}
