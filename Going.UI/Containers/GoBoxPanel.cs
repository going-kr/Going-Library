using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
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
    /// <summary>
    /// 테두리와 배경색을 가진 박스 형태의 패널 컨테이너입니다.
    /// </summary>
    public class GoBoxPanel : GoContainer
    {
        #region Properties
        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
        /// <summary>
        /// 테두리 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public string BorderColor { get; set; } = "Base3";
        /// <summary>
        /// 박스 배경 색상을 가져오거나 설정합니다. 테마 색상 이름을 사용합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public string BoxColor { get; set; } = "Base2";
        /// <summary>
        /// 모서리 라운드 타입을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public GoRoundType Round { get; set; } = GoRoundType.All;

        /// <summary>
        /// 배경을 그릴지 여부를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 3)] public bool BackgroundDraw { get; set; } = true;
        /// <summary>
        /// 테두리 두께를 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 4)] public float BorderSize { get; set; } = 1F;
        #endregion

        #region Constructor
        /// <summary>
        /// 자식 컨트롤 목록을 사용하여 <see cref="GoBoxPanel"/>의 새 인스턴스를 초기화합니다. JSON 역직렬화에 사용됩니다.
        /// </summary>
        /// <param name="childrens">자식 컨트롤 목록</param>
        [JsonConstructor]
        public GoBoxPanel(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary>
        /// <see cref="GoBoxPanel"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoBoxPanel() { }
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cBox = thm.ToColor(BoxColor);
            var cBorder = thm.ToColor(BorderColor);
            var rts = Areas();
            var rtBox = rts["Content"];

            if (BackgroundDraw) Util.DrawBox(canvas, rtBox, cBox, cBorder, Round, thm.Corner, true, BorderSize);

            base.OnDraw(canvas, thm);
        }
        #endregion
    }
}
