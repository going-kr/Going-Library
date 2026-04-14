using Going.UI.Controls;
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

namespace Going.UI.ImageCanvas
{
    /// <summary>
    /// 이미지 캔버스에서 사용되는 버튼 컨트롤입니다. 부모의 On/Off 이미지를 활용하여 눌림 효과를 표현합니다.
    /// </summary>
    public class IcButton : GoControl
    {
        #region Properties
        /// <summary>
        /// 아이콘 문자열을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        /// <summary>
        /// 아이콘 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        /// <summary>
        /// 아이콘 배치 방향을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public GoDirectionHV IconDirection { get; set; } = GoDirectionHV.Horizon;
        /// <summary>
        /// 아이콘과 텍스트 사이의 간격을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public float IconGap { get; set; } = 5;
        /// <summary>
        /// 버튼에 표시할 텍스트를 가져오거나 설정합니다.
        /// </summary>
        [GoMultiLineProperty(PCategory.Control, 4)] public string Text { get; set; } = "button";
        /// <summary>
        /// 글꼴 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoFontNameProperty(PCategory.Control, 5)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>
        /// 글꼴 스타일을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 6)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>
        /// 글꼴 크기를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 7)] public float FontSize { get; set; } = 12;
        /// <summary>
        /// 텍스트 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 8)] public string TextColor { get; set; } = "Fore";
        #endregion

        #region Member Variable
        private bool bDown = false;
        #endregion

        #region Event
        /// <summary>
        /// 버튼이 클릭되었을 때 발생하는 이벤트입니다.
        /// </summary>
        public event EventHandler? ButtonClicked;
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtBox = rts["Content"];
            var cText = thm.ToColor(TextColor).BrightnessTransmit(bDown ? thm.DownBrightness : 0);

            if (Design != null && Parent != null)
            {
                List<SKImage>? offs = null, ons = null;
                if (Parent is IcContainer con) { offs = Design.GetImage(con.OffImage); ons = Design.GetImage(con.OnImage); }
                else if (Parent is IcPage page) { offs = Design.GetImage(page.OffImage); ons = Design.GetImage(page.OnImage); }

                if (offs?.Count > 0 && ons?.Count > 0)
                {
                    var off = offs.First();
                    var on = ons.First();

                    var sx = (double)on.Width / Parent.Bounds.Width;
                    var sy = (double)on.Height / Parent.Bounds.Height;
                    canvas.DrawImage(bDown ? on : off, Util.FromRect(Convert.ToInt32(Left * sx), Convert.ToInt32(Top * sy), Convert.ToInt32(Width * sx), Convert.ToInt32(Height * sy)), rtBox, Util.Sampling);

                    if (bDown) rtBox.Offset(0, 1);
                    Util.DrawTextIcon(canvas, Text, FontName, FontStyle, FontSize, IconString, IconSize, IconDirection, IconGap, rtBox, cText, GoContentAlignment.MiddleCenter);
                }
            }

            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];
            if (CollisionTool.Check(rtBox, x, y)) bDown = true;

            base.OnMouseDown(x, y, button);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtBox = rts["Content"];
           if (bDown)
            {
                bDown = false;
                if (CollisionTool.Check(rtBox, x, y)) ButtonClicked?.Invoke(this, EventArgs.Empty);
            }

            base.OnMouseUp(x, y, button);
        }
        #endregion
    }
}
