using Going.UI.Containers;
using Going.UI.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    public class IcContainer : GoContainer
    {
        public string? ContainerImage { get; set; } 
        public string BackgroundColor { get; set; } = "white";

        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];

        protected override void OnDraw(SKCanvas canvas)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            if (Design != null && ContainerImage != null)
            {
                var ip = Design.GetIC();
                if (ip != null && ip.Containers.TryGetValue(ContainerImage, out var bm) && bm.Off != null) canvas.DrawBitmap(bm.Off, rtContent);
            }

            base.OnDraw(canvas);
        }
    }
}
