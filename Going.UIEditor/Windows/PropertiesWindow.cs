using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Forms.Controls;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using Going.UIEditor.Controls;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UIEditor.Windows
{
    public partial class PropertiesWindow : xWindow
    {
        public GoPropertyGrid Grid => pg;

        #region Constructor
        public PropertiesWindow()
        {
            InitializeComponent();

            TitleIconString = "fa-list";
            Title = "Properties";
        }
        #endregion
        
        public void SelectObjects(IEnumerable<IGoControl>? objects)
        {
            pg.SelectedObjects = [.. objects];
        }
    }
     
}
