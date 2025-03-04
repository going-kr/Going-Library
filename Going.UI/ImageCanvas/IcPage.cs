using Going.UI.Design;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    public class IcPage : GoPage
    {
        public string BackgroundColor { get; set; } = "white";

        protected override void OnBackgroundDraw(SKCanvas canvas)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            if (Design != null && Name != null)
            {
                var ip = Design.GetIC();
                if (ip != null && ip.Pages.TryGetValue(Name, out var bm) && bm.Off != null) canvas.DrawBitmap(bm.Off, rtContent);
            }
        }
    }

}
