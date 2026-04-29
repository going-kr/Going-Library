using Going.UI.Controls;
using Going.UI.Enums;
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
        [GoChildList]
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

        /// <summary>
        /// Elevation 단계 (0~5). 0이면 그림자 없음, 1~5는 점진적으로 강한 드롭 그림자가 외곽에 그려져 카드가 떠있는 효과를 만듭니다.
        /// 그림자는 부모 컨테이너에서 그려지므로 카드 외부 여백이 충분해야 잘리지 않습니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public int Elevation { get; set; } = 0;
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
