using Eede.Application.UseCase.Animations;
using Eede.Domain.Animations;

namespace Eede.Application.Animations;

public class AnimationPatternEditor(
    IAddAnimationPatternUseCase addUseCase,
    IReplaceAnimationPatternUseCase replaceUseCase,
    IRemoveAnimationPatternUseCase removeUseCase) : IAnimationPatternEditor
{
    public void Add(AnimationPattern pattern) => addUseCase.Execute(pattern);
    public void Replace(int index, AnimationPattern pattern) => replaceUseCase.Execute(index, pattern);
    public void Remove(int index) => removeUseCase.Execute(index);
}
