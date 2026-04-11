using Going.UI.Containers;
using Going.UI.Datas;
using Going.UI.Design;
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

namespace Going.UI.Controls
{
    /// <summary>
    /// Going.UI 컨트롤의 공통 인터페이스. 위치, 크기, 이벤트 처리 등의 기본 계약을 정의합니다.
    /// </summary>
    public interface IGoControl
    {
        /// <summary>컨트롤의 고유 식별자</summary>
        Guid Id { get; init; }
        /// <summary>컨트롤 이름</summary>
        string? Name { get; set; }
        /// <summary>컨트롤의 경계 영역</summary>
        SKRect Bounds { get; set; }
        /// <summary>컨트롤의 외부 여백</summary>
        GoPadding Margin { get; set; }
        /// <summary>컨트롤의 도킹 스타일</summary>
        GoDockStyle Dock { get; set; }
        /// <summary>컨트롤 표시 여부</summary>
        bool Visible { get; set; }
        /// <summary>컨트롤 활성화 여부</summary>
        bool Enabled { get; set; }

        /// <summary>컨트롤의 X 좌표</summary>
        [JsonIgnore] float X { get; set; }
        /// <summary>컨트롤의 Y 좌표</summary>
        [JsonIgnore] float Y { get; set; }
        /// <summary>컨트롤 왼쪽 좌표</summary>
        [JsonIgnore] float Left { get; set; }
        /// <summary>컨트롤 위쪽 좌표</summary>
        [JsonIgnore] float Top { get; set; }
        /// <summary>컨트롤 오른쪽 좌표</summary>
        [JsonIgnore] float Right { get; set; }
        /// <summary>컨트롤 아래쪽 좌표</summary>
        [JsonIgnore] float Bottom { get; set; }
        /// <summary>컨트롤 너비</summary>
        [JsonIgnore] float Width { get; set; }
        /// <summary>컨트롤 높이</summary>
        [JsonIgnore] float Height { get; set; }
        /// <summary>첫 번째 렌더링 여부</summary>
        [JsonIgnore] bool FirstRender { get; }
        /// <summary>부모 컨테이너</summary>
        [JsonIgnore] IGoContainer? Parent { get; }
        /// <summary>디자인 편집기 객체</summary>
        [JsonIgnore] GoDesign? Design { get; }
        /// <summary>화면 기준 X 좌표</summary>
        [JsonIgnore] float ScreenX { get; }
        /// <summary>화면 기준 Y 좌표</summary>
        [JsonIgnore] float ScreenY { get; }

        /// <summary>컨트롤을 초기화합니다.</summary>
        /// <param name="design">디자인 편집기 객체</param>
        void FireInit(GoDesign? design);
        /// <summary>컨트롤을 그립니다.</summary>
        /// <param name="canvas">SkiaSharp 캔버스</param>
        /// <param name="thm">테마</param>
        void FireDraw(SKCanvas canvas, GoTheme thm);
        /// <summary>컨트롤 상태를 업데이트합니다.</summary>
        void FireUpdate();
        /// <summary>컨트롤이 표시될 때 호출합니다.</summary>
        void FireShow();
        /// <summary>컨트롤이 숨겨질 때 호출합니다.</summary>
        void FireHide();
        /// <summary>마우스 버튼 누름 이벤트를 발생시킵니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">눌린 마우스 버튼</param>
        void FireMouseDown(float x, float y, GoMouseButton button);
        /// <summary>마우스 버튼 놓음 이벤트를 발생시킵니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">놓은 마우스 버튼</param>
        void FireMouseUp(float x, float y, GoMouseButton button);
        /// <summary>마우스 더블클릭 이벤트를 발생시킵니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="button">클릭한 마우스 버튼</param>
        void FireMouseDoubleClick(float x, float y, GoMouseButton button);
        /// <summary>마우스 이동 이벤트를 발생시킵니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        void FireMouseMove(float x, float y) ;
        /// <summary>마우스 휠 이벤트를 발생시킵니다.</summary>
        /// <param name="x">마우스 X 좌표</param>
        /// <param name="y">마우스 Y 좌표</param>
        /// <param name="delta">휠 스크롤 량</param>
        void FireMouseWheel(float x, float y, float delta);
        /// <summary>키 누름 이벤트를 발생시킵니다.</summary>
        /// <param name="Shift">Shift 키 눌림 여부</param>
        /// <param name="Control">Control 키 눌림 여부</param>
        /// <param name="Alt">Alt 키 눌림 여부</param>
        /// <param name="key">눌린 키</param>
        void FireKeyDown(bool Shift, bool Control, bool Alt, GoKeys key) { }
        /// <summary>키 놓음 이벤트를 발생시킵니다.</summary>
        /// <param name="Shift">Shift 키 눌림 여부</param>
        /// <param name="Control">Control 키 눌림 여부</param>
        /// <param name="Alt">Alt 키 눌림 여부</param>
        /// <param name="key">놓은 키</param>
        void FireKeyUp(bool Shift, bool Control, bool Alt, GoKeys key) { }
        /// <summary>리소스를 해제합니다.</summary>
        void Dispose();
    }
}
