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

    internal class IcImageFolder : IDisposable
    {
        #region Const
        const string pagePattern = @"^(?i)(.+)\.Page\.(On|Off)\.(bmp|png|gif)$";
        const string containerPattern = @"^(?i)(.+)\.Container\.(On|Off)\.(bmp|png|gif)$";
        const string miscPattern = @"^(?i)(.+)\.Misc\.(On|Off)\.(bmp|png|gif)$";
        const string statePattern = @"^(?i)(.+)\.State\.(\d+)\.(bmp|png|gif)$";
        const string aniPattern = @"^(?i)(.+)\.Animation\.(\d+)\.(bmp|png|gif)$";
        #endregion

        #region Properties
        public string ImageFolder { get; }
        public Dictionary<string, IcOnOffImage> Pages { get; } = [];
        public Dictionary<string, IcOnOffImage> Containers { get; } = [];
        public Dictionary<string, IcOnOffImage> Miscs { get; } = [];
        public Dictionary<string, Dictionary<int, SKBitmap?>> States { get; } = [];
        public Dictionary<string, List<SKBitmap?>> Animations { get; } = [];
        #endregion

        #region Constructor
        public IcImageFolder(string imageFolder)
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
                    var pages = new Dictionary<string, Dictionary<string, string>>();
                    var containers = new Dictionary<string, Dictionary<string, string>>();
                    var miscs = new Dictionary<string, Dictionary<string, string>>();
                    var states = new Dictionary<string, Dictionary<int, string>>();
                    var anis = new Dictionary<string, Dictionary<int, string>>();

                    #region group
                    var pageRegex = new Regex(pagePattern);
                    var conRegex = new Regex(containerPattern);
                    var miscRegex = new Regex(miscPattern);
                    var aniRegex = new Regex(aniPattern);
                    var stateRegex = new Regex(statePattern);

                    foreach (var path in Directory.GetFiles(ImageFolder))
                    {
                        string fileName = Path.GetFileName(path.ToLower());

                        if (pageRegex.IsMatch(fileName))
                        {
                            var vs = fileName.Split('.');
                            if (vs.Length == 4)
                            {
                                pages.TryAdd(vs[0], []);
                                pages[vs[0]].TryAdd(vs[2], path);
                            }
                        }

                        if (conRegex.IsMatch(fileName))
                        {
                            var vs = fileName.Split('.');
                            if (vs.Length == 4)
                            {
                                containers.TryAdd(vs[0], []);
                                containers[vs[0]].TryAdd(vs[2], path);
                            }
                        }

                        if (miscRegex.IsMatch(fileName))
                        {
                            var vs = fileName.Split('.');
                            if (vs.Length == 4)
                            {
                                miscs.TryAdd(vs[0], []);
                                miscs[vs[0]].TryAdd(vs[2], path);
                            }
                        }

                        if (stateRegex.IsMatch(fileName))
                        {
                            var vs = fileName.Split('.');
                            if (vs.Length == 4)
                            {
                                states.TryAdd(vs[0], []);
                                if (int.TryParse(vs[2], out var n)) states[vs[0]].TryAdd(n, path);
                            }
                        }

                        if (aniRegex.IsMatch(fileName))
                        {
                            var vs = fileName.Split('.');
                            if (vs.Length == 4)
                            {
                                anis.TryAdd(vs[0], []);
                                if (int.TryParse(vs[2], out var n)) anis[vs[0]].TryAdd(n, path);
                            }
                        }
                    }
                    #endregion

                    #region load
                    Pages.Clear();
                    foreach (var pageName in pages.Keys)
                        if (pages[pageName].TryGetValue("on", out var pOn) && pages[pageName].TryGetValue("off", out var pOff))
                            Pages.Add(pageName, new IcOnOffImage(Util.FromBitmap(pOn), Util.FromBitmap(pOff)));

                    Containers.Clear();
                    foreach (var containerName in containers.Keys)
                        if (containers[containerName].TryGetValue("on", out var pOn) && containers[containerName].TryGetValue("off", out var pOff))
                            Containers.Add(containerName, new IcOnOffImage(Util.FromBitmap(pOn), Util.FromBitmap(pOff)));

                    Miscs.Clear();
                    foreach (var imgName in miscs.Keys)
                        if (miscs[imgName].TryGetValue("on", out var pOn) && miscs[imgName].TryGetValue("off", out var pOff))
                            Miscs.Add(imgName, new IcOnOffImage(Util.FromBitmap(pOn), Util.FromBitmap(pOff)));

                    States.Clear();
                    foreach (var stateName in states.Keys)
                    {
                        States.TryAdd(stateName, []);
                        foreach (var state in states[stateName].Keys)
                        {
                            var path = states[stateName][state];
                            States[stateName].TryAdd(state, Util.FromBitmap(path));
                        }
                    }

                    Animations.Clear();
                    foreach (var animationName in anis.Keys)
                    {
                        Animations.TryAdd(animationName, []);
                        foreach (var ani in anis[animationName].Keys.OrderBy(x => x))
                        {
                            var path = anis[animationName][ani];
                            Animations[animationName].Add(Util.FromBitmap(path));
                        }
                    }
                    #endregion
                }
            }
            catch { }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            foreach (var v in Pages.Values) { v.Off?.Dispose(); v.On?.Dispose(); }
            foreach (var v in Containers.Values) { v.Off?.Dispose(); v.On?.Dispose(); }
            foreach (var v in Miscs.Values) { v.On?.Dispose(); v.Off?.Dispose(); }

            foreach (var v in States.Values)
                foreach (var v2 in v.Values)
                    v2?.Dispose();

            foreach (var v in Animations.Values)
                foreach (var v2 in v)
                    v2?.Dispose();
        }
        #endregion
        #endregion
    }

    internal class IcOnOffImage(SKBitmap? on, SKBitmap? off)
    {
        public SKBitmap? On => on;
        public SKBitmap? Off => off;
    }
}
