using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    #region GoDropDownWindow
    public class GoDropDownWindow : GoContainer
    {
        public string WindowColor { get; set; } = "Window";
        public string BorderColor { get; set; } = "WindowBorder";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public GoDropDownWindow() { Visible = false; }

        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var rts = Areas();
            var rtWnd = rts["Content"];
            rtWnd= Util.Int(rtWnd);
            rtWnd.Offset(0.5F, 0.5F);
            var rtrWnd = new SKRoundRect(rtWnd, thm.Corner);
            Util.SetRound(rtrWnd, Round, thm.Corner);
            var cBack = thm.ToColor(WindowColor);
            var cBorder = thm.ToColor(BorderColor);

            using var p = new SKPaint { IsAntialias = true };
            
            if (cBack != SKColors.Transparent)
            {
                using var imgf = SKImageFilter.CreateDropShadow(3, 3, 3, 3, SKColors.Black);
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

        public void Show(SKRect screenBounds, string fontName, float fontSize, float itemHeight, int maximumViewCount,
                         List<GoListItem> items, GoListItem? selectedItem, 
                         Action<GoListItem?> result)
        {
            feedback = result;

            lb.FontName = fontName;
            lb.FontSize = fontSize;
            lb.IconSize = fontSize + 2;
            lb.IconGap = 3;
            lb.ItemHeight = itemHeight;

            var designH = GoDesign.ActiveDesign?.Height ?? 0;
            var iw = items.Count > 0 ? items.Select(x => Util.MeasureTextIcon(x.Text, lb.FontName, lb.FontSize, new SKSize(lb.IconSize, lb.IconSize), lb.IconDirection, lb.IconGap,
                                                        new SKRect(0, 0, 100, 40), GoContentAlignment.MiddleCenter)).Max(x => x.Width) : 0F;

            var ih = itemHeight * Math.Min(maximumViewCount, items.Count) + 1;
            var rtValue = screenBounds;
            var bounds = Util.FromRect(rtValue.Left, rtValue.Bottom, Math.Max(iw, rtValue.Width), ih);
            if (bounds.Bottom > designH) bounds = Util.FromRect(rtValue.Left, rtValue.Top - ih, Math.Max(iw, rtValue.Width), ih);

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
}
