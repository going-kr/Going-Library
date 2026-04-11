using Going.UI.Enums;
using Going.UI.Extensions;
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
    /// 아이콘만으로 구성된 버튼 컨트롤. 아이콘 회전 및 클릭 영역 확장을 지원합니다.
    /// </summary>
    public class GoIconButton : GoControl
    {
        #region Properties
        /// <summary>아이콘 문자열 (FontAwesome 등)</summary>
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        /// <summary>아이콘 회전 각도</summary>
        [GoProperty(PCategory.Control, 1)] public float Rotate { get; set; } = 0;
        /// <summary>버튼 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 2)] public string ButtonColor { get; set; } = "Base3";
        /// <summary>클릭 영역을 컨트롤 전체로 확장할지 여부. false이면 아이콘 경로 내부만 클릭 가능합니다.</summary>
        [GoProperty(PCategory.Control, 3)] public bool ClickBoundsExtends { get; set; } = false;
        #endregion

        #region Event
        /// <summary>버튼이 클릭되었을 때 발생하는 이벤트</summary>
        public event EventHandler? ButtonClicked;
        #endregion

        #region Constructor
        public GoIconButton()
        {
            Selectable = true;
        }
        #endregion

        #region Member Variable
        private bool bDown = false;
        private bool bHover = false;
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cBtn = thm.ToColor(ButtonColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);
            var rts = Areas();
            var rtBox = rts["Content"];

            var icosz = Math.Min(rtBox.Width, rtBox.Height) - 2;
            if (bDown) rtBox.Offset(0, 1);
            Util.DrawIcon(canvas, IconString, icosz, Rotate, rtBox, cBtn.BrightnessTransmit(bHover ? thm.HoverFillBrightness : 0), cBtn.BrightnessTransmit(bHover ? thm.HoverBorderBrightness : 0));

            base.OnDraw(canvas, thm);
        }

        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            var icosz = Math.Min(rtBox.Width, rtBox.Height) - 2;
            var pth = Util.GetIconPath(IconString, icosz, Rotate, rtBox);
            if (ClickBoundsExtends)
            {
                if (CollisionTool.Check(rtBox, x, y)) bDown = true;
            }
            else
            {
                if (pth != null && pth.Contains(x, y)) bDown = true;
            }
            pth?.Dispose();

            base.OnMouseDown(x, y, button);
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (bDown)
            {
                bDown = false;

                var icosz = Math.Min(rtBox.Width, rtBox.Height) - 2;
                var pth = Util.GetIconPath(IconString, icosz, Rotate, rtBox);
                if (pth != null && pth.Contains(x, y)) ButtonClicked?.Invoke(this, EventArgs.Empty);
                pth?.Dispose();
            }

            base.OnMouseUp(x, y, button);
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            var icosz = Math.Min(rtBox.Width, rtBox.Height) - 2;
            var pth = Util.GetIconPath(IconString, icosz, Rotate, rtBox);

            if (ClickBoundsExtends)
            {
                bHover = CollisionTool.Check(rtBox, x, y);
            }
            else
            {
                bHover = pth != null && pth.Contains(x, y);
            }

            
            pth?.Dispose();

            base.OnMouseMove(x, y);
        }

        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            bDown = false;
            base.OnMouseLongClick(x, y, button);
        }
        #endregion
    }
}
