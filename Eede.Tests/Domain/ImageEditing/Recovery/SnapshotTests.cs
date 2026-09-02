#nullable enable
using System;
using System.Collections.Generic;
using Eede.Domain.ImageEditing.Recovery;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using NUnit.Framework;

namespace Eede.Tests.Domain.ImageEditing.Recovery;

[TestFixture]
public class SnapshotTests
{
    [Test]
    public void DocumentSnapshot_Initialization_SetsPropertiesCorrectly()
    {
        var size = new PictureSize(32, 32);
        var snapshot = new DocumentSnapshot(
            documentId: "doc-1",
            originalFilePath: "C:/test/sample.png",
            isEdited: true,
            size: size,
            magnification: 2.0f,
            imagePayloadRef: "blob-key-1");

        Assert.That(snapshot.DocumentId, Is.EqualTo("doc-1"));
        Assert.That(snapshot.OriginalFilePath, Is.EqualTo("C:/test/sample.png"));
        Assert.That(snapshot.IsEdited, Is.True);
        Assert.That(snapshot.Size, Is.EqualTo(size));
        Assert.That(snapshot.Magnification, Is.EqualTo(2.0f));
        Assert.That(snapshot.ImagePayloadRef, Is.EqualTo("blob-key-1"));
    }

    [Test]
    public void DocumentSnapshot_AllowsNullOriginalFilePathAndImagePayloadRef()
    {
        var snapshot = new DocumentSnapshot(
            documentId: "doc-2",
            originalFilePath: null,
            isEdited: false,
            size: new PictureSize(16, 16),
            magnification: 1.0f,
            imagePayloadRef: null);

        Assert.That(snapshot.OriginalFilePath, Is.Null);
        Assert.That(snapshot.ImagePayloadRef, Is.Null);
    }

