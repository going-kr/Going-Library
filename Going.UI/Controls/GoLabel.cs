using Going.UI.Datas;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Controls
{
    /// <summary>
    /// 라벨 컨트롤. 텍스트와 아이콘을 표시하는 읽기 전용 컨트롤입니다.
    /// </summary>
    public class GoLabel : GoControl
    {
        #region Properties
        /// <summary>아이콘 문자열 (FontAwesome 등)</summary>
        [GoProperty(PCategory.Control, 0)] public string? IconString { get; set; }
        /// <summary>아이콘 크기</summary>
        [GoProperty(PCategory.Control, 1)] public float IconSize { get; set; } = 12;
        /// <summary>아이콘 배치 방향</summary>
        [GoProperty(PCategory.Control, 2)] public GoDirectionHV IconDirection { get; set; }
        /// <summary>아이콘과 텍스트 사이 간격</summary>
        [GoProperty(PCategory.Control, 3)] public float IconGap { get; set; } = 5;
        /// <summary>표시할 텍스트</summary>
        [GoMultiLineProperty(PCategory.Control, 4)] public string Text { get; set; } = "label";
        /// <summary>글꼴 이름</summary>
        [GoFontNameProperty(PCategory.Control, 5)] public string FontName { get; set; } = "나눔고딕";
        /// <summary>글꼴 스타일</summary>
        [GoProperty(PCategory.Control, 6)] public GoFontStyle FontStyle { get; set; } = GoFontStyle.Normal;
        /// <summary>글꼴 크기</summary>
        [GoProperty(PCategory.Control, 7)] public float FontSize { get; set; } = 12;
        /// <summary>텍스트 영역의 내부 여백</summary>
        [GoProperty(PCategory.Control, 8)] public GoPadding TextPadding { get; set; } = new GoPadding(0, 0, 0, 0);
        /// <summary>텍스트 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 9)] public string TextColor { get; set; } = "Fore";
        /// <summary>라벨 배경 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 10)] public string LabelColor { get; set; } = "Base2";
        /// <summary>테두리 색상 (테마 색상 키)</summary>
        [GoProperty(PCategory.Control, 11)] public string BorderColor { get; set; } = "Base2";
        /// <summary>모서리 둥글기 유형</summary>
        [GoProperty(PCategory.Control, 12)] public GoRoundType Round { get; set; } = GoRoundType.All;
        /// <summary>테두리 두께</summary>
        [GoProperty(PCategory.Control, 13)] public float BorderWidth { get; set; } = 1F;
        /// <summary>배경 그리기 여부</summary>
        [GoProperty(PCategory.Control, 14)] public bool BackgroundDraw { get; set; } = false;
        /// <summary>테두리만 그리기 여부</summary>
        [GoProperty(PCategory.Control, 15)] public bool BorderOnly{ get; set; } = false;
        /// <summary>콘텐츠 정렬 방식</summary>
        [GoProperty(PCategory.Control, 16)] public GoContentAlignment ContentAlignment { get; set; } = GoContentAlignment.MiddleCenter;

        /// <summary>자동 글꼴 크기 설정</summary>
        [GoProperty(PCategory.Control, 17)] public GoAutoFontSize AutoFontSize { get; set; } = GoAutoFontSize.NotUsed;
        /// <summary>자동 아이콘 크기 설정</summary>
        [GoProperty(PCategory.Control, 18)] public GoAutoFontSize AutoIconSize { get; set; } = GoAutoFontSize.NotUsed;

        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cText = thm.ToColor(TextColor);
            var cLabel = thm.ToColor(LabelColor);
            var cBor = thm.ToColor(BorderColor);
            var rts = Areas();
            var rtBox = rts["Content"];
            var rtText = rts["Text"];
            
            if (BackgroundDraw)
            {
                if (BorderOnly)
                    Util.DrawBox(canvas, rtBox, SKColors.Transparent, cBor, Round, thm.Corner, true, BorderWidth);
                else
                    Util.DrawBox(canvas, rtBox, cLabel, cBor, Round, thm.Corner, true, BorderWidth);
            }

            var fsz = Util.FontSize(AutoFontSize, rtText.Height) ?? FontSize;
            var isz = Util.FontSize(AutoIconSize, rtText.Height) ?? IconSize;

            Util.DrawTextIcon(canvas, Text, FontName, FontStyle, fsz, IconString, isz, IconDirection, IconGap, rtText, cText, ContentAlignment);

            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        public override Dictionary<string, SKRect> Areas()
        {
            var dic = base.Areas();

            dic["Text"] = Util.FromRect(dic["Content"], TextPadding);

            return dic;
        }
        #endregion

    }
}
