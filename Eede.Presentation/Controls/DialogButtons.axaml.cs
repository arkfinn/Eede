using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Windows.Input;

namespace Eede.Presentation.Controls
{
    public partial class DialogButtons : UserControl
    {
        public static readonly StyledProperty<string> PrimaryTextProperty =
            AvaloniaProperty.Register<DialogButtons, string>(nameof(PrimaryText), defaultValue: "OK");

        public string PrimaryText
        {
            get => GetValue(PrimaryTextProperty);
            set => SetValue(PrimaryTextProperty, value);
        }

        public static readonly StyledProperty<ICommand> PrimaryCommandProperty =
            AvaloniaProperty.Register<DialogButtons, ICommand>(nameof(PrimaryCommand));

        public ICommand PrimaryCommand
        {
            get => GetValue(PrimaryCommandProperty);
            set => SetValue(PrimaryCommandProperty, value);
        }

        public static readonly StyledProperty<bool> IsPrimaryVisibleProperty =
            AvaloniaProperty.Register<DialogButtons, bool>(nameof(IsPrimaryVisible), defaultValue: true);

        public bool IsPrimaryVisible
        {
            get => GetValue(IsPrimaryVisibleProperty);
            set => SetValue(IsPrimaryVisibleProperty, value);
        }

        public static readonly StyledProperty<string> SecondaryTextProperty =
            AvaloniaProperty.Register<DialogButtons, string>(nameof(SecondaryText));

        public string SecondaryText
        {
            get => GetValue(SecondaryTextProperty);
            set => SetValue(SecondaryTextProperty, value);
        }

        public static readonly StyledProperty<ICommand> SecondaryCommandProperty =
            AvaloniaProperty.Register<DialogButtons, ICommand>(nameof(SecondaryCommand));

        public ICommand SecondaryCommand
        {
            get => GetValue(SecondaryCommandProperty);
            set => SetValue(SecondaryCommandProperty, value);
        }

        public static readonly StyledProperty<bool> IsSecondaryVisibleProperty =
            AvaloniaProperty.Register<DialogButtons, bool>(nameof(IsSecondaryVisible), defaultValue: false);

        public bool IsSecondaryVisible
        {
            get => GetValue(IsSecondaryVisibleProperty);
            set => SetValue(IsSecondaryVisibleProperty, value);
        }

        public static readonly StyledProperty<string> CancelTextProperty =
            AvaloniaProperty.Register<DialogButtons, string>(nameof(CancelText), defaultValue: "キャンセル");

        public string CancelText
        {
            get => GetValue(CancelTextProperty);
            set => SetValue(CancelTextProperty, value);
        }

        public static readonly StyledProperty<ICommand> CancelCommandProperty =
            AvaloniaProperty.Register<DialogButtons, ICommand>(nameof(CancelCommand));

        public ICommand CancelCommand
        {
            get => GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        public static readonly StyledProperty<bool> IsCancelVisibleProperty =
            AvaloniaProperty.Register<DialogButtons, bool>(nameof(IsCancelVisible), defaultValue: true);

        public bool IsCancelVisible
        {
            get => GetValue(IsCancelVisibleProperty);
            set => SetValue(IsCancelVisibleProperty, value);
        }

        public DialogButtons()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
