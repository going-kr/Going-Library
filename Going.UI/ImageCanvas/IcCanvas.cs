using Going.UI.Containers;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.ImageCanvas
{
    public class IcCanvas : GoContainer
    {
        #region Properties
        public string BackgroundColor { get; set; } = "White";

        public string? ImageFolder { get; set; }
        public string? OnImage { get; set; }
        public string? OffImage { get; set; }

        [JsonIgnore, Browsable(false)] public SKBitmap? On => bmOn;
        [JsonIgnore, Browsable(false)] public SKBitmap? Off => bmOff;
        #endregion

        #region Member Variable
        SKBitmap? bmOff = null, bmOn = null;
        #endregion

        #region Constructor
        public IcCanvas()
        {

        }
        #endregion

        #region Override
        protected override void OnDraw(SKCanvas canvas)
        {
            if (FirstRender) Load();

            if (bmOff != null)
                canvas.DrawBitmap(bmOff, new SKRect(0, 0, Width, Height));

            base.OnDraw(canvas);
        }
        #endregion

        #region Load
        void Load()
        {
            if (Directory.Exists(ImageFolder) && OnImage != null && OffImage != null)
            {
                var nmOn = Path.Combine(ImageFolder, OnImage);
                var nmOff = Path.Combine(ImageFolder, OffImage);

                if (File.Exists(nmOn)) bmOn = Util.FromBitmap(nmOn);
                if (File.Exists(nmOff)) bmOff = Util.FromBitmap(nmOff);
            }
        }
        #endregion
    }
}
