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
    public class GoScalePanel : GoContainer
    {
        #region Properties
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
        [GoProperty(PCategory.Control, 0)] public int? BaseWidth { get; set; }
        [GoProperty(PCategory.Control, 1)] public int? BaseHeight { get; set; }
        [GoProperty(PCategory.Control, 2)] public GoContentAlignment PanelAlignment { get; set; } = GoContentAlignment.MiddleCenter;

        [JsonIgnore] public override SKRect PanelBounds => Design?.DesignMode ?? false ? Areas()["Editor"] : Areas()["Scale"];
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoScalePanel(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoScalePanel() { }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale= rts["Scale"];
            var rtEditor = rts["Editor"];

            if (Design?.DesignMode ?? false)
            {
                var rt = rtEditor;
                rt.Inflate(-0.5F, -0.5F);
                rt.Offset(0.5F, 0.5F);
                using var pe = SKPathEffect.CreateDash([1, 2], 2);
                using var p = new SKPaint { };
                p.IsAntialias = false;
                p.IsStroke = true; p.StrokeWidth = 1; p.Color = thm.Base3;
                p.PathEffect = pe;
                canvas.DrawRect(rt, p);

                using (new SKAutoCanvasRestore(canvas))
                {
                    base.OnDraw(canvas, thm);
                }
            }
            else
            {
                float widthRatio = rtContent.Width / rtScale.Width;
                float heightRatio = rtContent.Height / rtScale.Height;
                float scale = Math.Min(widthRatio, heightRatio);

                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.Scale(scale);
                    base.OnDraw(canvas, thm);
                }
            }
        }

        public override Dictionary<string, SKRect> Areas()
        {
            var rts = base.Areas();
            var (iw, ih) = GetScaledSize();

            var rtContent = rts["Content"];

            float widthRatio = rtContent.Width / iw;
            float heightRatio = rtContent.Height / ih;
            float scale = Math.Min(widthRatio, heightRatio);
            var w = Width / scale;
            var h = Height / scale;

            rts["Scale"] = MathTool.MakeRectangle(Util.FromRect(0, 0, w, h), new SKSize(iw, ih), PanelAlignment);
            rts["Editor"] = MathTool.MakeRectangle(rtContent, new SKSize(BaseWidth ?? Width, BaseHeight ?? Height), GoContentAlignment.MiddleCenter);

            return rts;
        }
        #endregion

        #region 
        #region Mouse
        protected override void OnMouseDown(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];
            var rtEditor = rts["Editor"];
           
            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            if (Design?.DesignMode ?? false)
            {
                 
            }
            else
            {
                x /= scale; 
                y /= scale; 

                base.OnMouseDown(x + ViewPosition.X, y + ViewPosition.Y, button);
            }
        }

        protected override void OnMouseMove(float x, float y)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];
            var rtEditor = rts["Editor"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);


            if (Design?.DesignMode ?? false)
            {
 
            }
            else
            {
                x /= scale; 
                y /= scale; 
                base.OnMouseMove(x + ViewPosition.X, y + ViewPosition.Y);
            }
        }

        protected override void OnMouseUp(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];
            var rtEditor = rts["Editor"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            if (Design?.DesignMode ?? false)
            {
 
            }
            else
            {
                x /= scale; 
                y /= scale; 
                base.OnMouseUp(x + ViewPosition.X, y + ViewPosition.Y, button);
            }
        }
         

        protected override void OnMouseClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];
            var rtEditor = rts["Editor"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            if (Design?.DesignMode ?? false)
            {

            }
            else
            {
                x /= scale;
                y /= scale;

                base.OnMouseClick(x + ViewPosition.X, y + ViewPosition.Y, button);
            }
        }

        protected override void OnMouseDoubleClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];
            var rtEditor = rts["Editor"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);

            if (Design?.DesignMode ?? false)
            {

            }
            else
            {
                x /= scale;
                y /= scale;

                base.OnMouseDoubleClick(x + ViewPosition.X, y + ViewPosition.Y, button);
            }

        }

        protected override void OnMouseLongClick(float x, float y, GoMouseButton button)
        {
            var rts = Areas();
            var rtContent = rts["Content"];
            var rtScale = rts["Scale"];
            var rtEditor = rts["Editor"];

            float widthRatio = rtContent.Width / rtScale.Width;
            float heightRatio = rtContent.Height / rtScale.Height;
            float scale = Math.Min(widthRatio, heightRatio);


            if (Design?.DesignMode ?? false)
            {

            }
            else
            {
                x /= scale;
                y /= scale;

                base.OnMouseLongClick(x + ViewPosition.X, y + ViewPosition.Y, button);
            }
        }
        #endregion
        #endregion

        #region Method
        (float width, float height) GetScaledSize()
        {
            if (!BaseWidth.HasValue && !BaseHeight.HasValue)
                return (Width, Height);

            // 둘 다 지정된 경우 - Base의 종횡비를 따름
            if (BaseWidth.HasValue && BaseHeight.HasValue)
            {
                return (BaseWidth.Value, BaseHeight.Value);
            }

            // 나머지는 원본 종횡비 유지
            float aspectRatio = (float)Width / Height;

            if (BaseWidth.HasValue)
            {
                float scaledWidth = BaseWidth.Value;
                float scaledHeight = scaledWidth / aspectRatio;
                return (scaledWidth, scaledHeight);
            }
            else
            {
                float scaledHeight = BaseHeight!.Value;
                float scaledWidth = scaledHeight * aspectRatio;
                return (scaledWidth, scaledHeight);
            }
        }
        #endregion
    }
}
