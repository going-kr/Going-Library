using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoLabel : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 2)] public GoDirectionHV IconDirection { get; set; }
        [GoProperty(PCategory.Control, 3)] public float IconGap { get; set; } = 5;
        [GoMultiLineProperty(PCategory.Control, 4)] public string Text { get; set; } = "label";
        [GoFontNameProperty(PCategory.Control, 5)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Control, 6)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoFontSizeProperty(PCategory.Control, 7)] public float FontSize { get; set; } = 12;
        [GoProperty(PCategory.Control, 8)] public GoPadding TextPadding { get; set; } = new GoPadding(0, 0, 0, 0);
        [GoProperty(PCategory.Control, 9)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Control, 10)] public string LabelColor { get; set; } = "Base2";
        [GoProperty(PCategory.Control, 11)] public string BorderColor { get; set; } = "Base2";
        [GoProperty(PCategory.Control, 12)] public GoRoundType Round { get; set; } = GoRoundType.All;
        [GoProperty(PCategory.Control, 13)] public float BorderWidth { get; set; } = 1F;
        [GoProperty(PCategory.Control, 14)] public bool BackgroundDraw { get; set; } = true;
        [GoProperty(PCategory.Control, 15)] public bool BorderOnly{ get; set; } = false;
        [GoProperty(PCategory.Control, 16)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var cLabel = thm.ToColor(LabelColor);
            var cBor = thm.ToColor(BorderColor);
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtText = rts["Text"];
            var FontSize = Util.FontSize(this.FontSize, rtText.Height);

            if (BackgroundDraw)
            {
                if (BorderOnly)
                    Util.DrawBox(canvas, rtBox, SKColors.Transparent, cBor, Round, thm.Corner, true, BorderWidth);
                else
                    Util.DrawBox(canvas, rtBox, cLabel, cBor, Round, thm.Corner, true, BorderWidth);
            }

            Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtText, cText, ContentAlignment);

            base.OnDraw(canvas, thm);
        }

        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();

            dic["Text"] = Util.FromRect(dic["Content"], TextPadding);

            return dic;
        }
        #endregion

    }
}
