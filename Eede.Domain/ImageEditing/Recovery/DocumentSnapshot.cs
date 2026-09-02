#nullable enable
using System;
using Eede.Domain.SharedKernel;

namespace Eede.Domain.ImageEditing.Recovery;

public sealed record DocumentSnapshot
{
    public string DocumentId { get; init; }
    public string? OriginalFilePath { get; init; }
    public bool IsEdited { get; init; }
    public PictureSize Size { get; init; }
    public float Magnification { get; init; }
    public string? ImagePayloadRef { get; init; }

    public DocumentSnapshot(
        string documentId,
        string? originalFilePath,
        bool isEdited,
        PictureSize size,
        float magnification,
        string? imagePayloadRef)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        if (magnification <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(magnification), "Magnification must be greater than zero.");
        }

        DocumentId = documentId;
        OriginalFilePath = originalFilePath;
        IsEdited = isEdited;
        Size = size;
        Magnification = magnification;
        ImagePayloadRef = imagePayloadRef;
    }
}
