using Going.UI.Forms.ImageCanvas;
using Going.UI.Tools;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Going.UI.Forms.Components
{
    internal static class GoResources
    {
        private static Dictionary<string, GoImageFolder> Folders = [];

        public static void Load(string? path)
        {
            if (path != null && Directory.Exists(path))
            {
                if (!Folders.ContainsKey(path))
                    Folders[path] = new GoImageFolder(path);
            }
        }

        public static GoImageFolder? Get(string? path)
        {
            GoImageFolder? ret = null;
            if (path != null && Folders.TryGetValue(path, out var folder)) ret = folder;
            return ret;
        }
    }

    internal class GoImageFolder : IDisposable
    {
        #region Properties
        public string ImageFolder { get; }
        public Dictionary<string, List<SKImage>> Images { get; } = [];
        #endregion

        #region Constructor
        public GoImageFolder(string imageFolder)
        {
            ImageFolder = imageFolder;
            Load();
        }
        #endregion

        #region Method
        #region Load
        public void Load()
        {
            try
            {
                if (ImageFolder != null && Directory.Exists(ImageFolder))
                {
                    foreach (var path in Directory.GetFiles(ImageFolder))
                    {
                        string fileName = Path.GetFileName(path.ToLower());

                        var name = Path.GetFileNameWithoutExtension(path);
                        if (Images.ContainsKey(name))
                        {
                            var ls = Images.Keys.Where(x => x.StartsWith(name));
                            var ls2 = ls.Where(x => x.Split('_').Length == 2);
                            var max = ls2.Count() > 0 ? ls2.Max(x => int.TryParse(x.Split('_')[1], out var n) ? n : 0) : 0;
                            AddImage($"{name}_{max + 1}", path);
                        }
                        else AddImage(name, path);
                    }
                }
            }
            catch { }
        }
        #endregion

        #region AddImage
        void AddImage(string name, string path)
        {
            if (!Images.ContainsKey(name) && File.Exists(path))
            {
                var data = File.ReadAllBytes(path);
                var ls = ImageExtractor.ProcessImageFromMemory(data);
                if (ls != null && ls.Count > 0) Images.Add(name, ls);
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            foreach (var v in Images.Values)
                foreach (var v2 in v)
                    v2?.Dispose();
        }
        #endregion
        #endregion
    }
}
