using Going.UI.Forms.Dialogs;
using Going.UI.Themes;
using Going.UIEditor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Going.UIEditor.Forms.Dialogs
{
    public partial class FormTheme : GoForm
    {
        bool nouse = false;

        public FormTheme()
        {
            InitializeComponent();
            btnOK.ButtonClicked += (o, s) => { nouse = false; DialogResult = DialogResult.OK; };
            btnCancel.ButtonClicked += (o, s) => { nouse = false; DialogResult = DialogResult.Cancel; };
            btnNoUse.ButtonClicked += (o, s) => { nouse = true; DialogResult = DialogResult.OK; };
            
        }

        public (bool notUse, GoTheme? thm) ShowTheme(GoTheme thm)
        {
            nouse = false;
            GoTheme? ret = null;

            Title = LM.ThemeEditor;
            btnOK.Text = LM.Ok;
            btnCancel.Text = LM.Cancel;
            btnNoUse.Text = LM.NoUse;

            #region Set
            ooDark.OnOff = thm.Dark;

            cinFore.Value = thm.Fore;
            cinBack.Value = thm.Back;
            cinWindow.Value = thm.Window;
            cinWindowBorder.Value = thm.WindowBorder;
            cinPoint.Value = thm.Point;
            cinTitle.Value = thm.Title;

            cinScrollBar.Value = thm.ScrollBar;
            cinScrollCursor.Value = thm.ScrollCursor;
            cinGood.Value = thm.Good;
            cinWarning.Value = thm.Warning;
            cinDanger.Value = thm.Danger;
            cinError.Value = thm.Error;
            cinHighlight.Value = thm.Highlight;
            cinSelect.Value = thm.Select;

            cinBase0.Value = thm.Base0;
            cinBase1.Value = thm.Base1;
            cinBase2.Value = thm.Base2;
            cinBase3.Value = thm.Base3;
            cinBase4.Value = thm.Base4;
            cinBase5.Value = thm.Base5;

            cinUser1.Value = thm.User1;
            cinUser2.Value = thm.User2;
            cinUser3.Value = thm.User3;
            cinUser4.Value = thm.User4;
            cinUser5.Value = thm.User5;
            cinUser6.Value = thm.User6;
            cinUser7.Value = thm.User7;
            cinUser8.Value = thm.User8;
            cinUser9.Value = thm.User9;

            inCorner.Value = thm.Corner;
            inDownBrightness.Value = Math.Round(thm.DownBrightness,3);
            inBorderBrightness.Value = Math.Round(thm.BorderBrightness,3);
            inHoverBorderBrightness.Value = Math.Round(thm.HoverBorderBrightness, 3);
            inHoverFillBrightness.Value = Math.Round(thm.HoverFillBrightness, 3);
            inShadowAlpha.Value = thm.ShadowAlpha;
            #endregion

            if (this.ShowDialog() == DialogResult.OK && !nouse)
            {
                ret = new GoTheme();
                #region Get
                ret.Dark = ooDark.OnOff;

                ret.Fore = cinFore.Value;
                ret.Back = cinBack.Value;
                ret.Window = cinWindow.Value;
                ret.WindowBorder = cinWindowBorder.Value;
                ret.Point = cinPoint.Value;
                ret.Title = cinTitle.Value;

                ret.ScrollBar = cinScrollBar.Value;
                ret.ScrollCursor = cinScrollCursor.Value;
                ret.Good = cinGood.Value;
                ret.Warning = cinWarning.Value;
                ret.Danger = cinDanger.Value;
                ret.Error = cinError.Value;
                ret.Highlight = cinHighlight.Value;
                ret.Select = cinSelect.Value;

                ret.Base0 = cinBase0.Value;
                ret.Base1 = cinBase1.Value;
                ret.Base2 = cinBase2.Value;
                ret.Base3 = cinBase3.Value;
                ret.Base4 = cinBase4.Value;
                ret.Base5 = cinBase5.Value;

                ret.User1 = cinUser1.Value;
                ret.User2 = cinUser2.Value;
                ret.User3 = cinUser3.Value;
                ret.User4 = cinUser4.Value;
                ret.User5 = cinUser5.Value;
                ret.User6 = cinUser6.Value;
                ret.User7 = cinUser7.Value;
                ret.User8 = cinUser8.Value;
                ret.User9 = cinUser9.Value;

                ret.Corner = inCorner.Value;
                ret.DownBrightness = Convert.ToSingle(inDownBrightness.Value);
                ret.BorderBrightness = Convert.ToSingle(inBorderBrightness.Value);
                ret.HoverBorderBrightness = Convert.ToSingle(inHoverBorderBrightness.Value);
                ret.HoverFillBrightness = Convert.ToSingle(inHoverFillBrightness.Value);
                ret.ShadowAlpha = Convert.ToByte(inShadowAlpha.Value);
                #endregion
            }

            return (nouse, ret);
        }
    }
}
