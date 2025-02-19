using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public class GoDropDownWindow : GoContainer
    {
        public GoDropDownWindow() { Visible = false; }

        protected override void OnDraw(SKCanvas canvas)
        {
            base.OnDraw(canvas);

            var thm = GoTheme.Current;
            var rts = Areas();
            var rtWnd = rts["Content"];
            Util.DrawBox(canvas, rtWnd, thm.Window, Enums.GoRoundType.All, thm.Corner);
        }
    }
}
