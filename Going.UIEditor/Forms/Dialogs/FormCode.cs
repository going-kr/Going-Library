using Going.UI.Design;
using Going.UI.Forms.Dialogs;
using Going.UIEditor.Utils;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UIEditor.Forms.Dialogs
{
    public partial class FormCode : GoForm
    {
        public FormCode()
        {
            InitializeComponent();
        }

        public void ShowCode(GoPage page)
        {
            var (cPage, cDesign) = Code.MakePageCode("Test", page);
            
            txtDesign.Text = cDesign ?? "";
            
            this.ShowDialog();
        }
    }
}
