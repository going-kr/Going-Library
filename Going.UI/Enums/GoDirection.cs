using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Enums
{
    /// <summary>
    /// 네 방향을 정의하는 열거형입니다.
    /// </summary>
    public enum GoDirection
    {
        /// <summary>왼쪽</summary>
        Left,
        /// <summary>오른쪽</summary>
        Right,
        /// <summary>위쪽</summary>
        Up,
        /// <summary>아래쪽</summary>
        Down
    }

    /// <summary>
    /// 수평 또는 수직 방향을 정의하는 열거형입니다.
    /// </summary>
    public enum GoDirectionHV
    {
        /// <summary>수평 방향</summary>
        Horizon,
        /// <summary>수직 방향</summary>
        Vertical
    }

    /// <summary>
    /// 스택 레이아웃의 교차축(cross-axis) 정렬 방식입니다.
    /// </summary>
    public enum GoStackAlignment
    {
        /// <summary>교차축 시작에 정렬(자식 크기 유지). 세로 스택이면 왼쪽, 가로 스택이면 위.</summary>
        Near,
        /// <summary>교차축 가운데 정렬(자식 크기 유지).</summary>
        Center,
        /// <summary>교차축 끝에 정렬(자식 크기 유지). 세로 스택이면 오른쪽, 가로 스택이면 아래.</summary>
        Far,
        /// <summary>교차축을 가득 채움(자식 크기 무시, Margin 인셋).</summary>
        Fill
    }

    /// <summary>
    /// 컨트롤의 도킹 스타일을 정의하는 열거형입니다.
    /// </summary>
    public enum GoDockStyle
    {
        /// <summary>도킹 없음</summary>
        None,
        /// <summary>왼쪽 도킹</summary>
        Left,
        /// <summary>상단 도킹</summary>
        Top,
        /// <summary>오른쪽 도킹</summary>
        Right,
        /// <summary>하단 도킹</summary>
        Bottom,
        /// <summary>전체 영역 채우기</summary>
        Fill
    }
}
