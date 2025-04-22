using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;
using test.Pages;
using test.Windows;

namespace test
{
    public partial class MainWindow : GoViewWindow
    {
        public MainWindow() : base(800, 480, WindowBorder.Hidden)
        {
            InitializeComponent();
        }
    }
}
