using Going.UI.Enums;
using Going.UI.Forms.Components;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Controls
{
    public class GoPictureBox : GoControl
    {
        #region Properties
        public string? Image { get => sImage; set { if (sImage != value) { sImage = value; Invalidate(); } } }
        public GoImageScaleMode ScaleMode { get => eScaleMode; set { if (eScaleMode != value) { eScaleMode = value; Invalidate(); } } }
        public GoRoundType Round { get => eRound; set { if (eRound != value) { eRound = value; Invalidate(); } } }
        public GoResourceManager? Resources { get => rm; set { if (rm != value) { rm = value; Invalidate(); } } }
        #endregion

        #region Member Variable
        string? sImage = null;
        GoImageScaleMode eScaleMode = GoImageScaleMode.Real;
        GoRoundType eRound = GoRoundType.Rect;
        GoResourceManager? rm = null;
        #endregion

        #region Override
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            var canvas = e.Canvas;
            var thm = GoTheme.Current;
            var rtContent = Util.FromRect(0, 0, Width - 1, Height - 1);

            if (Image != null && Resources != null)
            {
                var ls = Resources.GetImage(Image);
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
                        canvas.DrawBitmap(img, rt);
                    }
                }
            }

            base.OnContentDraw(e);
        }
        #endregion
    }
}
