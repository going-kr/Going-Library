using Going.UI.Managers;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Tools
{
    public class IconTool
    {
        public static Icon? GetIcon(string? iconString, SKColor color, int iconSize = 24)
        {
            Icon? ret = null;
            if (GoIconManager.Contains(iconString ?? ""))
            {
                using var surface = SKSurface.Create(new SKImageInfo(iconSize, iconSize, SKColorType.Rgba8888));
                var canvas = surface.Canvas;

                canvas.Clear(SKColors.Transparent);
                Util.DrawIcon(canvas, iconString, iconSize*0.9F, Util.FromRect(0, 0, iconSize, iconSize), color);

                using var bmp = BitmapFromSKSurface(surface);
                ret = Icon.FromHandle(bmp.GetHicon());
            }
            return ret;
        }

        public static Bitmap BitmapFromSKSurface(SKSurface surface)
        {
            using (SKImage image = surface.Snapshot())
            using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100)) // PNG로 인코딩
            using (MemoryStream ms = new MemoryStream(data.ToArray()))
            {
                return new Bitmap(ms); // System.Drawing.Bitmap 생성
            }
        }
    }
}
