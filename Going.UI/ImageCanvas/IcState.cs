using Going.UI.Controls;
using Going.UI.Enums;
using Going.UI.Themes;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    public class IcState : GoControl
    {
        #region Properties
        [GoProperty(PCategory.Control, 0)] public List<StateImage> StateImages { get; set; } = [];
        [GoProperty(PCategory.Control, 1)] public int State { get; set; }
        #endregion

        #region Member Variable
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var rts = Areas();
            var rtBox = rts["Content"];

            if (Design != null && Parent != null)
            {
                var simg = StateImages.FirstOrDefault(x=>x.State == State);
                var img = Design.GetImage(simg?.Image)?.FirstOrDefault();
                if (img != null) canvas.DrawImage(img, rtBox, Util.Sampling);
            }

            base.OnDraw(canvas);
        }
        #endregion
    }

    public class StateImage
    {
        [GoImageProperty(PCategory.Control, 0)] public string? Image { get; set; }
        [GoProperty(PCategory.Control, 1)] public int State { get; set; }
    }
}
