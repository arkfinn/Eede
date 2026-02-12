#nullable enable
namespace Eede.Domain.ImageEditing.DrawingTools;

public interface IDrawStyleFactory
{
    IDrawStyle Create(DrawStyleType type);
}
