using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    #region attr : InputBoxIgnore
    /// <summary>
    /// 입력 상자에서 해당 필드 또는 프로퍼티를 무시하도록 지정하는 특성입니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class InputBoxIgnoreAttribute : Attribute { }
    #endregion
    #region class : InputBoxTag
    internal class InputBoxTag
    {
        public PropertyInfo? prop { get; set; }
        public InputBoxInfo? attr { get; set; }
    }
    #endregion
    #region class : InputBoxInfo
    /// <summary>
    /// 입력 상자의 추가 설정 정보를 나타내는 클래스입니다.
    /// </summary>
    public class InputBoxInfo
    {
        /// <summary>입력 가능한 최솟값</summary>
        public decimal? Minimum { get; set; } = null;
        /// <summary>입력 가능한 최댓값</summary>
        public decimal? Maximum { get; set; } = null;

        /// <summary>입력 상자의 제목</summary>
        public string? Title { get; set; }
        /// <summary>값 표시 형식 문자열</summary>
        public string? FormatString { get; set; }
        /// <summary>불리언 값이 true일 때 표시할 텍스트</summary>
        public string? OnText { get; set; }
        /// <summary>불리언 값이 false일 때 표시할 텍스트</summary>
        public string? OffText { get; set; }
        /// <summary>선택 목록에 표시할 항목 리스트</summary>
        public List<GoListItem>? Items { get; set; }
    }
    #endregion
}
