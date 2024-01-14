using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Windows.Input;

namespace Eede.Views.General.Buttons
{
    public partial class ToolButton : Avalonia.Controls.UserControl
    {
        public static readonly StyledProperty<string> SourceProperty =
            AvaloniaProperty.Register<ToolButton, string>(nameof(Source));

        public static readonly StyledProperty<ICommand?> CommandProperty =
            AvaloniaProperty.Register<ToolButton, ICommand?>(nameof(Command));

        public static readonly StyledProperty<object> CommandParameterProperty = 
            AvaloniaProperty.Register<ToolButton, object>(nameof(CommandParameter));

        public static readonly StyledProperty<bool> IsPressedProperty =
            AvaloniaProperty.Register<ToolButton, bool>(nameof(IsPressed));


        public ToolButton()
        {
            InitializeComponent();
        }

        public string Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public ICommand? Command
        {
            get { return GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public bool IsPressed
        {
            get { return GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }
    }
}
