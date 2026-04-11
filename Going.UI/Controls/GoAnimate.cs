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
    /// <summary>
    /// 애니메이션 컨트롤. ON/OFF 상태에 따라 이미지 시퀀스를 애니메이션으로 표시합니다.
    /// </summary>
    public class GoAnimate : GoControl
    {
        #region Properties
        /// <summary>
        /// ON 상태에서 표시할 이미지(프레임 시퀀스)의 리소스 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoImageProperty(PCategory.Control, 0)] public string? OnImage { get; set; }
        /// <summary>
        /// OFF 상태에서 표시할 이미지의 리소스 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoImageProperty(PCategory.Control, 1)] public string? OffImage { get; set; }
        /// <summary>
        /// 이미지 스케일 모드를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public GoImageScaleMode ScaleMode { get; set; } = GoImageScaleMode.Real;
        /// <summary>
        /// 모서리 둥글기 타입을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public GoRoundType Round { get; set; } = GoRoundType.Rect;
        /// <summary>
        /// 프레임 전환 간격(밀리초)을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public int Time { get; set; } = 30;
        /// <summary>
        /// ON/OFF 상태를 가져오거나 설정합니다. 값이 변경되면 <see cref="OnOffChanged"/> 이벤트가 발생합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)]
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
        /// <summary>
        /// <see cref="OnOff"/> 속성 값이 변경되었을 때 발생합니다.
        /// </summary>
        public event EventHandler? OnOffChanged;
        #endregion

        #region Member Variable
        private bool bOnOff = false;
        private int idx = 0;
        private DateTime prev = DateTime.Now;
        private SKPath path = new SKPath();
        #endregion

        #region Override
        #region OnDraw
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
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
                        case GoImageScaleMode.Stretch:
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

                    PathTool.Box(path, rt, Round, thm.Corner);
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipPath(path, SKClipOperation.Intersect, true);
                        canvas.DrawImage(img, rt, Util.Sampling);
                    }
                }
            }
            base.OnDraw(canvas, thm);
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

        protected override void OnDispose()
        {
            path.Dispose();
            base.OnDispose();
        }
        #endregion
        #endregion
    }
}
