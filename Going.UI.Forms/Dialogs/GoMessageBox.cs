using Going.UI.Containers;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Controls;
using Going.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Going.UI.Datas;
using SkiaSharp;

namespace Going.UI.Forms.Dialogs
{
    public partial class GoMessageBox : GoForm
    {
        #region Properties
        public string YesText { get => btnYes.Text; set => btnYes.Text = value; }
        public string NoText { get => btnNo.Text; set => btnNo.Text = value; }
        public string OkText { get => btnOK.Text; set => btnOK.Text = value; }
        public string CancelText { get => btnCancel.Text; set => btnCancel.Text = value; }
        #endregion

        #region Member Variable
        GoTableLayoutPanel tpnl;
        GoLabel lbl;
        GoButton btnOK, btnCancel, btnYes, btnNo;
        #endregion

        #region Constructor
        public GoMessageBox()
        {
            InitializeComponent();

            TitleIconString = "fa-comment-dots";
            TitleIconSize = 18;

            tpnl = new GoTableLayoutPanel { Fill = true, Margin = new GoPadding(10), Columns = ["16.66%", "16.66%", "16.66%", "16.66%", "16.66%", "16.67%",], Rows = ["100%", "40px"] };
            lbl = new GoLabel { Fill = true, BackgroundDraw = false };
            btnOK = new GoButton { Fill = true, Text = "확인" };
            btnCancel = new GoButton { Fill = true, Text = "취소" };
            btnYes = new GoButton { Fill = true, Text = "예" };
            btnNo = new GoButton { Fill = true, Text = "아니요" };

            btnOK.ButtonClicked += (o, s) => DialogResult = DialogResult.OK;
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;
            btnYes.ButtonClicked += (o, s) => DialogResult = DialogResult.Yes;
            btnNo.ButtonClicked += (o, s) => DialogResult = DialogResult.No;
        }
        #endregion

        #region Override
        public override void OnContentDraw(Going.UI.Forms.Controls.ContentDrawEventArgs e)
        {
            tpnl.Bounds = Util.FromRect(Util.FromRect(0, 0, ClientSize.Width, ClientSize.Height), new GoPadding(10));
            using (new SKAutoCanvasRestore(e.Canvas))
            {
                e.Canvas.Translate(tpnl.Left, tpnl.Top);
                tpnl.FireDraw(e.Canvas);
            }
            base.OnContentDraw(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            tpnl.FireMouseDown(e.X - tpnl.Left, e.Y - tpnl.Top, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            tpnl.FireMouseMove(e.X - tpnl.Left, e.Y - tpnl.Top);
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            tpnl.FireMouseUp(e.X - tpnl.Left, e.Y - tpnl.Top, ToGoMouseButton(e.Button));
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            tpnl.FireMouseMove(-1, -1);
            Invalidate();
            base.OnMouseLeave(e);
        }
        #endregion

        #region Method
        (int w, int h) show(string title, string message)
        {
            this.Text = title;
            lbl.Text = message;

            var sz = Util.MeasureText(message, lbl.FontName, lbl.FontStyle, lbl.FontSize);
            var w = Math.Max(180, sz.Width + 80);
            var h = Math.Max(120, sz.Height + 50 + 40 + 40);

            return (Convert.ToInt32(w), Convert.ToInt32(h));
        }

        public DialogResult ShowMessageBoxOk(string title, string message)
        {
            var (w, h) = show(title, message);

            tpnl.Childrens.Clear();
            tpnl.Childrens.Add(lbl, 0, 0, 6, 1);
            tpnl.Childrens.Add(btnOK, 1, 1, 4, 1);
            tpnl.FireMouseMove(-1, -1);

            this.Width = w;
            this.Height = h;
            return this.ShowDialog();
        }

        public DialogResult ShowMessageBoxOkCancel(string title, string message)
        {
            var (w, h) = show(title, message);

            tpnl.Childrens.Clear();
            tpnl.Childrens.Add(lbl, 0, 0, 6, 1);
            tpnl.Childrens.Add(btnOK, 0, 1, 3, 1);
            tpnl.Childrens.Add(btnCancel, 3, 1, 3, 1);
            tpnl.FireMouseMove(-1, -1);

            this.Width = w;
            this.Height = h;
            return this.ShowDialog();
        }

        public DialogResult ShowMessageBoxYesNo(string title, string message)
        {
            var (w, h) = show(title, message);

            tpnl.Childrens.Clear();
            tpnl.Childrens.Add(lbl, 0, 0, 6, 1);
            tpnl.Childrens.Add(btnYes, 0, 1, 3, 1);
            tpnl.Childrens.Add(btnNo, 3, 1, 3, 1);
            tpnl.FireMouseMove(-1, -1);

            this.Width = w;
            this.Height = h;
            return this.ShowDialog();
        }

        public DialogResult ShowMessageBoxYesNoCancel(string title, string message)
        {
            var (w, h) = show(title, message);

            tpnl.Childrens.Clear();
            tpnl.Childrens.Add(lbl, 0, 0, 6, 1);
            tpnl.Childrens.Add(btnYes, 0, 1, 2, 1);
            tpnl.Childrens.Add(btnNo, 2, 1, 2, 1);
            tpnl.Childrens.Add(btnCancel, 4, 1, 2, 1);
            tpnl.FireMouseMove(-1, -1);

            this.Width = w;
            this.Height = h;
            return this.ShowDialog();
        }
        #endregion
    }
}
