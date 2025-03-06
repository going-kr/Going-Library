using Going.UI.Containers;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Design
{
    public class GoTitleBar : GoContainer
    {
        public float BarSize { get; set; } = 50F;

        public string Title { get; set; } = "Title";
        public string? TitleImage { get; set; } = null;
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;

        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            

            base.OnDraw(canvas);
        }

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();

            var sz = Util.MeasureText(Title, FontName, FontStyle, FontSize);

            


            return dic;
        }
        #endregion
    }

    public class GoLeftSideBar : GoContainer
    {
        public float BarSize { get; set; } = 100F;
    }

    public class GoRightSideBar:GoContainer
    {
        public float BarSize { get; set; } = 100F;
    }

    public class GoFooter : GoContainer
    {
        public float BarSize { get; set; } = 30F;
    }
}
