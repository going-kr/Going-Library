using Going.UI.ImageCanvas;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Going.UI.Forms.ImageCanvas
{
    internal static class IcResources
    {
        private static Dictionary<string, IcImageFolder> Folders = [];

        public static void Load(string? path)
        {
            if (path != null && Directory.Exists(path))
            {
                if (!Folders.ContainsKey(path))
                    Folders[path] = new IcImageFolder(path);
            }
        }

        public static IcImageFolder? Get(string? path)
        {
            IcImageFolder? ret = null;
            if (path != null && Folders.TryGetValue(path, out var folder)) ret = folder;
            return ret;
        }
    }
}
