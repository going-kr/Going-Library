using Going.UI.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    /// <summary>
    /// 리스트 컨트롤에서 사용되는 항목 클래스입니다.
    /// </summary>
    public class GoListItem
    {
        /// <summary>항목에 표시할 아이콘 문자열</summary>
        [GoProperty(PCategory.Basic, 0)] public string? IconString { get; set; }
        /// <summary>항목에 표시할 텍스트</summary>
        [GoProperty(PCategory.Basic, 1)] public string? Text { get; set; }
        /// <summary>사용자 정의 데이터를 저장하기 위한 태그 객체</summary>
        public object? Tag { get; set; }

        internal SKRect Bounds { get; set; }

        /// <summary>항목의 텍스트를 반환합니다.</summary>
        public override string ToString() => Text ?? string.Empty;
    }

    /// <summary>
    /// 리스트 항목 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="itm">이벤트가 발생한 리스트 항목</param>
    public class ListItemEventArgs(GoListItem itm) : EventArgs
    {
        /// <summary>이벤트가 발생한 리스트 항목</summary>
        public GoListItem Item { get; } = itm;
    }
}
