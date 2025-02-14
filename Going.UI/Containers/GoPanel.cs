using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public class GoPanel : GoContainer
    {
        #region Properties
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public GoDirectionHV IconDirection { get; set; }
        public float IconGap { get; set; } = 5;

        public string Text { get; set; } = "Panel";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;

        public string TextColor { get; set; } = "Fore";
        public string PanelColor { get; set; } = "Base2";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public bool BackgroundDraw { get; set; } = true;
        public bool BorderOnly { get; set; } = false;
 
        public float TitleHeight { get; set; } = 30;
        #endregion

        #region Override
        #region OnContainerDraw
        protected override void OnContainerDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cPanel = thm.ToColor(PanelColor);
            var cBorder = cPanel.BrightnessTransmit(thm.BorderBrightness); 
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtTitle = rts["Title"];
            var rtTitleText = Util.FromRect(rtTitle, new GoPadding(10, 0, 0, 0));

            if (BackgroundDraw)
            {
                Util.DrawBox(canvas, rtBox, BorderOnly ? SKColors.Transparent : cPanel, cPanel, Round, thm.Corner);

                using var p = new SKPaint { IsAntialias = false };
                using var pe = SKPathEffect.CreateDash([3, 3], 2);
                p.PathEffect = pe;
                p.Color = cBorder;
                canvas.DrawLine(rtTitle.Left + 10, rtTitle.Bottom, rtTitle.Right - 10, rtTitle.Bottom, p);
            }
            Util.DrawTextIcon(canvas, Text, FontName, FontSize, IconString, IconSize, IconDirection, IconGap, rtTitleText, cText, GoContentAlignment.MiddleLeft);

            base.OnContainerDraw(canvas);
        }
        #endregion
        #region OnLayout
        protected override void OnLayout()
        {
            var rts = Areas();
            var rtPanel = rts["Panel"];
            foreach (var c in Childrens)
            {
                if (c.Fill)
                {
                    c.Bounds = Util.FromRect(rtPanel, c.Margin);
                }
            }

            base.OnLayout();
        }
        #endregion
        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();

            var rts =  Util.Rows(dic["Content"], [$"{TitleHeight}px", "100%"]);
            dic["Title"] = rts[0];
            dic["Panel"] = rts[1];

            return dic;
        }
        #endregion
        #endregion
    }

}
