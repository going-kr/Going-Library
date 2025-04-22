using Going.UI.Forms.Dialogs;
using Going.UIEditor.Controls;
using Going.UIEditor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UIEditor.Forms
{
    public partial class FormImageSelector : GoForm
    {
        #region Member Variable
        bool noSelect = false;
        #endregion

        #region Constructor
        public FormImageSelector()
        {
            InitializeComponent();
            grid.MultiSelect = false;
            btnOK.ButtonClicked += (o, s) => { if (ValidCheck()) DialogResult = DialogResult.OK; };
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;
            btnNotSelect.ButtonClicked += (o, s) => { noSelect = true; DialogResult = DialogResult.OK; };
        }
        #endregion

        #region Method
        #region RefrshGrid
        void RefreshGrid()
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var imgs = prj.Design.GetImages();
                var contents = imgs.Select(x => new ImageContent(prj.Design, grid)
                {
                    Name = x.name,
                    Images = x.images,
                });
                grid.Items.Clear();
                grid.Items.AddRange(contents);
                grid.Arrange();
                grid.Invalidate();
            }
        }
        #endregion
        #region ValidCheck
        bool ValidCheck() => grid.SelectedItems.Count > 0;
        #endregion
        #region ShowImageSelect
        public ImageContent? ShowImageSelect()
        {
            ImageContent? ret = null;

            noSelect = false;

            Title = LM.ImageSelector;
            btnOK.Text = LM.Ok;
            btnCancel.Text = LM.Cancel;
            lblTitle.Text = LM.ImageList;

            RefreshGrid();

            var prj = Program.CurrentProject;
            if (prj != null && this.ShowDialog() == DialogResult.OK)
            {
                if (noSelect) ret = new ImageContent(prj.Design, grid) { Name = "{NONE}" };
                else ret = grid.SelectedItems.FirstOrDefault() as ImageContent;
            }

            return ret;
        }
        #endregion
        #endregion
    }

}
