using Going.UI.Forms.Dialogs;
using Going.UIEditor.Controls;
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

namespace Going.UIEditor.Forms.Editors
{
    public partial class FormFontSelector : GoForm
    {
        #region Member Variable
        bool noSelect = false;
        (string name, List<byte[]> fonts) defaultFont;
        #endregion

        #region Constructor
        public FormFontSelector()
        {
            InitializeComponent();

            grid.MultiSelect = false;
            btnOK.ButtonClicked += (o, s) => { if (ValidCheck()) DialogResult = DialogResult.OK; };
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;
            btnDefault.ButtonClicked += (o, s) => { noSelect = true; DialogResult = DialogResult.OK; };

            defaultFont = ("나눔고딕", FontPath.GetFontPaths("나눔고딕").Select(x => File.ReadAllBytes(x)).ToList());

        }
        #endregion

        #region Method
        #region SetLang
        void SetLang(string title)
        {
            Title = $"{LM.FontSelector} : {title}";
            btnOK.Text = LM.Ok;
            btnCancel.Text = LM.Cancel;
            btnDefault.Text = LM.DefaultFont;
            lblTitle.Text = LM.FontList;
        }
        #endregion
        #region RefrshGrid
        void RefreshGrid()
        {
            var prj = Program.CurrentProject;
            if (prj != null)
            {
                var fonts = prj.Design.GetFonts();
                fonts.Insert(0, defaultFont);
                var contents = fonts.Select(x => new FontContent(prj.Design, grid)
                {
                    Name = x.name,
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
        #region ShowFontSelect
        public FontContent? ShowFontSelect(string title)
        {
            FontContent? ret = null;

            noSelect = false;

            SetLang(title);
            RefreshGrid();

            var prj = Program.CurrentProject;
            if (prj != null && this.ShowDialog() == DialogResult.OK)
            {
                if (noSelect) ret = new FontContent(prj.Design, grid) { Name = "{NONE}" };
                else ret = grid.SelectedItems.FirstOrDefault() as FontContent;
            }

            return ret;
        }
        #endregion
        #endregion
    }
}
