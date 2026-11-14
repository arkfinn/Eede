using Eede.Application.Pictures;
using NUnit.Framework;

namespace Eede.Tests.Application.Pictures;

[TestFixture]
public class TitleTrimmerTests
{
    [Test]
    public void TruncateMiddle_ShortText_ReturnsOriginal()
    {
        string input = "walk.png";
        string result = TitleTrimmer.TruncateMiddle(input, 18);
        Assert.That(result, Is.EqualTo("walk.png"));
    }

    [Test]
    public void TruncateMiddle_ExactLength_ReturnsOriginal()
    {
        string input = "123456789012345678";
        string result = TitleTrimmer.TruncateMiddle(input, 18);
        Assert.That(result, Is.EqualTo("123456789012345678"));
    }

    [Test]
    public void TruncateMiddle_MaxLengthAtAndUnderSix_ReturnsOriginal()
    {
        string input = "long_filename.png";
        Assert.That(TitleTrimmer.TruncateMiddle(input, 6), Is.EqualTo("long_filename.png"));
        Assert.That(TitleTrimmer.TruncateMiddle(input, 5), Is.EqualTo("long_filename.png"));
    }

    [Test]
    public void TruncateMiddle_LongFilenameWithExtension_PreservesHeadTailAndExtension()
    {
        // 28 chars, max: 18, ext: .png (4 chars)
        // available = 18 - 3 - 4 = 11 -> head = 6, tail = 5
        // nameWithoutExt = "player_animation_walk_01" (24 chars)
        // head = "player", tail = "lk_01" -> "player...lk_01.png" (18 chars)
        string input = "player_animation_walk_01.png";
        string result = TitleTrimmer.TruncateMiddle(input, 18);

        Assert.That(result, Is.EqualTo("player...lk_01.png"));
        Assert.That(result.Length, Is.EqualTo(18));
    }

    [Test]
    public void TruncateMiddle_AvailableLengthOne_OmitsTailCleanly()
    {
        // maxLength = 8, maxWithoutEllipsis = 5, ext = ".png" (4) -> available = 1 -> head = 1, tail = 0
        string input = "abcdefghij.png";
        string result = TitleTrimmer.TruncateMiddle(input, 8);

        Assert.That(result, Is.EqualTo("a....png"));
        Assert.That(result.Length, Is.EqualTo(8));
    }

    [Test]
    public void TruncateMiddle_VeryLongExtension_FallsBackToTrailingEllipsis()
    {
        // maxLength = 10 -> maxWithoutEllipsis = 7
        // ext = ".123456" (len 7 == maxWithoutEllipsis) -> boundary triggers trailing "..."
        Assert.That(TitleTrimmer.TruncateMiddle("abcd.123456", 10), Is.EqualTo("abcd.12..."));
        Assert.That(TitleTrimmer.TruncateMiddle("ab.12345678", 10), Is.EqualTo("ab.1234..."));
    }

    [Test]
    public void TruncateMiddle_WithoutExtension_PreservesHeadAndTail()
    {
        string input = "VeryLongPaletteNameForTesting";
        string result = TitleTrimmer.TruncateMiddle(input, 15);

        Assert.That(result.Length, Is.EqualTo(15));
        Assert.That(result, Is.EqualTo("VeryLo...esting"));
    }

    [Test]
    public void TruncateMiddle_NullOrEmpty_ReturnsOriginal()
    {
        Assert.That(TitleTrimmer.TruncateMiddle("", 18), Is.EqualTo(""));
        Assert.That(TitleTrimmer.TruncateMiddle(null!, 18), Is.Null);
    }
}
