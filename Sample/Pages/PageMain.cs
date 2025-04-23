using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Design;
using Going.UI.ImageCanvas;

namespace test.Pages
{
    public partial class PageMain : GoPage
    {
        public PageMain()
        {
            InitializeComponent();

            btnTestWindow.ButtonClicked += (o, s) => Design.ShowWindow(MainWindow.Current.TestWindow);
        }
    }
}
