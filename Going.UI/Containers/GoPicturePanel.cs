using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public class GoPicturePanel : GoContainer
    {
        #region Properties
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
        [GoImageProperty(PCategory.Control, 0)] public string? Image { get; set; }
        [GoProperty(PCategory.Control, 1)] public GoImageScaleMode ScaleMode { get; set; } = GoImageScaleMode.Real;
        [GoProperty(PCategory.Control, 2)] public GoRoundType Round { get; set; } = GoRoundType.All;
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoPicturePanel(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoPicturePanel() { }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            if (Image != null && Design != null)
            {
                var ls = Design.GetImage(Image);
                if (ls.Count > 0)
                {
                    var cx = rtContent.MidX;
                    var cy = rtContent.MidY;
                    var img = ls.First();

                    var rt = rtContent;
                    #region bounds
                    switch (ScaleMode)
                    {
                        case GoImageScaleMode.Real:
                            rt = Util.FromRect(rtContent.Left, rtContent.Top, img.Width, img.Height);
                            break;
                        case GoImageScaleMode.CenterImage:
                            rt = Util.FromRect(cx - (img.Width / 2), cy - (img.Height / 2), img.Width, img.Height);
                            break;
                        case GoImageScaleMode.Strech:
                            rt = rtContent;
                            break;
                        case GoImageScaleMode.Zoom:
                            var ia = (float)img.Width / img.Height;
                            var ta = rtContent.Width / rtContent.Height;
                            float w, h;
                            if (ia > ta) { w = rtContent.Width; h = rtContent.Width / ia; }
                            else { h = rtContent.Height; w = rtContent.Height * ia; }

                            rt = MathTool.MakeRectangle(rtContent, new SKSize(w, h), GoContentAlignment.MiddleCenter);
                            break;
                    }
                    #endregion

                    using var path = PathTool.Box(rt, Round, thm.Corner);
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipPath(path, SKClipOperation.Intersect, true);
                        canvas.DrawImage(img, rt, Util.Sampling);
                    }
                }
            }
            base.OnDraw(canvas, thm);
        }
        #endregion
    }
}
