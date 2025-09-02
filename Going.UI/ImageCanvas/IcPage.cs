using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    public class IcPage : GoPage
    {
        [GoProperty(PCategory.Control, 0)] public string BackgroundColor { get; set; } = "white";
        [GoImageProperty(PCategory.Control, 1)] public string? OffImage { get; set; }
        [GoImageProperty(PCategory.Control, 2)] public string? OnImage { get; set; }

        #region Constructor
        [JsonConstructor]
        public IcPage(List<IGoControl> childrens) : base(childrens) { }
        public IcPage() { }
        #endregion

        protected override void OnBackgroundDraw(SKCanvas canvas, GoTheme thm)
        {
            using var p = new SKPaint {  };

            var rts = Areas();
            var rtContent = rts["Content"];

            if (Design != null && Design.GetImage(OffImage) is List<SKImage> imgs && imgs.Count > 0)
                canvas.DrawImage(imgs.First(), rtContent, Util.Sampling);
            else
            {
                p.IsStroke = false;
                p.Color = thm.ToColor(BackgroundColor);
                canvas.DrawRect(rtContent, p);
            }
        }
    }

}
