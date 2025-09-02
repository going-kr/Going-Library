using Going.UI.Forms.Controls;
using Going.UI.Forms.Tools;
using Going.UI.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Containers
{
    public class GoScrollablePanel : GoContainer 
    {
        #region Interop
        [DllImport("uxtheme", CharSet = CharSet.Unicode)]
        static extern Int32 SetWindowTheme(IntPtr hWnd, string subAppName, string subIdList);
        #endregion

        #region Member Variable
        bool bFirst = true;
        #endregion

        #region Constructor
        public GoScrollablePanel()
        {
            AutoScroll = true;

            DwmTool.SetDarkMode(Handle, GoThemeW.Current.Dark);
        }
        #endregion

        #region Override
        #region OnContentDraw
        protected override void OnPaint(PaintEventArgs e)
        {
            if (bFirst)
            {
                DwmTool.SetDarkMode(Handle, GoThemeW.Current.Dark);

                bFirst = false;
            }
            base.OnPaint(e);
        }
        #endregion
        #region OnVisibleChanged
        protected override void OnVisibleChanged(EventArgs e)
        {
            DwmTool.SetDarkMode(this.Handle, GoThemeW.Current.Dark);
            base.OnVisibleChanged(e);
        }
        #endregion
        #endregion
    }
}
