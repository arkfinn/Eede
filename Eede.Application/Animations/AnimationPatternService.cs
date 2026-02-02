using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;

namespace Eede.Application.Animations;

public interface IAnimationPatternService
{
    void Add(AnimationPattern pattern);
    void Replace(int index, AnimationPattern pattern);
    void Remove(int index);
}

public class AnimationPatternService(
    IAddAnimationPatternUseCase addUseCase,
    IReplaceAnimationPatternUseCase replaceUseCase,
    IRemoveAnimationPatternUseCase removeUseCase) : IAnimationPatternService
{
    public void Add(AnimationPattern pattern) => addUseCase.Execute(pattern);
    public void Replace(int index, AnimationPattern pattern) => replaceUseCase.Execute(index, pattern);
    public void Remove(int index) => removeUseCase.Execute(index);
}
