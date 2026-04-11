using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Themes;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    /// <summary>
    /// 이미지 캔버스용 페이지 컨트롤입니다. Off/On 이미지를 배경으로 사용하며 자식 컨트롤을 포함할 수 있습니다.
    /// </summary>
    public class IcPage : GoPage
    {
        /// <summary>
        /// 배경 색상을 가져오거나 설정합니다. 이미지가 없을 때 사용됩니다.
        /// </summary>
        [GoProperty(PCategory.Control, 0)] public string BackgroundColor { get; set; } = "Back";
        /// <summary>
        /// 비활성(Off) 상태 배경 이미지 리소스 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoImageProperty(PCategory.Control, 1)] public string? OffImage { get; set; }
        /// <summary>
        /// 활성(On) 상태 배경 이미지 리소스 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoImageProperty(PCategory.Control, 2)] public string? OnImage { get; set; }

        #region Constructor
        /// <summary>
        /// 자식 컨트롤 목록을 사용하여 <see cref="IcPage"/>의 새 인스턴스를 초기화합니다. JSON 역직렬화에 사용됩니다.
        /// </summary>
        /// <param name="childrens">자식 컨트롤 목록</param>
        [JsonConstructor]
        public IcPage(List<IGoControl> childrens) : base(childrens) { }
        /// <summary>
        /// <see cref="IcPage"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public IcPage() { }
        #endregion

        protected override void OnBackgroundDraw(SKCanvas canvas, GoTheme thm)
        {
            using var p = new SKPaint { };

            var rts = Areas();
            var rtContent = rts["Content"];

            if (Design != null && Design.GetImage(OffImage) is List<SKImage> imgs && imgs.Count > 0)
            {
                canvas.DrawImage(imgs.First(), rtContent, Util.Sampling);
            }
            else
            {
                p.IsStroke = false;
                p.Color = thm.ToColor(BackgroundColor);
                canvas.DrawRect(rtContent, p);
            }
        }
    }

}
