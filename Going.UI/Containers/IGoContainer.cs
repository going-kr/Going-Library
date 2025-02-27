using Going.UI.Controls;
using SkiaSharp;
using System.Text.Json.Serialization;

// 컨테이너는 자식을 가질 수 있는 UI 요소입니다.
// 영역을 지정할 수 있는 Bounds 속성을 가지고 있습니다.(영역)
namespace Going.UI.Containers
{
    public interface IGoContainer 
    {
        IEnumerable<IGoControl> Childrens { get; }
        SKRect Bounds { get; set; }

        [JsonIgnore] float ViewX { get; }
        [JsonIgnore] float ViewY { get; }
        [JsonIgnore] float ViewWidth { get; }
        [JsonIgnore] float ViewHeight { get; }
    }

}
