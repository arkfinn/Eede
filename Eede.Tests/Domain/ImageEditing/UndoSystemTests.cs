using Eede.Domain.ImageEditing;
using NUnit.Framework;

namespace Eede.Domain.Tests.ImageEditing
{
    [TestFixture()]
    public class UndoSystemTests
    {
        [Test()]
        public void UndoTest()
        {
            UndoSystem undo = new();
            TestUndoItem item = new();

            undo = undo.Add(new TestUndoItem());
            Assert.That(undo.CanUndo(), Is.EqualTo(true));

            undo = undo.Add(item);
            undo = undo.Undo();
            Assert.That(item.Undone, Is.EqualTo(true));
            Assert.That(undo.CanRedo(), Is.EqualTo(true));

            undo = undo.Redo();
            Assert.That(item.Redone, Is.EqualTo(true));
            Assert.That(undo.CanUndo(), Is.EqualTo(true));
            Assert.That(undo.CanRedo(), Is.EqualTo(false));

            undo = undo.Undo();
            undo = undo.Undo();
            Assert.That(undo.CanUndo(), Is.EqualTo(false));
        }

        [Test()]
        public void AddTest()
        {
            UndoSystem undo = new();
            TestUndoItem item = new();

            undo = undo.Add(item);
            undo = undo.Undo();
            undo = undo.Add(new TestUndoItem());

            Assert.That(undo.CanUndo(), Is.EqualTo(true));
            Assert.That(undo.CanRedo(), Is.EqualTo(false));
        }

        private sealed class TestUndoItem : IUndoItem
        {
            public bool Undone = false;
            public bool Redone = false;

            public void Redo()
            {
                Redone = true;
            }

            public void Undo()
            {
                Undone = true;
            }
        }
    }
}