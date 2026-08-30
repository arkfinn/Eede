using Eede.Application.Drawings;
using Eede.Application.Pictures;
using Eede.Domain.ImageEditing;
using Eede.Domain.ImageEditing.Blending;
using Eede.Domain.ImageEditing.DrawingTools;
using Eede.Domain.Palettes;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Services;
using NUnit.Framework;
using System;
using System.Reactive;
using Avalonia.Headless.NUnit;
using ReactiveUI;

namespace Eede.Presentation.Tests.Services;

#nullable enable

[TestFixture]
public class InteractionCoordinatorTests
{
    private DrawingSessionProvider _sessionProvider = default!;
    private InteractionCoordinator _coordinator = default!;

    [SetUp]
    public void SetUp()
    {
        _sessionProvider = new DrawingSessionProvider();
        _coordinator = new InteractionCoordinator(_sessionProvider);
    }

    [AvaloniaTest]
    public void HandlePointerReleased_ShouldPushToSessionProvider()
    {
        var sessionProvider = new DrawingSessionProvider();
        var initialPicture = Picture.CreateEmpty(new PictureSize(16, 16));
        sessionProvider.Update(new DrawingSession(initialPicture));

        var coordinator = new InteractionCoordinator(sessionProvider);
        coordinator.ChangeDrawStyle(new Eede.Domain.ImageEditing.DrawingTools.FreeCurve());

        // Begin drawing
        var pos = new Position(5, 5);
        var penStyle = new Eede.Domain.ImageEditing.DrawingTools.PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), new Eede.Domain.Palettes.ArgbColor(255, 0, 0, 0), 1);

        coordinator.PointerBegin(pos, new DrawingBuffer(sessionProvider.CurrentSession.Buffer.Previous), new Eede.Domain.ImageEditing.DrawingTools.FreeCurve(), new Eede.Domain.ImageEditing.DrawingTools.PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), new Eede.Domain.Palettes.ArgbColor(255, 0, 0, 0), 1), false, false, new PictureSize(16, 16), null);

        // Coordinator internal state updates

        // Release pointer to end drawing
        coordinator.PointerLeftButtonReleased(pos, new DrawingBuffer(sessionProvider.CurrentSession.Buffer.Previous), new Eede.Domain.ImageEditing.DrawingTools.FreeCurve(), false, new PictureSize(16, 16), new Eede.Domain.ImageEditing.DrawingTools.PenStyle(new Eede.Domain.ImageEditing.Blending.DirectImageBlender(), new Eede.Domain.Palettes.ArgbColor(255, 0, 0, 0), 1), false, null);

        // Verify that history was pushed (UndoStack is not empty anymore because a CanvasHistoryItem was pushed)
        Assert.That(sessionProvider.CurrentSession.CanUndo(), Is.True);
    }
}
