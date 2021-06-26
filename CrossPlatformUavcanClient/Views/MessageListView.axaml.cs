using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CrossPlatformUavcanClient.Views
{
    public partial class MessageListView : UserControl
    {
        public MessageListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
