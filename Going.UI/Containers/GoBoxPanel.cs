using Going.UI.Controls;
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

namespace Going.UI.Containers
{
    public class GoBoxPanel : GoContainer
    {
        #region Properties
        [JsonInclude] public override List<IGoControl> Childrens { get; } = [];
        [GoProperty(PCategory.Control, 0)] public string BorderColor { get; set; } = "Base3";
        [GoProperty(PCategory.Control, 1)] public string BoxColor { get; set; } = "Base2";
        [GoProperty(PCategory.Control, 2)] public GoRoundType Round { get; set; } = GoRoundType.All;

        [GoProperty(PCategory.Control, 3)] public bool BackgroundDraw { get; set; } = true;
        [GoProperty(PCategory.Control, 4)] public float BorderSize { get; set; } = 1F;
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoBoxPanel(List<IGoControl> childrens) : this() => Childrens = childrens;
        public GoBoxPanel() { }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas, GoTheme thm)
        {
            var cBox = thm.ToColor(BoxColor);
            var cBorder = thm.ToColor(BorderColor);
            var rts = Areas();
            var rtBox = rts["Content"];

            if (BackgroundDraw) Util.DrawBox(canvas, rtBox, cBox, cBorder, Round, thm.Corner, true, BorderSize);

            base.OnDraw(canvas, thm);
        }
        #endregion
    }
}
