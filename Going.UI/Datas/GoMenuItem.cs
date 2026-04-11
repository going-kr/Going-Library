using Going.UI.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    /// <summary>
    /// 메뉴 컨트롤에서 사용되는 메뉴 항목 클래스입니다.
    /// </summary>
    public class GoMenuItem
    {
        /// <summary>메뉴 항목에 표시할 아이콘 문자열</summary>
        [GoProperty(PCategory.Basic, 0)] public string? IconString { get; set; }
        /// <summary>메뉴 항목에 표시할 텍스트</summary>
        [GoProperty(PCategory.Basic, 1)] public string? Text { get; set; }
        /// <summary>사용자 정의 데이터를 저장하기 위한 태그 객체</summary>
        public object? Tag { get; set; }
        /// <summary>메뉴 선택 시 이동할 페이지 이름</summary>
        [GoProperty(PCategory.Basic, 2)] public string? PageName { get; set; }

        internal SKRect Bounds { get; set; }

        /// <summary>메뉴 항목의 텍스트와 페이지 이름을 반환합니다.</summary>
        public override string ToString() => $"{Text ?? string.Empty} / Page : {PageName}";
    }
}
