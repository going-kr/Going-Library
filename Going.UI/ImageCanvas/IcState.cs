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
        public string? StateImage { get; set; }
        public int State { get; set; }
        #endregion

        #region Member Variable
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            var thm = GoTheme.Current;
            var rts = Areas();
            var rtBox = rts["Content"];

            var (ip, img, cBack) = vars();
            if (ip != null && img != null && Parent != null && img.On != null && img.Off != null)
            {
                if (StateImage != null && ip.States.TryGetValue(StateImage, out var dic) && dic.TryGetValue(State, out var bm) && bm != null)
                    canvas.DrawBitmap(bm, rtBox);
            }

            base.OnDraw(canvas);
        }
        #endregion

        #region Method
        #region vars
        (IcImageFolder? ip, IcOnOffImage? img, SKColor cBack) vars()
        {
            var thm = GoTheme.Current;
            SKColor cBack = thm.Back;
            IcImageFolder? ip = null;
            IcOnOffImage? img = null;

            if (Design != null)
            {
                ip = Design.GetIC();
                if (Parent is IcContainer con)
                {
                    cBack = thm.ToColor(con.BackgroundColor);
                    img = ip != null && con.ContainerImage != null && ip.Containers.TryGetValue(con.ContainerImage, out var v) ? v : null;
                }
                else if (Parent is IcPage page)
                {
                    cBack = thm.ToColor(page.BackgroundColor);
                    img = ip != null && page.Name != null && ip.Pages.TryGetValue(page.Name, out var v) ? v : null;
                }
            }

            return (ip, img, cBack);
        }
        #endregion
        #endregion
    }
}
