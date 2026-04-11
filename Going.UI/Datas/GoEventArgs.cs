using Going.UI.Enums;
using Going.UI.Themes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    /// <summary>
    /// 마우스 이벤트의 기본 인자 클래스입니다.
    /// </summary>
    /// <param name="x">마우스 X 좌표</param>
    /// <param name="y">마우스 Y 좌표</param>
    public class GoMouseEventArgs(float x, float y) : EventArgs
    {
        /// <summary>마우스 X 좌표</summary>
        public float X { get; private set; } = x;
        /// <summary>마우스 Y 좌표</summary>
        public float Y { get; private set; } = y;
    }

    /// <summary>
    /// 마우스 클릭 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="x">마우스 X 좌표</param>
    /// <param name="y">마우스 Y 좌표</param>
    /// <param name="button">클릭된 마우스 버튼</param>
    public class GoMouseClickEventArgs(float x, float y, GoMouseButton button) : GoMouseEventArgs(x, y)
    {
        /// <summary>클릭된 마우스 버튼</summary>
        public GoMouseButton Button { get; private set; } = button;
    }

    /// <summary>
    /// 마우스 휠 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="x">마우스 X 좌표</param>
    /// <param name="y">마우스 Y 좌표</param>
    /// <param name="delta">휠 스크롤 양</param>
    public class GoMouseWheelEventArgs(float x, float y, float delta) : GoMouseEventArgs(x, y)
    {
        /// <summary>휠 스크롤 양 (양수: 위로, 음수: 아래로)</summary>
        public float Delta { get; private set; } = delta;
    }

    /// <summary>
    /// 취소 가능한 이벤트 인자 클래스입니다.
    /// </summary>
    public class GoCancelableEventArgs : EventArgs
    {
        /// <summary>이벤트를 취소할지 여부</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// 드래그 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="x">드래그 위치의 X 좌표</param>
    /// <param name="y">드래그 위치의 Y 좌표</param>
    /// <param name="dragItem">드래그 중인 객체</param>
    public class GoDragEventArgs(float x, float y, object dragItem) : EventArgs
    {
        /// <summary>드래그 위치의 X 좌표</summary>
        public float X { get; private set; } = x;
        /// <summary>드래그 위치의 Y 좌표</summary>
        public float Y { get; private set; } = y;
        /// <summary>드래그 중인 객체</summary>
        public object DragItem { get; private set; } = dragItem;
    }

    /// <summary>
    /// 컨트롤 그리기 완료 이벤트 인자 클래스입니다.
    /// </summary>
    /// <param name="canvas">그리기에 사용된 SKCanvas</param>
    /// <param name="thm">현재 적용된 테마</param>
    public class GoDrawnEventArgs(SKCanvas canvas, GoTheme thm) : EventArgs
    {
        /// <summary>현재 적용된 테마</summary>
        public GoTheme Theme { get; private set; } = thm;
        /// <summary>그리기에 사용된 SKCanvas</summary>
        public SKCanvas Canvas { get; private set; } = canvas;
    }
}
