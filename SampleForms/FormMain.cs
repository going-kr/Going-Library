using Going.UI.Datas;
using Going.UI.Forms.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleForms
{
    public partial class FormMain : GoForm
    {
        public FormMain()
        {
            InitializeComponent();

            //goPanel1.Enabled = false;
            //goTabControl1.Enabled = false;

            goAnimate1.MouseDown += (o, s) => goAnimate1.OnOff = !goAnimate1.OnOff;

            #region ToolBox
            for (int i = 1; i <= 7; i++)
            {
                var cat = new GoToolCategory { Text = $"Category {i}" };
                goToolBox1.Categories.Add(cat);

                for (int j = 1; j <= 10; j++)
                    cat.Items.Add(new GoToolItem { Text = $"Item {i}.{j}" });
            }

            #region Drag
            goToolBox1.DragStart += (o, s) =>
            {
                goToolBox1.DoDragDrop(s.Item, DragDropEffects.All);
            };

            goLabel2.AllowDrop = true;
            goLabel2.DragEnter += (o, s) =>
            {
                if (s.Data?.GetDataPresent(typeof(GoToolItem)) ?? false)
                    s.Effect = DragDropEffects.All;
                else
                    s.Effect = DragDropEffects.None;
            };

            goLabel2.DragDrop += (o, s) =>
            {
                var v = s.Data?.GetData(typeof(GoToolItem)) as GoToolItem;
                if (v != null) goLabel2.Text = v.Text ?? "";
            };
            #endregion
            #endregion

            #region TreeView
            var rnd = new Random();

            for (int i = 0; i < 5; i++)
            {
                var nd0 = new GoTreeNode { Text = $"Node {i}" };
                goTreeView1.Nodes.Add(nd0);

                for (int j = 0; j < rnd.Next(0, 10); j++)
                {
                    var nd1 = new GoTreeNode { Text = $"Node {i}.{j}" };
                    nd0.Nodes.Add(nd1);

                    for (int k = 0; k < rnd.Next(0, 10); k++)
                    {
                        var nd2 = new GoTreeNode { Text = $"Node {i}.{j}.{k}" };
                        nd1.Nodes.Add(nd2);
                    }
                }
            }
            #endregion
        }
    }
}
