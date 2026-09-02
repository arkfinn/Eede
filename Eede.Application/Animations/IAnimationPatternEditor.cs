using Eede.Domain.Animations;

namespace Eede.Application.Animations;

public interface IAnimationPatternEditor
{
    void Add(AnimationPattern pattern);
    void Replace(int index, AnimationPattern pattern);
    void Remove(int index);
}
