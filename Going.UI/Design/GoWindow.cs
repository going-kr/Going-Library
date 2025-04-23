using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Going.UI.Design
{
    public class GoWindow : GoContainer
    {
        #region Properties
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public float IconGap { get; set; } = 5;

        public string Text { get; set; } = "Window";
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string WindowColor { get; set; } = "Back";
        public string BorderColor { get; set; } = "Base2";
        public GoRoundType Round { get; set; } = GoRoundType.All;
        public float TitleHeight { get; set; } = 40;

        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        [JsonIgnore] public override SKRect PanelBounds => Areas()["Panel"];
        #endregion

        #region Member Variable
        bool bCloseHover = false;
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoWindow(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoWindow() { }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            #region var
            var thm = GoTheme.Current;
            var rts = Areas();
            var rtWnd = rts["Content"]; rtWnd = Util.Int(rtWnd); rtWnd.Offset(0.5F, 0.5F);
            var rtrWnd = new SKRoundRect(rtWnd, thm.Corner); Util.SetRound(rtrWnd, Round, thm.Corner);
            var rtTitle = rts["Title"];
            var rtClose = rts["Close"];
            var rtPanel = rts["Panel"];
            var rtTitleText = Util.FromRect(rtTitle, new GoPadding(10, 0, 0, 0));

            var cText = thm.ToColor(TextColor);
            var cBack = thm.ToColor(WindowColor);
            var cBorder = thm.ToColor(BorderColor);

            using var p = new SKPaint { IsAntialias = true };
            #endregion

            #region Fill
            if (cBack != SKColors.Transparent)
            {
                using var imgf = SKImageFilter.CreateDropShadow(3, 3, 3, 3, Util.FromArgb(thm.ShadowAlpha, SKColors.Black));
                p.ImageFilter = imgf;
                p.IsStroke = false;
                p.Color = cBack;
                canvas.DrawRoundRect(rtrWnd, p);
                p.ImageFilter = null;

            }
            #endregion
            #region Border
            if (cBorder != SKColors.Transparent)
            {
                p.IsStroke = true;
                p.StrokeWidth = 1F;
                p.Color = cBorder;
                canvas.DrawRoundRect(rtrWnd, p);
            }
            #endregion
            #region Title
            var x = (Design?.MousePosition.X ?? 0) - Left;
            var y = (Design?.MousePosition.Y ?? 0) - Top;
            var bCloseHover = CollisionTool.Check(rtClose, x, y);
            var cClose = !bCloseHover ? thm.Fore : SKColors.Red;

            Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, GoDirectionHV.Horizon, IconGap, rtTitleText, cText, GoContentAlignment.MiddleLeft);
            Util.DrawIcon(canvas, "fa-xmark", TitleHeight / 2, rtClose, cClose);
            #endregion
            #region Sep
            using var pe = SKPathEffect.CreateDash([3, 3], 2);
            p.PathEffect = pe;
            p.Color = cBorder;
            var sy = Convert.ToInt32(rtTitle.Bottom) + 0.5F;
            canvas.DrawLine(rtTitle.Left + 10, sy, rtTitle.Right - 10, sy, p);
            #endregion

            base.OnDraw(canvas);
        }
        #endregion

        #region Mouse
        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtClose = rts["Close"];

            if (CollisionTool.Check(rtClose, x, y))
            {
                OnCloseButtonClick();
            }

            base.OnMouseClick(x, y, button);
        }
        #endregion

        #region Layout
        protected override void OnLayout()
        {
            var rts = Areas();
            var rtPanel = rts["Panel"];
            foreach (var c in Childrens)
            {
                if (c.Fill)
                    c.Bounds = Util.FromRect(Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height), c.Margin);
            }

            //base.OnLayout();
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();
            var rts = Util.Rows(dic["Content"], [$"{TitleHeight}px", "100%"]);
            dic["Title"] = rts[0];
            dic["Panel"] = rts[1];
            dic["Close"] = new SKRect(rts[0].Right - TitleHeight, rts[0].Top, rts[0].Right, rts[0].Bottom);
            return dic;
        }
        #endregion
        #endregion

        #region Method
        protected virtual void OnCloseButtonClick() => Close();
        protected virtual void OnClosing(GoCancelableEventArgs e) { }

        public void Show() => (Design ?? GoDesign.ActiveDesign)?.ShowWindow(this);
        public void Show(float width, float height)
        {
            var design = Design ?? GoDesign.ActiveDesign;
            if (design != null)
            {
                Bounds = MathTool.MakeRectangle(Util.FromRect(0, 0, design.Width, design.Height), new SKSize(width, height));
                Show();
            }
        }

        public void Close()
        {
            var e = new GoCancelableEventArgs();
            OnClosing(e);
            if (!e.Cancel) Design?.HideWindow(this);
        }
        #endregion
    }
}
