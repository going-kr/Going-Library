using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;
using TspMonitor.Pages;
using TspMonitor.Windows;

namespace TspMonitor
{
    public partial class MainWindow : GoViewWindow
    {
        public MainWindow() : base(1024, 600, WindowBorder.Hidden)
        {
            InitializeComponent();
        }
    }
}
