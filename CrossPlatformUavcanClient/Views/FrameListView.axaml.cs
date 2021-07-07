using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CrossPlatformUavcanClient.Views
{
    public partial class FrameListView : UserControl
    {
        public FrameListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
