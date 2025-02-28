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

            goPanel1.Enabled = false;
            goTabControl1.Enabled = false;
        }
    }
}
