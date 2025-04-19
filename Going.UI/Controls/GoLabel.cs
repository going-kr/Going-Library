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
        [GoProperty(PCategory.Misc, 0)] public string? IconString { get; set; }
        [GoProperty(PCategory.Misc, 1)] public float IconSize { get; set; } = 12;
        [GoProperty(PCategory.Misc, 2)] public GoDirectionHV IconDirection { get; set; }
        [GoProperty(PCategory.Misc, 3)] public float IconGap { get; set; } = 5;

        [GoProperty(PCategory.Misc, 4)] public string Text { get; set; } = "label";
        [GoProperty(PCategory.Misc, 5)] public string FontName { get; set; } = "나눔고딕";
        [GoProperty(PCategory.Misc, 6)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        [GoProperty(PCategory.Misc, 7)] public float FontSize { get; set; } = 12;
        [GoProperty(PCategory.Misc, 8)] public GoPadding TextPadding { get; set; } = new GoPadding(0, 0, 0, 0);

        [GoProperty(PCategory.Misc, 9)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;

        [GoProperty(PCategory.Misc, 10)] public string TextColor { get; set; } = "Fore";
        [GoProperty(PCategory.Misc, 11)] public string LabelColor { get; set; } = "Base2";
        [GoProperty(PCategory.Misc, 12)] public GoRoundType Round { get; set; } = GoRoundType.All;

        [GoProperty(PCategory.Misc, 13)] public bool BackgroundDraw { get; set; } = true;
        [GoProperty(PCategory.Misc, 14)] public bool BorderOnly { get; set; } = false;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var cText = thm.ToColor(TextColor);
            var cLabel = thm.ToColor(LabelColor);
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtText = rts["Text"];

            if (BackgroundDraw) Util.DrawBox(canvas, rtBox, BorderOnly ? SKColors.Transparent : cLabel, cLabel, Round, thm.Corner);
            Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtText, cText, ContentAlignment);

            base.OnDraw(canvas);
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
