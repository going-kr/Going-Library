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
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 2)] public GoDirectionHV IconDirection { get; set; } = GoDirectionHV.Horizon;
        [GoProperty(PCategory.Control, 3)] public float IconGap { get; set; } = 5;
        [GoMultiLineProperty(PCategory.Control, 4)] public string Text { get; set; } = "label";
        [GoFontNameProperty(PCategory.Control, 5)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 6)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Control, 7)] public float FontSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 8)] public string TextColor { get; set; } = "Black";
        [GoProperty(PCategory.Control, 9)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        #endregion

        #region OnDraw
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var rtBox = Util.FromRect(0, 0, Width, Height);
            var cText = thm.ToColor(TextColor);

            if (Design != null && Parent != null && (Parent is IcPage || Parent is IcContainer))
                Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText, ContentAlignment);

            base.OnDraw(canvas);
        }
        #endregion
    }
}
