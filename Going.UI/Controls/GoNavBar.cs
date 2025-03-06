using Going.UI.Datas;
using Going.UI.Enums;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoNavBar : GoControl
    {
        #region Properties
        public float IconSize { get; set; } = 12;
        public GoDirectionHV IconDirection { get; set; }
        public float IconGap { get; set; } = 5;
        public string FontName { get; set; } = "나눔고딕";
        public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        public float FontSize { get; set; } = 12;

        public bool DrawTitle { get; set; } = true;
        public string? TitleImage { get; set; }
        public string Title { get; set; } = "Title";

        public List<GoMenuItem> Menus { get; set; } = [];
        public List<GoButtonItem> Buttons { get; set; } = [];

        public GoDirection Direction { get; set; } =  GoDirection.Up;
        public float NavBarSize { get; set; } = 50;
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);
        }
        #endregion

        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            base.OnMouseMove(x, y);
        }
        #endregion

        #region Areas
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();


            return dic;
        }
        #endregion
        #endregion


    }
}
