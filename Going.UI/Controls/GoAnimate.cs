using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    public class GoAnimate : GoControl
    {
        #region Properties
        [GoImageProperty(PCategory.Misc, 0)] public string? OnImage { get; set; }
        [GoImageProperty(PCategory.Misc, 1)] public string? OffImage { get; set; }
        [GoProperty(PCategory.Misc, 2)] public GoImageScaleMode ScaleMode { get; set; } = GoImageScaleMode.Real;
        [GoProperty(PCategory.Misc, 3)] public GoRoundType Round { get; set; } = GoRoundType.Rect;
        [GoProperty(PCategory.Misc, 4)] public int Time { get; set; } = 30;
        [GoProperty(PCategory.Misc, 5)]
        public bool OnOff
        {
            get => bOnOff;
            set
            {
                if (bOnOff != value)
                {
                    idx = 0;
                    prev = DateTime.Now;
                    bOnOff = value;

                    OnOffChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region Event
        public event EventHandler? OnOffChanged;
        #endregion

        #region Member Variable
        bool bOnOff = false;
        private int idx = 0;
        private DateTime prev = DateTime.Now;
        #endregion

        #region Override
        #region OnDraw
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var rts = Areas();
            var rtContent = rts["Content"];

            if (OnImage != null && OffImage != null  && Design != null)
            {
                var ls = Design.GetImage(bOnOff ? OnImage : OffImage);
                if (ls.Count > 0)
                {
                    var cx = rtContent.MidX;
                    var cy = rtContent.MidY;
                    var img = idx >= 0 && idx < ls.Count && OnOff ? ls[idx] : ls[0];

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
            base.OnDraw(canvas);
        }

        protected override void OnUpdate()
        {
            if (OnOff)
            {
                if (OnImage != null && OffImage != null && Design != null)
                {
                    var ls = Design.GetImage(bOnOff ? OnImage : OffImage);
                    if ((DateTime.Now - prev).TotalMilliseconds >= Time)
                    {
                        idx++;
                        if (idx >= ls.Count) idx = 0;
                        prev = DateTime.Now;
                    }
                }
            }
            base.OnUpdate();
        }
        #endregion
        #endregion
    }
}
