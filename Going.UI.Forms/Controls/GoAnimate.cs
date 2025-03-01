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
    public class GoAnimate : GoControl
    {
        #region Properties
        public string? OnImage { get => sOnImage; set { if (sOnImage != value) { sOnImage = value; Invalidate(); } } }
        public string? OffImage { get => sOffImage; set { if (sOffImage != value) { sOffImage = value; Invalidate(); } } }
        public GoImageScaleMode ScaleMode { get => eScaleMode; set { if (eScaleMode != value) { eScaleMode = value; Invalidate(); } } }
        public GoRoundType Round { get => eRound; set { if (eRound != value) { eRound = value; Invalidate(); } } }
        public int Time { get => tmr.Interval; set { if (tmr.Interval != value) { tmr.Interval = value; Invalidate(); } } }

        public GoResourceManager? Resources { get => rm; set { if (rm != value) { rm = value; Invalidate(); } } }

        public bool OnOff
        {
            get => tmr.Enabled;
            set
            {
                if (tmr.Enabled != value)
                {
                    idx = 0;
                    prev = DateTime.Now;
                    tmr.Enabled = value;

                    OnOffChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }
        #endregion

        #region Event
        public event EventHandler? OnOffChanged;
        #endregion

        #region Member Variable
        string? sOnImage = null, sOffImage = null;
        GoImageScaleMode eScaleMode = GoImageScaleMode.Real;
        GoRoundType eRound = GoRoundType.Rect;
        GoResourceManager? rm = null;

        System.Windows.Forms.Timer tmr = new System.Windows.Forms.Timer { Enabled = false, Interval = 30 };
        private int idx = 0;
        private DateTime prev = DateTime.Now;
        #endregion

        #region Constructor
        public GoAnimate()
        {
            tmr.Tick += (o, s) =>
            {
                if (OnOff && !DesignMode)
                {
                    if (OnImage != null && OffImage != null && rm != null)
                    {
                        var ls = rm.GetImage(OnOff ? OnImage : OffImage);
                        {
                            idx++;
                            if (idx >= ls.Count) idx = 0;
                            prev = DateTime.Now;
                            Invalidate();
                        }
                    }
                }
            };
        }
        #endregion

        #region Override
        protected override void OnContentDraw(ContentDrawEventArgs e)
        {
            var canvas = e.Canvas;
            var thm = GoTheme.Current;
            var rtContent = Util.FromRect(0, 0, Width - 1, Height - 1);

            if (OnImage != null && OffImage != null && rm != null)
            {
                var ls = rm.GetImage(OnOff ? OnImage : OffImage);
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

            base.OnContentDraw(e);
        }
        #endregion
    }
}
