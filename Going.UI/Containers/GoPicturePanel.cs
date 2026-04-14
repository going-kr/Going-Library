using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    /// <summary>
    /// 배경 이미지를 표시하는 패널 컨테이너입니다. 다양한 이미지 스케일 모드를 지원합니다.
    /// </summary>
    public class GoPicturePanel : GoContainer
    {
        #region Properties
        /// <summary>
        /// 자식 컨트롤 목록을 가져옵니다.
        /// </summary>
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
        /// <summary>
        /// 배경 이미지 리소스 이름을 가져오거나 설정합니다.
        /// </summary>
        [GoImageProperty(PCategory.Control, 0)] public string? Image { get; set; }
        /// <summary>
        /// 이미지 스케일 모드를 가져오거나 설정합니다. (Real, CenterImage, Stretch, Zoom)
        /// </summary>
        [GoProperty(PCategory.Control, 1)] public GoImageScaleMode ScaleMode { get; set; } = GoImageScaleMode.Real;
        /// <summary>
        /// 모서리 라운드 타입을 가져오거나 설정합니다.
        /// </summary>
        [GoProperty(PCategory.Control, 2)] public GoRoundType Round { get; set; } = GoRoundType.All;
        #endregion

        #region Member Variable
        SKPath path = new SKPath();
        #endregion

        #region Constructor
        /// <summary>
        /// 자식 컨트롤 목록을 사용하여 <see cref="GoPicturePanel"/>의 새 인스턴스를 초기화합니다. JSON 역직렬화에 사용됩니다.
        /// </summary>
        /// <param name="childrens">자식 컨트롤 목록</param>
        [JsonConstructor]
        public GoPicturePanel(List<IGoControl> childrens) : this() => Childrens = childrens ?? [];
        /// <summary>
        /// <see cref="GoPicturePanel"/>의 새 인스턴스를 초기화합니다.
        /// </summary>
        public GoPicturePanel() { }
        #endregion

        #region Override
        /// <inheritdoc/>
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            if (Image != null && Design != null)
            {
                var ls = Design.GetImage(Image);
                if (ls.Count > 0)
                {
                    var cx = rtContent.MidX;
                    var cy = rtContent.MidY;
                    var img = ls.First();

                    var rt = rtContent;
                    #region bounds
                    switch (ScaleMode)
                    {
                        case GoImageScaleMode.Real:
                            rt = Util.FromRect(rtContent.Left, rtContent.Top, img.Width, img.Height);
                            break;
                        case GoImageScaleMode.CenterImage:
                            rt = Util.FromRect(cx - (img.Width / 2), cy - (img.Height / 2), img.Width, img.Height);
                            break;
                        case GoImageScaleMode.Stretch:
                            rt = rtContent;
                            break;
                        case GoImageScaleMode.Zoom:
                            var ia = (float)img.Width / img.Height;
                            var ta = rtContent.Width / rtContent.Height;
                            float w, h;
                            if (ia > ta) { w = rtContent.Width; h = rtContent.Width / ia; }
                            else { h = rtContent.Height; w = rtContent.Height * ia; }

                            rt = MathTool.MakeRectangle(rtContent, new SKSize(w, h), GoContentAlignment.MiddleCenter);
                            break;
                    }
                    #endregion

                    PathTool.Box(path, rt, Round, thm.Corner);
                    using (new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipPath(path, SKClipOperation.Intersect, true);
                        canvas.DrawImage(img, rt, Util.Sampling);
                    }
                }
            }
            base.OnDraw(canvas, thm);
        }

        /// <inheritdoc/>
        protected override void OnDispose()
        {
            base.OnDispose();

            path.Dispose();
        }
        #endregion
    }
}
