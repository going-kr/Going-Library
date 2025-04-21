using Going.UI.Forms.Components;
using Going.UI.ImageCanvas;
using Going.UI.Tools;
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
        private static Dictionary<string, IcFolder> Folders = [];

        public static void Load(string? path)
        {
            if (path != null && Directory.Exists(path))
            {
                if (!Folders.ContainsKey(path))
                {
                    Folders[path] = new IcFolder(path);
                }
            }
        }

        public static IcFolder? Get(string? path) => path != null && Folders.TryGetValue(path, out var ic) ? ic : null;
    }

    public class IcFolder
    {
        #region Properties
        public string Folder { get; }
        private Dictionary<string, List<SKImage>> Images { get; } = [];
        #endregion

        #region Constructor
        public IcFolder(string dir)
        {
            Folder = dir;

            if (Directory.Exists(Folder))
            {
                foreach (var path in Directory.GetFiles(Folder))
                {
                    var data = File.ReadAllBytes(path);
                    var name = Path.GetFileNameWithoutExtension(path);
                    if (Images.ContainsKey(name))
                    {
                        var ls = Images.Keys.Where(x => x.StartsWith(name));
                        var ls2 = ls.Where(x => x.Split('_').Length == 2);
                        var max = ls2.Any() ? ls2.Max(x => int.TryParse(x.Split('_')[1], out var n) ? n : 0) : 0;
                        Add($"{name}_{max + 1}", data);
                    }
                    else Add(name, data);
                }
            }
        }
        #endregion

        #region Method
        private void Add(string name, byte[] data)
        {
            var nm = name.ToLower();
            if (!Images.ContainsKey(nm))
            {
                var ls = ImageExtractor.ProcessImageFromMemory(data);
                if (ls != null && ls.Count > 0)
                    Images.Add(nm, ls);
            }
        }

        public List<(string name, List<SKImage> images)> GetImages() => Images.Select(x => (x.Key, x.Value)).ToList();
        public List<SKImage> GetImage(string? name) => name != null && Images.TryGetValue(name.ToLower(), out var ls) ? ls : [];
        #endregion
    }

}
