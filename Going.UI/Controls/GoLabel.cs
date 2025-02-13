using Going.UI.Control.Controls;
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
        public string? IconString { get; set; }
        public float IconSize { get; set; } = 12;
        public GoDirectionHV IconDirection { get; set; }
        public float IconGap { get; set; } = 5;
        public string Text { get; set; } = "label";
        public string FontName { get; set; } = "나눔고딕";
        public float FontSize { get; set; } = 12;
        public GoPadding TextPadding { get; set; } = new GoPadding(0, 0, 0, 0);

        public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;

        public string TextColor { get; set; } = "Fore";
        public string LabelColor { get; set; } = "Base2";
        public GoRoundType Round { get; set; } = GoRoundType.All;

        public bool BackgroundDraw { get; set; } = true;
        public bool BorderOnly { get; set; } = false;
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
            Util.DrawTextIcon(canvas, Text, FontName, FontSize, IconString, IconSize, IconDirection, IconGap, rtText, cText, ContentAlignment);

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
