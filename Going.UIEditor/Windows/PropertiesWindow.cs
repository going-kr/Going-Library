using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UIEditor.Windows
{
    public partial class PropertiesWindow : xWindow
    {
        public PropertiesWindow()
        {
            InitializeComponent();

            TitleIconString = "fa-list";
            Title = "Properties";

        }
    }
}
