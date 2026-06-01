using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Extensions;
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
        [GoProperty(PCategory.Control, 4)] public float BorderWidth { get; set; } = 1.5F;

        /// <summary>
        /// Elevation 단계 (0~5). 0이면 그림자 없음, 1~5는 점진적으로 강한 드롭 그림자가 외곽에 그려져 카드가 떠있는 효과를 만듭니다.
        /// 그림자는 부모 컨테이너에서 그려지므로 카드 외부 여백이 충분해야 잘리지 않습니다.
        /// </summary>
        [GoProperty(PCategory.Control, 5)] public int Elevation { get; set; } = 0;

        /// <summary>
        /// 자식 컨트롤 영역의 내부 여백을 가져오거나 설정합니다.
        /// 자식들은 이 값만큼 안쪽으로 인셋된 영역에 배치됩니다.
        /// </summary>
        [GoProperty(PCategory.Bounds, 11)] public GoPadding Padding { get; set; } = new(8, 8, 8, 8);

        /// <summary>
        /// 자식 컨트롤이 배치되는 패널 영역을 가져옵니다. Padding만큼 인셋됩니다.
        /// </summary>
        [JsonIgnore] public override SKRect PanelBounds
            => Util.FromRect(Util.FromRect(0, 0, Width, Height), Padding);
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

            if (BackgroundDraw)
            {
                if (Elevation > 0)
                    Util.DrawElevationShadow(canvas, rtBox, Elevation, Round, thm.Corner, thm.ShadowColor, thm.ShadowAlpha);

                // 표면 재질 그라데이션 (단색 명도 1단계: 위 밝음 → 아래 어두움). 다크 HMI에서 깊이감을 줌.
                using (var p = new SKPaint { IsAntialias = true, IsStroke = false })
                using (var sh = SKShader.CreateLinearGradient(
                    new SKPoint(rtBox.MidX, rtBox.Top), new SKPoint(rtBox.MidX, rtBox.Bottom),
                    new[] { cBox.BrightnessTransmit(thm.GradientLightBrightness), cBox.BrightnessTransmit(thm.GradientDarkBrightness) },
                    new[] { 0F, 1F }, SKShaderTileMode.Clamp))
                {
                    p.Shader = sh;
                    if (Round == GoRoundType.Ellipse) canvas.DrawOval(rtBox, p);
                    else if (Round == GoRoundType.Rect) canvas.DrawRect(rtBox, p);
                    else { using var rr = new SKRoundRect(rtBox, thm.Corner); canvas.DrawRoundRect(rr, p); }
                }

                // 테두리만 (채움은 위 재질 그라데이션이 담당)
                Util.DrawBox(canvas, rtBox, SKColors.Transparent, cBorder, Round, thm.Corner, true, BorderWidth);
            }

            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        protected override void OnLayout()
        {
            var rtPanel = PanelBounds;

            var nofills = Childrens.Where(c => c.Dock != GoDockStyle.Fill).ToList();
            var fills = Childrens.Where(c => c.Dock == GoDockStyle.Fill).ToList();
            var bnds = Util.FromRect(0, 0, rtPanel.Width, rtPanel.Height);
            foreach (var control in nofills) bnds = ApplyDocking(control, bnds);
            foreach (var c in fills)
            {
                c.Left = bnds.Left + c.Margin.Left;
                c.Top = bnds.Top + c.Margin.Top;
                c.Right = bnds.Right - c.Margin.Right;
                c.Bottom = bnds.Bottom - c.Margin.Bottom;
            }
        }
        #endregion
    }
}
