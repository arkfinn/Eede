#nullable enable
namespace Eede.Domain.ImageEditing
{
    public class ImageCanvas
    {
        public object? Id { get; private set; }
        public object? Picture { get; private set; }
        public object? History { get; private set; }
        public bool IsDirty { get; private set; }
        public object? SourceFile { get; private set; }

        public ImageCanvas()
        {
            // 初期化: IsDirtyはfalse、他はnull（またはデフォルト値）
            Id = null;
            Picture = null;
            History = null;
            IsDirty = false;
            SourceFile = null;
        }
    }
}
