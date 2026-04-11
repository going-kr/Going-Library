using Going.UI.Tools;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Extensions
{
    /// <summary>
    /// SKColor에 대한 색상 관련 확장 메서드를 제공합니다.
    /// </summary>
    public static class ColorExtension
    {
        #region BrightnessTransmit
        /// <summary>
        /// 색상의 밝기를 비율로 조절합니다. 현재 밝기에 비례하여 밝기를 변경합니다.
        /// </summary>
        /// <param name="color">원본 색상</param>
        /// <param name="Brightness">밝기 비율 (-1.0 ~ 1.0). 양수는 밝게, 음수는 어둡게</param>
        /// <returns>밝기가 조절된 새 색상</returns>
        public static SKColor BrightnessTransmit(this SKColor color, float Brightness)
        {
            float h, s, l;
            color.ToHsl(out h, out s, out l);
            var lc = MathTool.Constrain(l * Brightness, -100, 100);
            l = Convert.ToSingle(MathTool.Constrain(l + lc, 0, 100));
            return SKColor.FromHsl(h, s, l, color.Alpha);
        }
        #endregion
        #region BrightnessChange
        /// <summary>
        /// 색상의 밝기를 절대값으로 변경합니다.
        /// </summary>
        /// <param name="color">원본 색상</param>
        /// <param name="Brightness">밝기 값 (-1.0 ~ 1.0). -1은 가장 어둡고, 1은 가장 밝음</param>
        /// <returns>밝기가 변경된 새 색상</returns>
        public static SKColor BrightnessChange(this SKColor color, float Brightness)
        {
            float h, s, l;
            color.ToHsl(out h, out s, out l);
            var lc = MathTool.Map(MathTool.Constrain(Brightness, -1, 1), -1, 1, 0, 100);
            l = Convert.ToSingle(lc);
            return SKColor.FromHsl(h, s, l, color.Alpha);
        }
        #endregion
        #region Reverse
        /// <summary>
        /// 색상을 반전합니다. 각 RGB 채널 값을 255에서 뺀 값으로 변환합니다.
        /// </summary>
        /// <param name="color">원본 색상</param>
        /// <returns>반전된 새 색상</returns>
        public static SKColor Reverse(this SKColor color)
        {
            return new SKColor(Convert.ToByte(255 - color.Red), Convert.ToByte(255 - color.Blue), Convert.ToByte(255 - color.Blue), color.Alpha);
        }
        #endregion

    }

}