    [Test]
    public void DocumentSnapshot_ThrowsOnNullDocumentId()
    {
        Assert.Throws<ArgumentNullException>(() => new DocumentSnapshot(
            documentId: null!,
            originalFilePath: null,
            isEdited: false,
            size: new PictureSize(16, 16),
            magnification: 1.0f,
            imagePayloadRef: null));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void DocumentSnapshot_ThrowsOnWhitespaceDocumentId(string invalidId)
    {
        Assert.Throws<ArgumentException>(() => new DocumentSnapshot(
            documentId: invalidId,
            originalFilePath: null,
            isEdited: false,
            size: new PictureSize(16, 16),
            magnification: 1.0f,
            imagePayloadRef: null));
    }

    [TestCase(0f)]
    [TestCase(-1.0f)]
    public void DocumentSnapshot_ThrowsOnNonPositiveMagnification(float invalidMag)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentSnapshot(
            documentId: "doc-1",
            originalFilePath: null,
            isEdited: false,
            size: new PictureSize(16, 16),
            magnification: invalidMag,
            imagePayloadRef: null));
    }

    [Test]
    public void DocumentSnapshot_WithExpression_ReturnsModifiedCopy()
    {
        var original = new DocumentSnapshot(
            documentId: "doc-1",
            originalFilePath: null,
            isEdited: false,
            size: new PictureSize(16, 16),
            magnification: 1.0f,
            imagePayloadRef: null);

        var modified = original with { IsEdited = true, Magnification = 4.0f };

        Assert.That(original.IsEdited, Is.False);
        Assert.That(original.Magnification, Is.EqualTo(1.0f));
        Assert.That(modified.IsEdited, Is.True);
        Assert.That(modified.Magnification, Is.EqualTo(4.0f));
        Assert.That(modified.DocumentId, Is.EqualTo(original.DocumentId));
    }

    [Test]
    public void PullSnapshot_Initialization_SetsPropertiesCorrectly()
    {
        var area = new PictureArea(new Position(10, 20), new PictureSize(30, 40));
        var snapshot = new PullSnapshot(
            sourceDocumentId: "doc-source",
            sourceArea: area,
            hasUnpushedChanges: true,
            canvasImagePayloadRef: "canvas-blob-1");

        Assert.That(snapshot.SourceDocumentId, Is.EqualTo("doc-source"));
        Assert.That(snapshot.SourceArea, Is.EqualTo(area));
        Assert.That(snapshot.HasUnpushedChanges, Is.True);
        Assert.That(snapshot.CanvasImagePayloadRef, Is.EqualTo("canvas-blob-1"));
    }

    [Test]
    public void PullSnapshot_AllowsNullCanvasImagePayloadRef()
    {
        var area = new PictureArea(new Position(0, 0), new PictureSize(16, 16));
        var snapshot = new PullSnapshot(
            sourceDocumentId: "doc-1",
            sourceArea: area,
            hasUnpushedChanges: false,
            canvasImagePayloadRef: null);

        Assert.That(snapshot.CanvasImagePayloadRef, Is.Null);
    }

    [Test]
    public void PullSnapshot_ThrowsOnNullSourceDocumentId()
    {
        var area = new PictureArea(new Position(0, 0), new PictureSize(16, 16));
        Assert.Throws<ArgumentNullException>(() => new PullSnapshot(
            sourceDocumentId: null!,
            sourceArea: area,
            hasUnpushedChanges: false,
            canvasImagePayloadRef: null));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void PullSnapshot_ThrowsOnWhitespaceSourceDocumentId(string invalidId)
    {
        var area = new PictureArea(new Position(0, 0), new PictureSize(16, 16));
        Assert.Throws<ArgumentException>(() => new PullSnapshot(
            sourceDocumentId: invalidId,
            sourceArea: area,
            hasUnpushedChanges: false,
            canvasImagePayloadRef: null));
    }

    [Test]
    public void PullSnapshot_WithExpression_ReturnsModifiedCopy()
    {
        var area = new PictureArea(new Position(0, 0), new PictureSize(16, 16));
        var original = new PullSnapshot("doc-1", area, false, null);
        var modified = original with { HasUnpushedChanges = true };

        Assert.That(original.HasUnpushedChanges, Is.False);
        Assert.That(modified.HasUnpushedChanges, Is.True);
    }

    [Test]
    public void PaletteSnapshot_Initialization_SetsPropertiesCorrectly()
    {
        var selected = new ArgbColor(255, 10, 20, 30);
        var colors = new List<ArgbColor> { selected, new(255, 100, 100, 100) };

        var snapshot = new PaletteSnapshot(
            selectedColor: selected,
            activeTabIndex: 2,
            paletteColors: colors);

        Assert.That(snapshot.SelectedColor, Is.EqualTo(selected));
        Assert.That(snapshot.ActiveTabIndex, Is.EqualTo(2));
        Assert.That(snapshot.PaletteColors, Has.Count.EqualTo(2));
        Assert.That(snapshot.PaletteColors[0], Is.EqualTo(selected));
    }

    [Test]
    public void PaletteSnapshot_ColorsAreDefensivelyCopied()
    {
        var list = new List<ArgbColor> { new(255, 0, 0, 0) };
        var snapshot = new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, list);

        list.Add(new ArgbColor(255, 255, 255, 255));

        Assert.That(snapshot.PaletteColors, Has.Count.EqualTo(1));
    }

    [Test]
    public void PaletteSnapshot_ThrowsOnNullPaletteColors()
    {
        Assert.Throws<ArgumentNullException>(() => new PaletteSnapshot(
            selectedColor: new ArgbColor(255, 0, 0, 0),
            activeTabIndex: 0,
            paletteColors: null!));
    }

    [Test]
    public void PaletteSnapshot_ThrowsOnNegativeActiveTabIndex()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PaletteSnapshot(
            selectedColor: new ArgbColor(255, 0, 0, 0),
            activeTabIndex: -1,
            paletteColors: []));
    }

    [Test]
    public void SessionSnapshot_Initialization_SetsPropertiesCorrectly()
    {
        var sessionId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var doc = new DocumentSnapshot("doc-1", null, false, new PictureSize(16, 16), 1.0f, null);
        var palette = new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, []);
        var pull = new PullSnapshot("doc-1", new PictureArea(new Position(0, 0), new PictureSize(8, 8)), false, null);

        var snapshot = new SessionSnapshot(
            sessionId: sessionId,
            createdAt: now,
            activeDocumentId: "doc-1",
            documents: [doc],
            pullState: pull,
            paletteState: palette);

        Assert.That(snapshot.SessionId, Is.EqualTo(sessionId));
        Assert.That(snapshot.CreatedAt, Is.EqualTo(now));
        Assert.That(snapshot.ActiveDocumentId, Is.EqualTo("doc-1"));
        Assert.That(snapshot.Documents, Has.Count.EqualTo(1));
        Assert.That(snapshot.Documents[0], Is.EqualTo(doc));
        Assert.That(snapshot.PullState, Is.EqualTo(pull));
        Assert.That(snapshot.PaletteState, Is.EqualTo(palette));
    }

    [Test]
    public void SessionSnapshot_AllowsNullPullState()
    {
        var snapshot = new SessionSnapshot(
            sessionId: Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow,
            activeDocumentId: "doc-1",
            documents: [],
            pullState: null,
            paletteState: new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, []));

        Assert.That(snapshot.PullState, Is.Null);
    }

    [Test]
    public void SessionSnapshot_DocumentsAreDefensivelyCopied()
    {
        var doc = new DocumentSnapshot("doc-1", null, false, new PictureSize(16, 16), 1.0f, null);
        var list = new List<DocumentSnapshot> { doc };

        var snapshot = new SessionSnapshot(
            sessionId: Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow,
            activeDocumentId: "doc-1",
            documents: list,
            pullState: null,
            paletteState: new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, []));

        list.Add(new DocumentSnapshot("doc-2", null, false, new PictureSize(8, 8), 1.0f, null));

        Assert.That(snapshot.Documents, Has.Count.EqualTo(1));
    }

    [Test]
    public void SessionSnapshot_ThrowsOnEmptySessionId()
    {
        Assert.Throws<ArgumentException>(() => new SessionSnapshot(
            sessionId: Guid.Empty,
            createdAt: DateTimeOffset.UtcNow,
            activeDocumentId: "doc-1",
            documents: [],
            pullState: null,
            paletteState: new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, [])));
    }

    [Test]
    public void SessionSnapshot_ThrowsOnNullActiveDocumentId()
    {
        Assert.Throws<ArgumentNullException>(() => new SessionSnapshot(
            sessionId: Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow,
            activeDocumentId: null!,
            documents: [],
            pullState: null,
            paletteState: new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, [])));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void SessionSnapshot_ThrowsOnWhitespaceActiveDocumentId(string invalidId)
    {
        Assert.Throws<ArgumentException>(() => new SessionSnapshot(
            sessionId: Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow,
            activeDocumentId: invalidId,
            documents: [],
            pullState: null,
            paletteState: new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, [])));
    }

    [Test]
    public void SessionSnapshot_ThrowsOnNullDocuments()
    {
        Assert.Throws<ArgumentNullException>(() => new SessionSnapshot(
            sessionId: Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow,
            activeDocumentId: "doc-1",
            documents: null!,
            pullState: null,
            paletteState: new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, [])));
    }

    [Test]
    public void SessionSnapshot_ThrowsOnNullElementInDocuments()
    {
        Assert.Throws<ArgumentException>(() => new SessionSnapshot(
            sessionId: Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow,
            activeDocumentId: "doc-1",
            documents: [null!],
            pullState: null,
            paletteState: new PaletteSnapshot(new ArgbColor(255, 0, 0, 0), 0, [])));
    }

    [Test]
    public void SessionSnapshot_ThrowsOnNullPaletteState()
    {
        Assert.Throws<ArgumentNullException>(() => new SessionSnapshot(
            sessionId: Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow,
            activeDocumentId: "doc-1",
            documents: [],
            pullState: null,
            paletteState: null!));
    }
}
