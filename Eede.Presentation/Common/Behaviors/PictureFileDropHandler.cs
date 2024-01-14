//using Avalonia.Input;
//using Avalonia.Xaml.Interactivity;
//using Eede.ViewModels.Pages;
//using Eede.Views.Pages;

//namespace Eede.Common.Behaviors
//{
//    public class PictureFileDropHandler : Behavior<MainView>
//    {
//        private MainView? Control;
//        protected override void OnAttached()
//        {
//            base.OnAttached();

//            if (AssociatedObject is not MainView control) return;

//            Control = control;
//            Control.AddHandler(DragDrop.DragOverEvent, DragOver);
//            Control.AddHandler(DragDrop.DropEvent, Drop);
//        }

//        protected override void OnDetaching()
//        {
//            base.OnDetaching();

//            if (Control is null) return;
//            Control.RemoveHandler(DragDrop.DropEvent, Drop);
//            Control.RemoveHandler(DragDrop.DragOverEvent, DragOver);
//        }

//        private void DragOver(object? sender, DragEventArgs e)
//        {
//            if (Control?.DataContext is not MainViewModel vm) return;
//            vm.DragOverPicture(e);
//        }


//        private void Drop(object? sender, DragEventArgs e)
//        {
//            if (Control?.DataContext is not MainViewModel vm) return;
//            vm.DropPicture(e);
//        }
//    }
//}
