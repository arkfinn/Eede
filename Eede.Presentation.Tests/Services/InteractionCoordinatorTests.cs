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

namespace Eede.Presentation.Tests.Services;

[TestFixture]
public class InteractionCoordinatorTests
{
    private DrawingSessionProvider _sessionProvider;
    private InteractionCoordinator _coordinator;

    [SetUp]
    public void SetUp()
    {
        _sessionProvider = new DrawingSessionProvider();
        _coordinator = new InteractionCoordinator(_sessionProvider);
    }

    [AvaloniaTest]
    public void PointerRightButtonPressed_ShouldPickColorWithAlpha()
    {
        // 1. アルファ値を含む画像データを作成
        var size = new PictureSize(16, 16);
        var expectedColor = new ArgbColor(128, 255, 0, 0); // 半透明の赤
        var imageData = new byte[size.Width * size.Height * 4];
        
        // (5, 5) の位置に色を書き込む (Bgra8888)
        int index = (5 * 4) + (size.Width * 4 * 5);
        imageData[index + 0] = expectedColor.Blue;
        imageData[index + 1] = expectedColor.Green;
        imageData[index + 2] = expectedColor.Red;
        imageData[index + 3] = expectedColor.Alpha;

        var picture = Picture.Create(size, imageData);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        // 2. 右クリック実行
        ArgbColor? pickedColor = null;
        var pos = new Position(5, 5);
        _coordinator.PointerRightButtonPressed(
            pos, 
            new DrawingBuffer(picture), 
            new FreeCurve(), 
            false, 
            new PictureSize(16, 16), 
            (color) => pickedColor = color, 
            null);

        // 3. 検証
        Assert.That(pickedColor, Is.Not.Null, "Color should be picked");
        Assert.That(pickedColor.Value.Alpha, Is.EqualTo(128), "Alpha value should be picked correctly");
        Assert.That(pickedColor.Value.Red, Is.EqualTo(255), "Red value should be picked correctly");
    }

    [AvaloniaTest]
    public void PointerRightButtonPressed_ShouldPickColorEvenWhenCancellingDraw()
    {
        // 描画中に右クリックした場合でも色が拾えることを確認
        var size = new PictureSize(16, 16);
        var expectedColor = new ArgbColor(200, 0, 255, 0); // 半透明の緑
        var imageData = new byte[size.Width * size.Height * 4];
        int index = (10 * 4) + (size.Width * 4 * 10);
        imageData[index + 0] = expectedColor.Blue;
        imageData[index + 1] = expectedColor.Green;
        imageData[index + 2] = expectedColor.Red;
        imageData[index + 3] = expectedColor.Alpha;

        var picture = Picture.Create(size, imageData);
        _sessionProvider.Update(new DrawingSession(picture));
        _coordinator.SyncWithSession();

        // 1. 描画開始 (左クリック相当)
        var buffer = new DrawingBuffer(picture);
        _coordinator.PointerBegin(new Position(0, 0), buffer, new FreeCurve(), new PenStyle(new DirectImageBlender()), false, false, new PictureSize(16, 16), null);
        
        // 2. 描画中に右クリック
        ArgbColor? pickedColor = null;
        _coordinator.PointerRightButtonPressed(
            new Position(10, 10), 
            _coordinator.CurrentBuffer, 
            new FreeCurve(), 
            false, 
            new PictureSize(16, 16), 
            (color) => pickedColor = color, 
            null);

        // 3. 検証
        Assert.That(pickedColor, Is.Not.Null, "Color should be picked even during drawing cancellation");
        Assert.That(pickedColor.Value.Alpha, Is.EqualTo(200));
        Assert.That(pickedColor.Value.Green, Is.EqualTo(255));
    }
}
