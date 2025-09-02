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
        void FireShow();
        void FireHide();
        void FireMouseDown(float x, float y, GoMouseButton button);
        void FireMouseUp(float x, float y, GoMouseButton button);
        void FireMouseDoubleClick(float x, float y, GoMouseButton button);
        void FireMouseMove(float x, float y) ;
        void FireMouseWheel(float x, float y, float delta);
        void FireKeyDown(bool Shift, bool Control, bool Alt, GoKeys key) { }
        void FireKeyUp(bool Shift, bool Control, bool Alt, GoKeys key) { }

    }
}
