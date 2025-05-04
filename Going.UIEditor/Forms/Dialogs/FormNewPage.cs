using Going.UI.Forms.Dialogs;
using Going.UIEditor.Utils;
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
    public partial class FormNewPage : GoForm
    {
        #region class : Return
        public class Return
        {
            public string Name { get; set; }
            public bool UseIcPage { get; set; }
        }
        #endregion

        #region Constructor
        public FormNewPage()
        {
            InitializeComponent();

            btnOK.ButtonClicked += (o, s) => { if (ValidCheck()) DialogResult = DialogResult.OK; };
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Method
        #region ValidCheck
        bool ValidCheck() => !string.IsNullOrWhiteSpace(inName.Value);
        #endregion
        #region ShowNewFile
        public Return? ShowNewPage()
        {
            Return? ret = null;

            Title = $"{LM.Page} {LM.Add}";
            btnOK.Text = LM.Ok;
            btnCancel.Text = LM.Cancel;
            lblPageName.Text = LM.PageName;
            lblPageType.Text = LM.PageType;

            inName.Value = "";
            inUseIcPage.Value = false;

            if (this.ShowDialog() == DialogResult.OK)
            {
                ret = new Return { Name = inName.Value, UseIcPage = inUseIcPage.Value };
            }

            return ret;
        }
        #endregion
        #endregion
    }
}
