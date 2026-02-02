using Eede.Domain.Animations;

namespace Eede.Application.UseCase.Animations;

public interface IAddAnimationPatternUseCase
{
    void Execute(AnimationPattern pattern);
}
