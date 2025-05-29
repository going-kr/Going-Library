using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Design
{
    #region GoDropDownWindow
    public class GoDropDownWindow : GoContainer
    {
        public string WindowColor { get; set; } = "Window";
        public string BorderColor { get; set; } = "WindowBorder";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        [JsonConstructor]
        public GoDropDownWindow(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoDropDownWindow() { Visible = false; }

        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var rts = Areas();
            var rtWnd = rts["Content"];
            rtWnd = Util.Int(rtWnd);
            rtWnd.Offset(0.5F, 0.5F);
            var rtrWnd = new SKRoundRect(rtWnd, thm.Corner);
            Util.SetRound(rtrWnd, Round, thm.Corner);
            var cBack = thm.ToColor(WindowColor);
            var cBorder = thm.ToColor(BorderColor);

            using var p = new SKPaint { IsAntialias = true };

            if (cBack != SKColors.Transparent)
            {
                using var imgf = SKImageFilter.CreateDropShadow(3, 3, 3, 3, Util.FromArgb(thm.ShadowAlpha, SKColors.Black));
                p.ImageFilter = imgf;
                p.IsStroke = false;
                p.Color = cBack;
                canvas.DrawRoundRect(rtrWnd, p);
                p.ImageFilter = null;

            }

            if (cBorder != SKColors.Transparent)
            {
                p.IsStroke = true;
                p.StrokeWidth = 1F;
                p.Color = cBorder;
                canvas.DrawRoundRect(rtrWnd, p);
            }

            base.OnDraw(canvas);

        }


        public virtual void Show() => GoDesign.ActiveDesign?.ShowDropDownWindow(this);
        public virtual void Hide() => GoDesign.ActiveDesign?.HideDropDownWindow(this);
    }
    #endregion

    #region GoComboBoxDropDownWindow
    public class GoComboBoxDropDownWindow : GoDropDownWindow
    {
        private GoListBox lb;
        private Action<GoListItem?>? feedback;

        public GoComboBoxDropDownWindow()
        {
            lb = new GoListBox { Margin = new GoPadding(0), Fill = true, BackgroundDraw = false };
            Childrens.Add(lb);

            lb.ItemClicked += (o, s) =>
            {
                feedback?.Invoke(s.Item);
                Hide();
            };
        }

        public void Show(SKRect screenBounds, string fontName, GoFontStyle fontStyle, float fontSize, float itemHeight, int maximumViewCount,
                         List<GoListItem> items, GoListItem? selectedItem,
                         Action<GoListItem?> result)
        {
            feedback = result;

            lb.FontName = fontName;
            lb.FontSize = fontSize;
            lb.FontStyle = fontStyle;
            lb.IconSize = fontSize + 2;
            lb.IconGap = 3;
            lb.ItemHeight = itemHeight;

            var designH = GoDesign.ActiveDesign?.Height ?? 0;
            var rtValue = screenBounds;
            var iw = items.Count > 0 ? items.Select(x => Util.MeasureTextIcon(x.Text, lb.FontName, lb.FontStyle, lb.FontSize, x.IconString, lb.IconSize, GoDirectionHV.Horizon, lb.IconGap)).Max(x => x.Width) : 0F;
            var ih = itemHeight * Math.Min(maximumViewCount, items.Count) + 1;
            var w = Math.Max(iw, rtValue.Width);
            var bounds = Util.FromRect(rtValue.Left, rtValue.Bottom + 1, w, ih);
            if (bounds.Bottom > designH) bounds = Util.FromRect(rtValue.Left, rtValue.Top - ih - 1, Math.Max(iw, rtValue.Width), ih);

            Bounds = bounds;

            lb.Items.Clear();
            lb.SelectedItems.Clear();
            lb.Items.AddRange(items);
            if (selectedItem != null)
            {
                lb.SelectedItems.Add(selectedItem);
                lb.ScrollPosition = lb.Items.IndexOf(selectedItem) * itemHeight;
            }

            Show();
        }
    }
    #endregion

    #region GoColorDropDownWindow
    public class GoColorDropDownWindow : GoDropDownWindow
    {
        private GoColorSelector cs;
        private GoTableLayoutPanel tbl;
        private GoButton btnOK, btnCancel;
        private Action<SKColor?>? feedback;

        public GoColorDropDownWindow()
        {
            tbl = new GoTableLayoutPanel { Margin = new GoPadding(5), Fill = true, };
            tbl.Columns = ["100%", "80px", "80px"];
            tbl.Rows = ["100%", "40px"];
            Childrens.Add(tbl);

            cs = new GoColorSelector { Fill = true, Margin = new GoPadding(5) };
            tbl.Childrens.Add(cs, 0, 0, 3, 1);

            btnOK = new GoButton { Fill = true, Margin = new GoPadding(5), Text = "선택" };
            btnCancel = new GoButton { Fill = true, Margin = new GoPadding(5), Text = "취소" };
            tbl.Childrens.Add(btnOK, 1, 1);
            tbl.Childrens.Add(btnCancel, 2, 1);

            btnOK.ButtonClicked += (o, s) =>
            {
                feedback?.Invoke(cs.Value);
                Hide();
            };

            btnCancel.ButtonClicked += (o, s) =>
            {
                feedback?.Invoke(null);
                Hide();
            };
        }

        public void Show(SKRect screenBounds, string fontName, GoFontStyle fontStyle, float fontSize, SKColor value, Action<SKColor?> result)
        {
            feedback = result;

            btnOK.FontName = btnCancel.FontName = cs.FontName = fontName;
            btnOK.FontSize = btnCancel.FontSize = cs.FontSize = fontSize;
            btnOK.FontStyle = btnCancel.FontStyle = cs.FontStyle = fontStyle;
            cs.Value = value;

            var designH = GoDesign.ActiveDesign?.Height ?? 0;
            var rtValue = screenBounds;
            var w = MathTool.Constrain(rtValue.Width, 280, 360);
            var h = w + 40;

            var bounds = Util.FromRect(rtValue.Left, rtValue.Bottom + 1, w, h);
            if (bounds.Bottom > designH) bounds = Util.FromRect(rtValue.Left, rtValue.Top - h - 1, Math.Max(w, rtValue.Width), h);
            Bounds = bounds;

            Show();
        }
    }
    #endregion

    #region GoDateTimeDropDownWindow
    public class GoDateTimeDropDownWindow : GoDropDownWindow
    {
        private GoTableLayoutPanel tbl;
        private GoCalendar cal;
        private GoButton btnOK, btnCancel;
        private GoInputNumber<int> inH, inM, inS;
        private Action<DateTime?>? feedback;
        private GoDateTimeKind style = GoDateTimeKind.DateTime;

        public GoDateTimeDropDownWindow()
        {
            tbl = new GoTableLayoutPanel { Margin = new GoPadding(5), Fill = true, };
            Childrens.Add(tbl);

            cal = new GoCalendar { Fill = true, Margin = new GoPadding(5), BackgroundDraw = false, MultiSelect = false };
            inH = new GoInputNumber<int> { Fill = true, Margin = new GoPadding(5), Minimum = 0, Maximum = 23, UnitSize = 30, Unit = "시" };
            inM = new GoInputNumber<int> { Fill = true, Margin = new GoPadding(5), Minimum = 0, Maximum = 59, UnitSize = 30, Unit = "분" };
            inS = new GoInputNumber<int> { Fill = true, Margin = new GoPadding(5), Minimum = 0, Maximum = 59, UnitSize = 30, Unit = "초" };
            btnOK = new GoButton { Fill = true, Margin = new GoPadding(5), Text = "선택" };
            btnCancel = new GoButton { Fill = true, Margin = new GoPadding(5), Text = "취소" };

            btnOK.ButtonClicked += (o, s) =>
            {
                if (cal.SelectedDays.Count > 0)
                {
                    var dt = cal.SelectedDays.First().Date + new TimeSpan(inH.Value, inM.Value, inS.Value);

                    feedback?.Invoke(dt);
                    Hide();
                }
            };

            btnCancel.ButtonClicked += (o, s) =>
            {
                feedback?.Invoke(null);
                Hide();
            };
        }

        public void Show(SKRect screenBounds, string fontName, GoFontStyle fontStyle, float fontSize, DateTime value, GoDateTimeKind style, Action<DateTime?> result)
        {
            feedback = result;
            this.style = style;

            tbl.Childrens.Clear();

            var designH = GoDesign.ActiveDesign?.Height ?? 0;
            var rtValue = screenBounds;
            var w = MathTool.Constrain(rtValue.Width, 240, 300);
            var h = w + 40 + 40;
            if (style == GoDateTimeKind.DateTime)
            {
                h = w + 40 + 40;

                tbl.Columns = ["33.34%", "33.33%", "33.33%"];
                tbl.Rows = ["100%", "40px", "40px"];

                tbl.Childrens.Add(cal, 0, 0, 3, 1);
                tbl.Childrens.Add(inH, 0, 1);
                tbl.Childrens.Add(inM, 1, 1);
                tbl.Childrens.Add(inS, 2, 1);
                tbl.Childrens.Add(btnOK, 1, 2);
                tbl.Childrens.Add(btnCancel, 2, 2);

            }
            else if (style == GoDateTimeKind.Date)
            {
                h = w + 40;

                tbl.Columns = ["33.34%", "33.33%", "33.33%"];
                tbl.Rows = ["100%", "40px"];

                tbl.Childrens.Add(cal, 0, 0, 3, 1);
                tbl.Childrens.Add(btnOK, 1, 1);
                tbl.Childrens.Add(btnCancel, 2, 1);
            }
            else if (style == GoDateTimeKind.Time)
            {
                h = 80 + 10;

                tbl.Columns = ["33.34%", "33.33%", "33.33%"];
                tbl.Rows = ["40px", "40px"];

                tbl.Childrens.Add(inH, 0, 0);
                tbl.Childrens.Add(inM, 1, 0);
                tbl.Childrens.Add(inS, 2, 0);
                tbl.Childrens.Add(btnOK, 1, 1);
                tbl.Childrens.Add(btnCancel, 2, 1);
            }

            #region bounds
            var bounds = Util.FromRect(rtValue.Left, rtValue.Bottom + 1, Math.Max(w, rtValue.Width), h);
            if (bounds.Bottom > designH) bounds = Util.FromRect(rtValue.Left, rtValue.Top - h - 1, Math.Max(w, rtValue.Width), h);
            if(bounds.Top < 0) bounds = Util.FromRect(rtValue.Left, rtValue.MidY - (h/2F), Math.Max(w, rtValue.Width), h);
            Bounds = bounds;
            #endregion

            #region set
            IGoControl[] cls = [cal, inH, inM, inS, btnOK, btnCancel];
            foreach (var v in cls)
            {
                if (v is GoLabel c1) { c1.FontName = fontName; c1.FontSize = fontSize; c1.FontStyle = fontStyle; }
                else if (v is GoInputNumber<int> c2) { c2.FontName = fontName; c2.FontSize = fontSize; c2.FontStyle = fontStyle; }
                else if (v is GoCalendar c3) { c3.FontName = fontName; c3.FontSize = fontSize; c3.FontStyle = fontStyle; }
                else if (v is GoButton c4) { c4.FontName = fontName; c4.FontSize = fontSize; c4.FontStyle = fontStyle; }
            }

            cal.SelectedDays.Clear();
            cal.SelectedDays.Add(value.Date);
            inH.Value = style != GoDateTimeKind.Date ? value.Hour : 0;
            inM.Value = style != GoDateTimeKind.Date ? value.Minute : 0;
            inS.Value = style != GoDateTimeKind.Date ? value.Second : 0;
            #endregion

            Show();
        }
    }
    #endregion
}
