using Eede.Domain.ImageEditing;

namespace Eede.Domain.Selections;

public record SelectionContent
{
    public Picture Image { get; }
    public Selection OriginalSelection { get; }

    public SelectionContent(Picture image, Selection originalSelection)
    {
        Image = image;
        OriginalSelection = originalSelection;
    }
}
