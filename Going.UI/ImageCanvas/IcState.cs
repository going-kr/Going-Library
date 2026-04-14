using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    /// <summary>
    /// 상태 값에 따라 서로 다른 이미지를 표시하는 이미지 캔버스 컨트롤입니다.
    /// </summary>
    public class IcState : GoControl
    {
        #region Properties
        /// <summary>
        /// 상태별 이미지 목록을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public List<StateImage> StateImages { get; set; } = [];
        /// <summary>
        /// 현재 상태 값을 가져오거나 설정합니다. 이 값에 해당하는 이미지가 표시됩니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public int State { get; set; }
        #endregion

        #region Member Variable
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtBox = rts["Content"];

            if (Design != null && Parent != null)
            {
                var simg = StateImages.FirstOrDefault(x=>x.State == State);
                var img = Design.GetImage(simg?.Image)?.FirstOrDefault();
                if (img != null) canvas.DrawImage(img, rtBox, Util.Sampling);
            }

            base.OnDraw(canvas, thm);
        }
        #endregion
    }

    /// <summary>
    /// 특정 상태에 대응하는 이미지를 정의하는 클래스입니다.
    /// </summary>
    public class StateImage
    {
        /// <summary>
        /// 이미지 리소스 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoImageProperty(PCategory.Control, 0)] public string? Image { get; set; }
        /// <summary>
        /// 이 이미지가 표시될 상태 값을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public int State { get; set; }
    }
}
