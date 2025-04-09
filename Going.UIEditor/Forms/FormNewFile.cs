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
    public partial class FormNewFile : GoForm
    {
        #region class : Return
        public class Return
        {
            public string Name { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }
        #endregion

        #region Constructor
        public FormNewFile()
        {
            InitializeComponent();

            btnOK.ButtonClicked += (o, s) => { if (ValidCheck()) DialogResult = DialogResult.OK; };
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;
        }
        #endregion

        #region Method
        #region ValidCheck
        bool ValidCheck() => !string.IsNullOrWhiteSpace(inName.Value) && inWidth.Value > 0 && inHeight.Value > 0;
        #endregion
        #region ShowNewFile
        public Return? ShowNewFile()
        {
            Return? ret = null;

            Title = LM.NewFile;
            btnOK.Text = LM.Ok;
            btnCancel.Text = LM.Cancel;
            lblProjectName.Text = LM.ProjectName;
            lblScreenSize.Text = LM.ScreenSize;

            inName.Value = "";
            inWidth.Value = 800;
            inHeight.Value = 480;

            if(this.ShowDialog() == DialogResult.OK)
            {
                ret = new Return { Name = inName.Value, Width = inWidth.Value, Height = inHeight.Value };
            }

            return ret;
        }
        #endregion
        #endregion
    }
}
