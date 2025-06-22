using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace Going.UI.Forms.Dialogs
{
    public partial class GoSelectorBox : GoForm
    {
        #region Properties
        public string OkText { get => btnOk.Text; set => btnOk.Text = value; }
        public string CancelText { get => btnCancel.Text; set => btnCancel.Text = value; }

        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;
        #endregion

        #region Member Varaible
        GoTableLayoutPanel tpnl;
        GoTableLayoutPanel tpnl2;
        GoButton btnOk, btnCancel;
        #endregion

        #region Constructor
        public GoSelectorBox()
        {
            InitializeComponent();

            TitleIconString = "fa-check";
            TitleIconSize = 18;

            tpnl = new GoTableLayoutPanel { Fill = true, Margin = new GoPadding(10), Columns = ["34%", "33%", "33%"], Rows = ["100%", "40px"] };
            tpnl2 = new GoTableLayoutPanel { Fill = true, Margin = new GoPadding(0), };
            btnOk = new GoButton { Fill = true, Text = "확인" };
            btnCancel = new GoButton { Fill = true, Text = "취소" };

            tpnl.Childrens.Add(tpnl2, 0, 0, 3, 1);
            tpnl.Childrens.Add(btnOk, 1, 1);
            tpnl.Childrens.Add(btnCancel, 2, 1);

            btnOk.ButtonClicked += (o, s) => DialogResult = DialogResult.OK;
            btnCancel.ButtonClicked += (o, s) => DialogResult = DialogResult.Cancel;

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
        #region ShowCheck
        public List<GoListItem>? ShowCheck(string title, int columnCount, List<GoListItem> items, List<GoListItem>? selectedItems = null)
        {
            List<GoListItem>? ret = null;
            this.Text = title;

            var isz = items.Count > 0 ? items.Max(x => Util.MeasureTextIcon(x.Text, FontName, FontStyle, FontSize, x.IconString, FontSize + 2, Enums.GoDirectionHV.Horizon, 5).Width) : 0;

            var rowCount = Convert.ToInt32(Math.Ceiling((double)items.Count / (double)columnCount));
            var csz = 100F / columnCount;
            var rsz = 100F / rowCount;

            var lsc = new List<string>(); for (int i = 0; i < columnCount; i++) lsc.Add($"{csz}%");
            var lsr = new List<string>(); for (int i = 0; i < rowCount; i++) lsr.Add($"{rsz}%");

            tpnl2.Childrens.Clear();
            tpnl2.Columns = lsc;
            tpnl2.Rows = lsr;

            int ic = 0, ir = 0;
            foreach (var v in items)
            {
                var c = new GoCheckBox { Fill = true, Text = v.Text ?? "", Tag = v, ContentAlignment = GoContentAlignment.MiddleLeft, BoxSize = 20, Checked = selectedItems?.Contains(v) ?? false };
                c.SetInvalidate(Invalidate);
                tpnl2.Childrens.Add(c, ic, ir);

                ic++;
                if (ic == columnCount) { ic = 0; ir++; }
            }

            var w = Convert.ToInt32(Math.Max(((isz + 40) * columnCount) + 40, 200));
            var h = Convert.ToInt32(Math.Max((40 * rowCount) + 100, 120));
            this.Width = w;
            this.Height = h;
            if (this.ShowDialog() == DialogResult.OK)
            {
                ret = tpnl2.Childrens.Cast<GoCheckBox>().Where(x => x.Checked && x.Tag is GoListItem).Select(x => (GoListItem)x.Tag!).ToList();
            }

            return ret;
        }

        public List<T>? ShowCheck<T>(string title, int columnCount, List<T>? selectedItems = null, Func<T, string>? textCnverter = null) where T : struct, Enum
        {
            List<GoListItem> items = Enum.GetValues<T>().Select(x => new GoListItem { Text = textCnverter != null ? textCnverter(x) : x.ToString(), Tag = x }).ToList();
            List<GoListItem>? sel = selectedItems != null ? items.Where(x => x.Tag is T v && selectedItems.Contains(v)).ToList() : null;
            var vret = ShowCheck(title, columnCount, items, sel);
            if (vret != null)
            {
                var ls = new List<T>();
                foreach (var v in vret) if (v.Tag is T r) ls.Add(r);
                return ls;
            }
            else return null;
        }
        #endregion
        #region ShowRadio
        public GoListItem? ShowRadio(string title, int columnCount, List<GoListItem> items, GoListItem? selectedItem = null)
        {
            GoListItem? ret = null;
            this.Text = title;

            var isz = items.Count > 0 ? items.Max(x => Util.MeasureTextIcon(x.Text, FontName, FontStyle, FontSize, x.IconString, FontSize + 2, Enums.GoDirectionHV.Horizon, 5).Width) : 0;

            var rowCount = Convert.ToInt32(Math.Ceiling((double)items.Count / (double)columnCount));
            var csz = 100F / columnCount;
            var rsz = 100F / rowCount;

            var lsc = new List<string>(); for (int i = 0; i < columnCount; i++) lsc.Add($"{csz}%");
            var lsr = new List<string>(); for (int i = 0; i < rowCount; i++) lsr.Add($"{rsz}%");

            tpnl2.Childrens.Clear();
            tpnl2.Columns = lsc;
            tpnl2.Rows = lsr;

            int ic = 0, ir = 0;
            foreach (var v in items)
            {
                var c = new GoRadioBox { Fill = true, Text = v.Text ?? "", Tag = v, ContentAlignment = GoContentAlignment.MiddleLeft, BoxSize = 20, Checked = selectedItem == v };
                c.SetInvalidate(Invalidate);
                tpnl2.Childrens.Add(c, ic, ir);

                ic++;
                if (ic == columnCount) { ic = 0; ir++; }
            }

            var w = Convert.ToInt32(Math.Max(((isz + 40) * columnCount) + 40, 200));
            var h = Convert.ToInt32(Math.Max((40 * rowCount) + 100, 120));
            this.Width = w;
            this.Height = h;
            if (this.ShowDialog() == DialogResult.OK)
            {
                ret = tpnl2.Childrens.Cast<GoRadioBox>().FirstOrDefault(x => x.Checked && x.Tag is GoListItem)?.Tag as GoListItem;
            }
            return ret;
        }

        public T? ShowRadio<T>(string title, int columnCount, T? selectedItem = null, Func<T, string>? textCnverter = null) where T : struct, Enum
        {
            var items = Enum.GetValues<T>().Select(x => new GoListItem { Text = textCnverter != null ? textCnverter(x) : x.ToString(), Tag = x }).ToList();
            var sel = selectedItem != null ? items.FirstOrDefault(x => selectedItem.Value.Equals(x.Tag)) : null;
            var ret = ShowRadio(title, columnCount, items, sel);

            if (ret?.Tag is T v) return v;
            else return null;
        }
        #endregion
        #region ShowCombo
        public GoListItem? ShowCombo(string title, List<GoListItem> items, GoListItem? selectedItem = null)
        {
            GoListItem? ret = null;
            this.Text = title;

            tpnl2.Childrens.Clear();
            tpnl2.Columns = ["100%"];
            tpnl2.Rows = ["100%"];

            var isz = items.Count > 0 ? items.Max(x => Util.MeasureTextIcon(x.Text, FontName, FontStyle, FontSize, x.IconString, FontSize + 2, Enums.GoDirectionHV.Horizon, 5).Width) : 0;

            var cmb = new GoInputCombo { Fill = true, Items = items, SelectedIndex = selectedItem != null ? items.IndexOf(selectedItem) : -1 };
            cmb.DropDownOpening += (o, s) => { s.Cancel = true; OpenDropDown(cmb); };
            cmb.SetInvalidate(Invalidate);

            tpnl2.Childrens.Add(cmb, 0, 0);

            var w = Convert.ToInt32(Math.Max(isz + 80, 200));
            var h = Convert.ToInt32(Math.Max(40 + 100, 120));
            this.Width = w;
            this.Height = h;
            if (this.ShowDialog() == DialogResult.OK)
            {
                ret = tpnl2.Childrens[0] is GoInputCombo c && c.SelectedIndex >= 0 ? c.Items[c.SelectedIndex] : null;
            }
            return ret;
        }

        public T? ShowCombo<T>(string title, T? selectedItem = null, Func<T, string>? textCnverter = null) where T : struct, Enum
        {
            var items = Enum.GetValues<T>().Select(x => new GoListItem { Text = textCnverter != null ? textCnverter(x) : x.ToString(), Tag = x }).ToList();
            var sel = selectedItem != null ? items.FirstOrDefault(x => selectedItem.Value.Equals(x.Tag)) : null;
            var ret = ShowCombo(title, items, sel);

            if (ret?.Tag is T v) return v;
            else return null;
        }
        #endregion

        #region DropDown
        GoComboBoxDropDownWindow? dwnd;

        void Opened(GoInputCombo cmb)
        {
            if (dwnd != null)
            {
                var sel = cmb.SelectedIndex >= 0 && cmb.SelectedIndex < cmb.Items.Count ? cmb.Items[cmb.SelectedIndex] : null;
                dwnd.Set(FontName, FontStyle, FontSize, cmb.ItemHeight, cmb.MaximumViewCount, cmb.Items, sel, (item) =>
                {
                    try
                    {
                        if (item != null) cmb.SelectedIndex = cmb.Items.IndexOf(item);
                        Invoke(Invalidate);
                    }
                    catch { };
                });
            }
        }

        #region var
        bool closedWhileInControl;

        DropWindowState DropState { get; set; }
        bool CanDrop
        {
            get
            {
                if (dwnd != null) return false;
                if (dwnd == null && closedWhileInControl)
                {
                    closedWhileInControl = false;
                    return false;
                }

                return !closedWhileInControl;
            }
        }
        #endregion
        #region Method
        #region Freeze
        internal void FreezeDropDown(bool remainVisible)
        {
            if (dwnd != null)
            {
                dwnd.Freeze = true;
                if (!remainVisible) dwnd.Visible = false;
            }
        }

        internal void UnFreezeDropDown()
        {
            if (dwnd != null)
            {
                dwnd.Freeze = false;
                if (!dwnd.Visible) dwnd.Visible = true;
            }
        }
        #endregion
        #region Open
        private void OpenDropDown(GoInputCombo cmb)
        {
            var rtValue = cmb.Areas()["Value"];
            this.Move += (o, s) => { if (dwnd != null) dwnd.Bounds = GetDropDownBounds(cmb, rtValue); };

            dwnd = new();
            dwnd.Bounds = GetDropDownBounds(cmb, rtValue);
            dwnd.DropStateChanged += (o, s) => { DropState = s.DropState; };
            dwnd.FormClosed += (o, s) =>
            {
                if (dwnd != null && !dwnd.IsDisposed) dwnd.Dispose();
                dwnd = null;
                closedWhileInControl = (this.RectangleToScreen(this.ClientRectangle).Contains(Cursor.Position));
                DropState = DropWindowState.Closed;
                this.Invalidate();
            };

            Opened(cmb);

            DropState = DropWindowState.Dropping;
            dwnd.Show(this);
            DropState = DropWindowState.Dropped;
            this.Invalidate();
        }
        #endregion
        #region Close
        public void CloseDropDown()
        {
            if (dwnd != null)
            {
                DropState = DropWindowState.Closing;
                dwnd.Freeze = false;
                dwnd.Close();
            }
        }
        #endregion
        #region Bounds
        private Rectangle GetDropDownBounds(GoInputCombo cmb, SKRect rtValue)
        {
            int n = cmb.Items.Count;
            Point pt = PointToScreen(new Point(Convert.ToInt32(rtValue.Left + cmb.Left + tpnl.Left), Convert.ToInt32(rtValue.Bottom + cmb.Top + tpnl.Top + 1)));
            if (cmb.MaximumViewCount != -1) n = cmb.Items.Count > cmb.MaximumViewCount ? cmb.MaximumViewCount : cmb.Items.Count;
            Size inflatedDropSize = new Size(Convert.ToInt32(rtValue.Width) , n * cmb.ItemHeight + 2);
            Rectangle screenBounds = new Rectangle(pt, inflatedDropSize);
            Rectangle workingArea = Screen.GetWorkingArea(screenBounds);

            if (screenBounds.X < workingArea.X) screenBounds.X = workingArea.X;
            if (screenBounds.Y < workingArea.Y) screenBounds.Y = workingArea.Y;

            if (screenBounds.Right > workingArea.Right && workingArea.Width > screenBounds.Width) screenBounds.X = workingArea.Right - screenBounds.Width;
            if (screenBounds.Bottom > workingArea.Bottom && workingArea.Height > screenBounds.Height) screenBounds.Y = Convert.ToInt32(pt.Y - rtValue.Height - screenBounds.Height);
            return screenBounds;
        }
        #endregion
        #endregion
        #endregion
        #endregion 
    }
}
