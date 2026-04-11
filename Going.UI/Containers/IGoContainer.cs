using Going.UI.Controls;
using SkiaSharp;
using System.Text.Json.Serialization;

// 컨테이너는 자식을 가질 수 있는 UI 요소입니다.
// 영역을 지정할 수 있는 Bounds 속성을 가지고 있습니다.(영역)
namespace Going.UI.Containers
{
    /// <summary>
    /// 자식 컨트롤을 포함할 수 있는 컨테이너의 인터페이스입니다. 영역(Bounds)과 패널 영역, 뷰 위치 정보를 제공합니다.
    /// </summary>
    public interface IGoContainer
    {
        /// <summary>
        /// 자식 컨트롤 컬렉션을 가져옵니다.
        /// </summary>
        IEnumerable<IGoControl> Childrens { get; }
        /// <summary>
        /// 컨테이너의 전체 영역을 가져오거나 설정합니다.
        /// </summary>
        SKRect Bounds { get; set; }

        /// <summary>
        /// 자식 컨트롤이 배치되는 패널 영역을 가져옵니다.
        /// </summary>
        [JsonIgnore] SKRect PanelBounds { get; }
        /// <summary>
        /// 현재 뷰의 스크롤 오프셋 위치를 가져옵니다.
        /// </summary>
        [JsonIgnore] SKPoint ViewPosition { get; }
    }

}
