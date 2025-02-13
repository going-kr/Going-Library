using Going.UI.Containers;
using Going.UI.Datas;
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
        string Name { get; set; }
        SKRect Bounds { get; set; }
        [JsonIgnore] float X { get; set; }
        [JsonIgnore] float Y { get; set; }
        [JsonIgnore] float Left { get; set; }
        [JsonIgnore] float Top { get; set; }
        [JsonIgnore] float Right { get; set; }
        [JsonIgnore] float Bottom { get; set; }
        [JsonIgnore] float Width { get; set; }
        [JsonIgnore] float Height { get; set; }
        GoPadding Margin { get; set; } 
        bool Fill { get; set; }
        [JsonIgnore] IGoContainer? Parent { get; set; }


        void Draw(SKCanvas canvas);
        void Update();
        void MouseDown(float x, float y, GoMouseButton button);
        void MouseUp(float x, float y, GoMouseButton button);
        void MouseDoubleClick(float x, float y, GoMouseButton button);
        void MouseMove(float x, float y) ;
        void MouseWheel(float x, float y, float delta);
    }
}
