using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CrossPlatformUavcanClient.Views
{
    public partial class ReceivedMessagesListView : UserControl
    {
        public ReceivedMessagesListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
