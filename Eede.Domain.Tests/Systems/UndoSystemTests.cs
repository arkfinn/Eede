using NUnit.Framework;

namespace Eede.Domain.Systems.Tests
{
    [TestFixture()]
    public class UndoSystemTests
    {
        [Test()]
        public void UndoTest()
        {
            var undo = new UndoSystem();
            var item = new TestUndoItem();

            undo = undo.Add(new TestUndoItem());
            Assert.AreEqual(true, undo.CanUndo());

            undo = undo.Add(item);
            undo = undo.Undo();
            Assert.AreEqual(true, item.Undone);
            Assert.AreEqual(true, undo.CanRedo());

            undo = undo.Redo();
            Assert.AreEqual(true, item.Redone);
            Assert.AreEqual(true, undo.CanUndo());
            Assert.AreEqual(false, undo.CanRedo());

            undo = undo.Undo();
            undo = undo.Undo();
            Assert.AreEqual(false, undo.CanUndo());
        }

        [Test()]
        public void AddTest()
        {
            var undo = new UndoSystem();
            var item = new TestUndoItem();

            undo = undo.Add(item);
            undo = undo.Undo();
            undo = undo.Add(new TestUndoItem());

            Assert.AreEqual(true, item.Disposed);
            Assert.AreEqual(true, undo.CanUndo());
            Assert.AreEqual(false, undo.CanRedo());
        }

        [Test()]
        public void DosposeUndoListTest()
        {
            var undo = new UndoSystem();
            var item = new TestUndoItem();

            undo = undo.Add(item);
            undo.Dispose();
            Assert.AreEqual(true, item.Disposed);
        }

        [Test()]
        public void DosposeRedoListTest()
        {
            var undo = new UndoSystem();
            var item = new TestUndoItem();

            undo = undo.Add(item);
            undo = undo.Undo();
            undo.Dispose();
            Assert.AreEqual(true, item.Disposed);
        }

        private sealed class TestUndoItem : IUndoItem
        {
            public bool Disposed = false;
            public bool Undone = false;
            public bool Redone = false;

            public void Dispose()
            {
                Disposed = true;
            }

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