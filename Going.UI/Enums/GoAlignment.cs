using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Enums
{
    /// <summary>
    /// 수직 정렬 방식을 정의하는 열거형입니다.
    /// </summary>
    public enum GoVerticalAlignment
    {
        /// <summary>상단 정렬</summary>
        Top,
        /// <summary>중앙 정렬</summary>
        Middle,
        /// <summary>하단 정렬</summary>
        Bottom
    }

    /// <summary>
    /// 수평 정렬 방식을 정의하는 열거형입니다.
    /// </summary>
    public enum GoHorizonAlignment
    {
        /// <summary>왼쪽 정렬</summary>
        Left,
        /// <summary>가운데 정렬</summary>
        Center,
        /// <summary>오른쪽 정렬</summary>
        Right
    }

    /// <summary>
    /// 콘텐츠의 수직 및 수평 정렬 방식을 조합하여 정의하는 열거형입니다.
    /// </summary>
    public enum GoContentAlignment
    {
        /// <summary>상단 왼쪽 정렬</summary>
        TopLeft,
        /// <summary>상단 가운데 정렬</summary>
        TopCenter,
        /// <summary>상단 오른쪽 정렬</summary>
        TopRight,
        /// <summary>중앙 왼쪽 정렬</summary>
        MiddleLeft,
        /// <summary>중앙 가운데 정렬</summary>
        MiddleCenter,
        /// <summary>중앙 오른쪽 정렬</summary>
        MiddleRight,
        /// <summary>하단 왼쪽 정렬</summary>
        BottomLeft,
        /// <summary>하단 가운데 정렬</summary>
        BottomCenter,
        /// <summary>하단 오른쪽 정렬</summary>
        BottomRight
    }
}
