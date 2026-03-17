using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.OpenTK.Windows;
using OpenTK.Windowing.Common;
using SenvasSample.Pages;
using SenvasSample.Windows;

namespace SenvasSample
{
    public partial class MainWindow : GoViewWindow
    {
        public MainWindow() : base(1024, 600, WindowBorder.Hidden)
        {
            InitializeComponent();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }
    }
}
