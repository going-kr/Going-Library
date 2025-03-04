 using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Going.UI.Forms.Containers;
using Going.UI.Utils;
using Going.UI.Forms.Controls;
using System.Reflection;
using SkiaSharp;
using System.Text.RegularExpressions;
using Going.UI.Themes;
using SkiaSharp.Views.Desktop;
using System.Runtime.InteropServices;

namespace Going.UI.Forms.ImageCanvas
{
    public class IcCanvas : TabControl
    {
        #region Const
        const string pagePattern = @"^(?i)(.+)\.Page\.(On|Off)\.(bmp|png|gif)$";
        const string statePattern = @"^(?i)(.+)\.State\.(\d+)\.(bmp|png|gif)$";
        const string aniPattern = @"^(?i)(.+)\.Ani\.(\d+)\.(bmp|png|gif)$";
        const string imagePattern = @"^(?i)(.+)\.Image\.(On|Off)\.(bmp|png|gif)$";
        #endregion

        #region Properties
        [Editor("System.Windows.Forms.Design.FolderNameEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
        public string? ImageFolder
        {
            get => sImageFolder;
            set
            {
                if (sImageFolder != value)
                {
                    sImageFolder = value;
                    IcResources.Load(sImageFolder);
                    Invalidate();
                }
            }
        }

        public string BackgroundColor { get; set; } = "Back";

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new TabPage SelectedTab { get => base.SelectedTab; set { base.SelectedTab = value; } }
        #endregion

        #region Member Variable
        private string? sImageFolder;
        #endregion

        #region Constructor
        public IcCanvas()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            this.Appearance = TabAppearance.FlatButtons;
            this.ItemSize = new Size(0, 1);
            this.SizeMode = TabSizeMode.Fixed;

            this.Multiline = false;
        }
        #endregion

        #region Override
        #region OnControlAdd
        protected override void OnControlAdded(ControlEventArgs e)
        {
            if (e.Control is TabPage page)
            {
                var prop = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
                prop?.SetValue(page, true, null);

                page.MouseUp += (o, s) => ((Control?)o)?.Invalidate();
                page.MouseDown += (o, s) => ((Control?)o)?.Invalidate();
                page.Paint += (o, s) =>
                {
                    var name = ((Control?)o)?.Name;

                    using (var bitmap = new SKBitmap(this.Width, this.Height))
                    using (var canvas = new SKCanvas(bitmap))
                    using (var surface = SKSurface.Create(bitmap.Info))
                    {
                        var cBack = GoTheme.Current.ToColor(BackgroundColor);
                        canvas.Clear(cBack);

                        var ip = IcResources.Get(ImageFolder);
                        if (ip != null)
                        {
                            using var p = new SKPaint { IsAntialias = true, Color = SKColors.Black.WithAlpha(Convert.ToByte(Enabled ? 255 : 255 - GoTheme.DisableAlpha)) };
                            var sp = canvas.SaveLayer(p);
                            if (name != null && ip.Pages.TryGetValue(name, out var vp) && vp.On != null && vp.Off != null)
                                canvas.DrawBitmap(vp.Off, Util.FromRect(0, 0, Width, Height));
                            canvas.RestoreToCount(sp);
                        }

                        using (var bmp = bitmap.ToBitmap()) s.Graphics.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    }

                };
            }

            base.OnControlAdded(e);
        }
        #endregion

        #region WndProc
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1328) return;
            base.WndProc(ref m);
        }
        #endregion
        #endregion
    }
}
