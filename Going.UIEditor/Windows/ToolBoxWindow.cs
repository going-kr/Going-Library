using Going.UI.Datas;
using Going.UI.Enums;
using Going.UIEditor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UIEditor.Windows
{
    public partial class ToolBoxWindow : xWindow
    {
        #region Const
        #endregion

        #region Constructor
        public ToolBoxWindow()
        {
            InitializeComponent();
         
            TitleIconString = "fa-toolbox";
            Title = "ToolBox";

            #region ToolBox
            toolBox.Round = GoRoundType.Rect;
            toolBox.Categories.Add(new GoToolCategory { Text = LM.Controls });
            toolBox.Categories.Add(new GoToolCategory { Text = LM.Containers });
            #endregion

            #region LoadItem
            {
                var asm = Assembly.Load("Going.UI");

                var types = asm.GetTypes();
                var lsControls = types.Where(x => x.FullName?.StartsWith("Going.UI.Controls.") ?? false)
                                      .Where(x => FindType(x, typeof(Going.UI.Controls.GoControl)) && !x.IsAbstract).ToList();
                var lsContainers = types.Where(x => x.FullName?.StartsWith("Going.UI.Containers.") ?? false)
                                        .Where(x => FindType(x, typeof(Going.UI.Containers.GoContainer)) && !x.IsAbstract).ToList();

                toolBox.Categories[0].Items.AddRange(lsControls.Select(x => new GoToolItem { Text = x.IsGenericType ? x.Name.Split('`').FirstOrDefault() + "<T>" : x.Name, Tag = x }));
                toolBox.Categories[1].Items.AddRange(lsContainers.Select(x => new GoToolItem { Text = x.IsGenericType ? x.Name.Split('`').FirstOrDefault() + "<T>" : x.Name, Tag = x }));

                var s = "";
                s += string.Concat(lsControls.Select(x => (x.IsGenericType ? x.Name.Split('`').FirstOrDefault() + "<T>" : x.Name) + "\r\n"));
                s += "\r\n";
                s += string.Concat(lsContainers.Select(x => (x.IsGenericType ? x.Name.Split('`').FirstOrDefault() + "<T>" : x.Name) + "\r\n"));
            }
            #endregion

            #region Evenrt
            toolBox.DragStart += (o, s) => DoDragDrop(s.Item, DragDropEffects.All);
            #endregion
        }
        #endregion

        #region FindType
        bool FindType(Type? v, Type n)
        {
            if (v == n) return true;
            else if (v == typeof(object)) return false;
            else if (v != null) return FindType(v.BaseType, n);
            else return false;
        }
        #endregion
    }
}
