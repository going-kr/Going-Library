using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    public class IcLabel : GoControl
    {
        #region Properties
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public GoDirectionHV IconDirection { get; set; } = GoDirectionHV.Horizon;
        public float IconGap { get; set; } = 5;
        public string Text { get; set; } = "label";
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;
        public string TextColor { get; set; } = "Black";
        public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        #endregion

        #region OnDraw
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var cText = thm.ToColor(TextColor);

            Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText, ContentAlignment);

            base.OnDraw(canvas);
        }
        #endregion
    }
}
