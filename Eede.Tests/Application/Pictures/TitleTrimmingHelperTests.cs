using Eede.Application.Pictures;
using NUnit.Framework;

namespace Eede.Tests.Application.Pictures;

[TestFixture]
public class TitleTrimmingHelperTests
{
    [Test]
    public void TruncateMiddle_ShortText_ReturnsOriginal()
    {
        string input = "walk.png";
        string result = TitleTrimmingHelper.TruncateMiddle(input, 18);
        Assert.That(result, Is.EqualTo("walk.png"));
    }

    [Test]
    public void TruncateMiddle_ExactLength_ReturnsOriginal()
    {
        string input = "123456789012345678";
        string result = TitleTrimmingHelper.TruncateMiddle(input, 18);
        Assert.That(result, Is.EqualTo("123456789012345678"));
    }

    [Test]
    public void TruncateMiddle_MaxLengthUnderSix_ReturnsOriginal()
    {
        string input = "long_filename.png";
        string result = TitleTrimmingHelper.TruncateMiddle(input, 5);
        Assert.That(result, Is.EqualTo("long_filename.png"));
    }

    [Test]
    public void TruncateMiddle_LongFilenameWithExtension_PreservesHeadTailAndExtension()
    {
        // 28 chars, max: 18, ext: .png (4 chars)
        // available = 18 - 3 - 4 = 11 -> head = 6, tail = 5
        // nameWithoutExt = "player_animation_walk_01" (24 chars)
        // head = "player", tail = "lk_01" -> "player...lk_01.png" (18 chars)
        string input = "player_animation_walk_01.png";
        string result = TitleTrimmingHelper.TruncateMiddle(input, 18);

        Assert.That(result.Length, Is.EqualTo(18));
        Assert.That(result.StartsWith("player..."), Is.True);
        Assert.That(result.EndsWith("lk_01.png"), Is.True);
    }

    [Test]
    public void TruncateMiddle_WithoutExtension_PreservesHeadAndTail()
    {
        string input = "VeryLongPaletteNameForTesting";
        string result = TitleTrimmingHelper.TruncateMiddle(input, 15);

        Assert.That(result.Length, Is.EqualTo(15));
        Assert.That(result.Contains("..."), Is.True);
        Assert.That(result.StartsWith("Very"), Is.True);
        Assert.That(result.EndsWith("ting"), Is.True);
    }

    [Test]
    public void TruncateMiddle_NullOrEmpty_ReturnsOriginal()
    {
        Assert.That(TitleTrimmingHelper.TruncateMiddle("", 18), Is.EqualTo(""));
        Assert.That(TitleTrimmingHelper.TruncateMiddle(null!, 18), Is.Null);
    }
}
