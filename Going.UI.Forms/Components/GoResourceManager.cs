using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Forms.Components
{
    public class GoResourceManager : Component
    {
        private string? sImageFolder = null;
        public string? ImageFolder
        {
            get => sImageFolder; 
            set
            {
                if(sImageFolder != value)
                {
                    sImageFolder = value;
                    GoResources.Load(sImageFolder);
                }
            }
        }

        public List<SKImage> GetImage(string? name)
        {
            var folder = GoResources.Get(ImageFolder);
            if (folder != null && name != null && folder.Images.TryGetValue(name, out var ret)) return ret;
            else return [];
        }
    }
}
