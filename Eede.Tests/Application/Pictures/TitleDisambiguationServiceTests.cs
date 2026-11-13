using Eede.Application.Pictures;
using NUnit.Framework;
using System.Collections.Generic;

namespace Eede.Tests.Application.Pictures;

[TestFixture]
public class TitleDisambiguationServiceTests
{
    private class TestItem : IDisambiguatableItem
    {
        public string FilePath { get; set; } = "";
        public string DisambiguationSuffix { get; set; } = "";

        public TestItem(string path)
        {
            FilePath = path;
        }
    }

    [Test]
    public void Recompute_NoDuplicates_SuffixesRemainEmpty()
    {
        var items = new List<TestItem>
        {
            new(@"C:\game\sprites\hero.png"),
            new(@"C:\game\sprites\enemy.png"),
            new(@"C:\game\backgrounds\forest.png")
        };

        TitleDisambiguationService.Recompute(items);

        Assert.That(items[0].DisambiguationSuffix, Is.EqualTo(""));
        Assert.That(items[1].DisambiguationSuffix, Is.EqualTo(""));
        Assert.That(items[2].DisambiguationSuffix, Is.EqualTo(""));
    }

    [Test]
    public void Recompute_DifferentParentFolders_AddsImmediateParentSuffix()
    {
        var items = new List<TestItem>
        {
            new(@"C:\game\player\walk.png"),
            new(@"C:\game\monster\walk.png")
        };

        TitleDisambiguationService.Recompute(items);

        Assert.That(items[0].DisambiguationSuffix, Is.EqualTo(" — player"));
        Assert.That(items[1].DisambiguationSuffix, Is.EqualTo(" — monster"));
    }

    [Test]
    public void Recompute_SameParentFolders_TraversesAncestorsUntilDistinct()
    {
        var items = new List<TestItem>
        {
            new(@"C:\projectA\assets\sprites\walk.png"),
            new(@"C:\projectB\assets\sprites\walk.png")
        };

        TitleDisambiguationService.Recompute(items);

        Assert.That(items[0].DisambiguationSuffix, Is.EqualTo(" — projectA/assets/sprites"));
        Assert.That(items[1].DisambiguationSuffix, Is.EqualTo(" — projectB/assets/sprites"));
    }

    [Test]
    public void Recompute_WhenDuplicateRemoved_RemainingItemSuffixIsCleared()
    {
        var itemA = new TestItem(@"C:\game\player\walk.png");
        var itemB = new TestItem(@"C:\game\monster\walk.png");
        var items = new List<TestItem> { itemA, itemB };

        TitleDisambiguationService.Recompute(items);
        Assert.That(itemA.DisambiguationSuffix, Is.EqualTo(" — player"));

        // itemB を削除して再計算
        items.Remove(itemB);
        TitleDisambiguationService.Recompute(items);

        Assert.That(itemA.DisambiguationSuffix, Is.EqualTo(""));
    }

    [Test]
    public void Recompute_WithEmptyPathNewFiles_DoesNotThrowAndSuffixIsEmpty()
    {
        var items = new List<TestItem>
        {
            new(""),
            new(""),
            new(@"C:\game\player\walk.png")
        };

        Assert.DoesNotThrow(() => TitleDisambiguationService.Recompute(items));
        Assert.That(items[0].DisambiguationSuffix, Is.EqualTo(""));
        Assert.That(items[1].DisambiguationSuffix, Is.EqualTo(""));
        Assert.That(items[2].DisambiguationSuffix, Is.EqualTo(""));
    }
}
