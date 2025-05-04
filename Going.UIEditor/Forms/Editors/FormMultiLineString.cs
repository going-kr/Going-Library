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

namespace Going.UIEditor.Forms
{
    public partial class FormMultiLineString : GoForm
    {
        public FormMultiLineString()
        {
            InitializeComponent();

            TitleIconString =  "fa-pen-to-square";
            btnOK.ButtonClicked += (o, s) => DialogResult = DialogResult.OK;
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;
        }

        protected override void OnShown(EventArgs e)
        {
            txt.SelectAll();
            txt.Focus();
            base.OnShown(e);
        }

        public string? ShowString(string title, string? value = null)
        {
            string? ret = null;

            Title = title;
            txt.Text = value ?? "";

            if(this.ShowDialog() == DialogResult.OK)
            {
                ret = txt.Text;
            }
            return ret;
        }
    }
}
