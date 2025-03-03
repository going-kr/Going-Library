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


            for (int i = 1; i <= 7; i++)
            {
                var cat = new GoToolCategory { Text = $"Category {i}" };
                goToolBox1.Categories.Add(cat);

                for (int j = 1; j <= 10; j++)
                    cat.Items.Add(new GoToolItem { Text = $"Item {i}.{j}" });
            }
        }
    }
}
