using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Design
{
    public class GoPage : GoContainer
    {
        #region Properties
        [JsonInclude]
        public override List<IGoControl> Childrens { get; } = [];
        public string? BackgroundImage { get; set; }
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoPage(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoPage() { }
        #endregion

        #region Override
        #region Draw
        protected override void OnDraw(SKCanvas canvas)
        {
            OnBackgroundDraw(canvas);

            base.OnDraw(canvas);
        }

        protected virtual void OnBackgroundDraw(SKCanvas canvas)
        {
            var rts = Areas();
            var rtContent = rts["Content"];

            if (Design != null && BackgroundImage != null)
            {
                var bg = Design.GetImage(BackgroundImage);
                if (bg != null && bg.Count > 0) canvas.DrawImage(bg[0], rtContent, Util.Sampling);
            }
        }
        #endregion
        #endregion
    }
}
