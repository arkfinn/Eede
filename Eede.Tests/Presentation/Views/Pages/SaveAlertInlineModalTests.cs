using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Eede.Application.Infrastructure;
using Eede.Domain.Palettes;
using Eede.Presentation.Common.Enums;
using Eede.Presentation.ViewModels.DataEntry;
using Eede.Presentation.ViewModels.Pages;
using Eede.Presentation.Views.DataDisplay;
using Eede.Presentation.Views.DataEntry;
using Eede.Presentation.Views.Pages;
using Eede.Views.Pages;
using Moq;
using NUnit.Framework;
using ReactiveUI;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using RxVoid = ReactiveUI.Primitives.RxVoid;

namespace Eede.Presentation.Tests.Views.Pages;

#nullable enable

[TestFixture]
public class SaveAlertInlineModalTests
{
    [AvaloniaTest]
    [TestCase(SaveAlertResult.Save)]
    [TestCase(SaveAlertResult.NoSave)]
    [TestCase(SaveAlertResult.Cancel)]
    public void SaveAlertViewModel_Commands_ShouldSetResultAndInvokeClose(SaveAlertResult expected)
    {
        SaveAlertResult? closedWith = null;
        var vm = new SaveAlertViewModel("hero.png", r => closedWith = r);
        var view = new SaveAlertContentView { DataContext = vm };
        var host = new Window { Content = view };
        host.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            Assert.That(vm.FileName, Is.EqualTo("hero.png"));
            (expected switch
            {
                SaveAlertResult.Save => vm.SaveCommand,
                SaveAlertResult.NoSave => vm.NoSaveCommand,
                _ => vm.CancelCommand
            }).Execute(null);

            Assert.Multiple(() =>
            {
                Assert.That(vm.Result, Is.EqualTo(expected));
                Assert.That(closedWith, Is.EqualTo(expected));
            });
        }
        finally
        {
            host.Close();
        }
    }

    [AvaloniaTest]
    [TestCase(SaveAlertResult.Save)]
    [TestCase(SaveAlertResult.NoSave)]
    [TestCase(SaveAlertResult.Cancel)]
    public async Task PaletteContainer_InInlineModalHost_ShouldResolveConfirmCloseWithoutDeadlock(SaveAlertResult choice)
    {
        var vm = new PaletteContainerViewModel(new Mock<IPaletteRepository>().Object, new Mock<IPaletteSessionRepository>().Object);
        vm.OpenImportedPalette(Palette.Create(), "test.png", "src_test");
        var dirtyTab = vm.Tabs[1];
        dirtyTab.Palette = dirtyTab.Palette.Apply(0, new ArgbColor(255, 255, 0, 0));
        dirtyTab.FilePath = @"C:\test.act";

        _ = new PaletteContainer { DataContext = vm };
        SaveAlertContentView? shownContent = null;

        var prevHandler = MainView.InlineSaveAlertHandler;
        MainView.InlineSaveAlertHandler = fileName =>
        {
            var tcs = new TaskCompletionSource<SaveAlertResult>();
            var alertVm = new SaveAlertViewModel(fileName, res => tcs.TrySetResult(res));
            shownContent = new SaveAlertContentView { DataContext = alertVm };
            Dispatcher.UIThread.Post(() => (choice switch
            {
                SaveAlertResult.Save => alertVm.SaveCommand,
                SaveAlertResult.NoSave => alertVm.NoSaveCommand,
                _ => alertVm.CancelCommand
            }).Execute(null));
            return tcs.Task;
        };

        try
        {
            var closeTask = vm.CloseTabCommand.Execute(dirtyTab).ToTask();
            Assert.That(await Task.WhenAny(closeTask, Task.Delay(2000)), Is.SameAs(closeTask));
            await closeTask;

            Assert.Multiple(() =>
            {
                Assert.That(shownContent, Is.Not.Null);
                Assert.That(vm.Tabs.Contains(dirtyTab), Is.EqualTo(choice == SaveAlertResult.Cancel));
            });
        }
        finally
        {
            MainView.InlineSaveAlertHandler = prevHandler;
        }
    }

    [AvaloniaTest]
    [TestCase(SaveAlertResult.Save, true)]
    [TestCase(SaveAlertResult.NoSave, true)]
    [TestCase(SaveAlertResult.Cancel, false)]
    public async Task PictureDocument_OpenSaveAlertDialog_InInlineModalHost_ShouldCompleteFlow(SaveAlertResult choice, bool expectedClosed)
    {
        bool closingExecuted = false;
        bool closeActionCalled = false;
        var doc = new PictureDocument
        {
            Subject = "sprite.png",
            Closable = false,
            CloseAction = () => closeActionCalled = true
        };
        doc.ClosingAction = ReactiveCommand.CreateFromTask<RxVoid, bool>(async _ =>
        {
            await Task.Yield();
            closingExecuted = true;
            return doc.Closable = doc.SaveAlertResult != SaveAlertResult.Cancel;
        });

        var factory = new Factory();
        var dock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<Dock.Model.Core.IDockable>(doc),
            ActiveDockable = doc
        };
        doc.Owner = dock;
        doc.Factory = factory;
        factory.InitLayout(dock);

        var prevHandler = MainView.InlineSaveAlertHandler;
        MainView.InlineSaveAlertHandler = fileName =>
        {
            Assert.That(fileName, Is.EqualTo("sprite.png"));
            return Task.FromResult(choice);
        };

        try
        {
            var task = doc.OpenSaveAlertDialog(doc);
            Assert.That(await Task.WhenAny(task, Task.Delay(2000)), Is.SameAs(task));
            await task;

            Assert.Multiple(() =>
            {
                Assert.That(doc.SaveAlertResult, Is.EqualTo(choice));
                Assert.That(closingExecuted, Is.True);
                Assert.That(doc.Closable, Is.EqualTo(expectedClosed));
                Assert.That(dock.VisibleDockables!.Contains(doc), Is.EqualTo(!expectedClosed));
                Assert.That(closeActionCalled, Is.EqualTo(expectedClosed));
            });
        }
        finally
        {
            MainView.InlineSaveAlertHandler = prevHandler;
        }
    }
}
