using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Gudx;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Design
{
    /// <summary>
    /// 페이지 컨트롤. 디자인 내에서 화면 단위를 구성하며 배경 이미지를 지원합니다.
    /// </summary>
    public class GoPage : GoContainer
    {
        #region Properties
        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [GoChildList]
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
        /// <summary>
        /// 배경 이미지 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Basic, 5)] public string? BackgroundImage { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// <see cref="GoPage"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        [JsonConstructor]
        public GoPage(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary>
        /// <see cref="GoPage"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoPage() { }
        #endregion

        #region Override
        #region Draw
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            OnBackgroundDraw(canvas, thm);

            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        protected virtual void OnBackgroundDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            if (Design != null && BackgroundImage != null)
            {
                var bg = Design.GetImage(BackgroundImage);
                if (bg != null && bg.Count > 0) canvas.DrawImage(bg[0], rtContent, Util.Sampling);
            }
        }
        #endregion
        #endregion
    }
}
