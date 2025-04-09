using Going.UI.Forms.Dialogs;
using Going.UIEditor.Managers;
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
    public partial class FormSetting : GoForm
    {
        #region Constructor
        public FormSetting()
        {
            InitializeComponent();

            btnOK.ButtonClicked += (o, s) => DialogResult = DialogResult.OK;
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;

            #region lblPath.ButtonClicked
            lblPath.ButtonClicked += (o, s) =>
            {
                using (var fbd = new FolderBrowserDialog() { })
                {
                    fbd.SelectedPath = Program.DataMgr.ProjectFolder ?? "";
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        lblPath.Value = Util2.EllipsisPath(fbd.SelectedPath, lblPath.FontName, lblPath.FontStyle, lblPath.FontSize, lblPath.Width - (lblPath.TitleSize ?? 0) - (lblPath.ButtonSize ?? 0) - 20);
                        lblPath.Tag = fbd.SelectedPath;
                    }
                }
            };
            #endregion
            #region inLang.ValueChanged
            inLang.ValueChanged += (o, s) =>
            {
                if (inLang.Value) LangSet(Lang.EN);
                else LangSet(Lang.KO);
            };
            #endregion
        }
        #endregion

        #region Override
        protected override void OnShown(EventArgs e)
        {
            LangSet(Program.DataMgr.Language);

            base.OnShown(e);
        }
        #endregion

        #region Method
        #region LangSet
        void LangSet(Lang lang)
        {
            try
            {
                this.Invoke(() =>
                {
                    if (lang == Lang.KO)
                    {
                        Title = LM.ProgramSettingK;
                        lblPath.Title = LM.ProjectFolderK;
                        inLang.Title = LM.LanguageK;
                        btnOK.Text = LM.OkK;
                        btnCancel.Text = LM.CancelK;
                    }
                    else if (lang == Lang.EN)
                    {
                        Title = LM.ProgramSettingE;
                        lblPath.Title = LM.ProjectFolderE;
                        inLang.Title = LM.LanguageE;
                        btnOK.Text = LM.OkE;
                        btnCancel.Text = LM.CancelE;
                    }
                });
            }
            catch(Exception ex) { }
        }
        #endregion
        #region ShowSetting
        public Set? ShowSetting()
        {
            Set? ret = null;

            lblPath.Value = Util2.EllipsisPath(Program.DataMgr.ProjectFolder ?? "", lblPath.FontName, lblPath.FontStyle, lblPath.FontSize, lblPath.Width - (lblPath.TitleSize ?? 0) - (lblPath.ButtonSize ?? 0) - 20);
            lblPath.Tag = Program.DataMgr.ProjectFolder ?? "";
            inLang.Value = Program.DataMgr.Language == Lang.EN;

            if (this.ShowDialog() == DialogResult.OK)
            {
                ret = new Set
                {
                    ProjectFolder = (string)lblPath.Tag ,
                    Language = inLang.Value ? Lang.EN : Lang.KO,
                };
            }

            return ret;
        }
        #endregion
        #endregion
    }
}
