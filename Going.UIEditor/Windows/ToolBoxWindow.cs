using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UIEditor.Utils;
using System;
using System.CodeDom;
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
                                      .Where(x => FindType(x, typeof(Going.UI.Controls.GoControl)) && !x.IsAbstract && !x.IsGenericType).ToList();
                var lsContainers = types.Where(x => x.FullName?.StartsWith("Going.UI.Containers.") ?? false)
                                        .Where(x => FindType(x, typeof(Going.UI.Containers.GoContainer)) && !x.IsAbstract && !x.IsGenericType).ToList();


                int ii = lsControls.IndexOf(typeof(GoInputDateTime)) + 1;
                lsControls.InsertRange(ii, 
                    [typeof(GoInputNumber<byte>), typeof(GoInputNumber<ushort>), typeof(GoInputNumber<uint>), typeof(GoInputNumber<ulong>),
                     typeof(GoInputNumber<sbyte>), typeof(GoInputNumber<short>), typeof(GoInputNumber<int>), typeof(GoInputNumber<long>),
                     typeof(GoInputNumber<float>), typeof(GoInputNumber<double>), typeof(GoInputNumber<decimal>)]);


                int iv = lsControls.IndexOf(typeof(GoValueBoolean)) + 1;
                lsControls.InsertRange(iv,
                    [typeof(GoValueNumber<byte>), typeof(GoValueNumber<ushort>), typeof(GoValueNumber<uint>), typeof(GoValueNumber<ulong>),
                     typeof(GoValueNumber<sbyte>), typeof(GoValueNumber<short>), typeof(GoValueNumber<int>), typeof(GoValueNumber<long>),
                     typeof(GoValueNumber<float>), typeof(GoValueNumber<double>), typeof(GoValueNumber<decimal>)]);

                //lsControls.Sort((x, y) => x.FullName != null ? x.FullName.CompareTo(y.FullName) : 0);


                toolBox.Categories[0].Items.AddRange(lsControls.Select(x => new GoToolItem { Text = x.IsGenericType ? x.Name.Split('`').FirstOrDefault() + $"<{NumberTypeName(x.GenericTypeArguments[0])}>" : x.Name, Tag = x }));
                toolBox.Categories[1].Items.AddRange(lsContainers.Select(x => new GoToolItem { Text = x.IsGenericType ? x.Name.Split('`').FirstOrDefault() + $"<{NumberTypeName(x.GenericTypeArguments[0])}>" : x.Name, Tag = x }));

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

        string NumberTypeName(Type t)
        {
            var s = "";
            if (t == typeof(byte)) s = "byte";
            else if (t == typeof(ushort)) s = "ushort";
            else if (t == typeof(uint)) s = "uint";
            else if (t == typeof(ulong)) s = "ulong";
            else if (t == typeof(sbyte)) s = "sbyte";
            else if (t == typeof(short)) s = "short";
            else if (t == typeof(int)) s = "int";
            else if (t == typeof(long)) s = "long";
            else if (t == typeof(float)) s = "float";
            else if (t == typeof(double)) s = "double";
            else if (t == typeof(decimal)) s = "decimal";
            return s;
        }
        #endregion
    }
}
