using Going.UI.Containers;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Enums;
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
    public interface IGoControl
    {
        /// <summary>
        /// Id는 컨트롤의 고유 식별자입니다.
        /// Name은 거의 사용되지 않습니다.
        /// Parent는 부모 컨테이너를 나타냅니다.
        /// Design은 컨트롤이 속한 모든 디자인을 나타냅니다. - json에 엮이면 안됨(순환참조)
        /// ScreenX, ScreenY는 화면 전체를 기준으로 한 좌표입니다.
        /// Fire는 아까 GoDesign에서 설명한 이벤트를 발생시키는 메서드입니다.
        /// </summary>
        Guid Id { get; init; }
        string? Name { get; set; }
        SKRect Bounds { get; set; }
        GoPadding Margin { get; set; }
        bool Fill { get; set; }
        bool Visible { get; set; }
        bool Enabled { get; set; }

        [JsonIgnore] float X { get; set; }
        [JsonIgnore] float Y { get; set; }
        [JsonIgnore] float Left { get; set; }
        [JsonIgnore] float Top { get; set; }
        [JsonIgnore] float Right { get; set; }
        [JsonIgnore] float Bottom { get; set; }
        [JsonIgnore] float Width { get; set; }
        [JsonIgnore] float Height { get; set; }
        [JsonIgnore] bool FirstRender { get; }
        [JsonIgnore] IGoContainer? Parent { get; }
        [JsonIgnore] GoDesign? Design { get; }
        [JsonIgnore] float ScreenX { get; }
        [JsonIgnore] float ScreenY { get; }

        void FireInit(GoDesign? design);
        void FireDraw(SKCanvas canvas);
        void FireUpdate();
        void FireMouseDown(float x, float y, GoMouseButton button);
        void FireMouseUp(float x, float y, GoMouseButton button);
        void FireMouseDoubleClick(float x, float y, GoMouseButton button);
        void FireMouseMove(float x, float y) ;
        void FireMouseWheel(float x, float y, float delta);
        void FireKeyDown(bool Shift, bool Control, bool Alt, GoKeys key) { }
        void FireKeyUp(bool Shift, bool Control, bool Alt, GoKeys key) { }

    }
}
