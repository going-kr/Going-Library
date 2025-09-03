using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UIEditor.Utils
{
    public class FontPath
    {
        public static bool IsLoaded => task?.IsCompletedSuccessfully == true;

        static Dictionary<string, List<string>> Fonts = new(StringComparer.OrdinalIgnoreCase);
        static Task? task;

        public static void Build()
        {
            if (!IsLoaded && task == null)
                task = Task.Run(() =>
                {
                    var path1 = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                    var path2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts");

                    var files = new List<string>();
                    files.AddRange(Directory.GetFiles(path1, "*.ttf", SearchOption.TopDirectoryOnly));
                    files.AddRange(Directory.GetFiles(path1, "*.otf", SearchOption.TopDirectoryOnly));
                    files.AddRange(Directory.GetFiles(path2, "*.ttf", SearchOption.TopDirectoryOnly));
                    files.AddRange(Directory.GetFiles(path2, "*.otf", SearchOption.TopDirectoryOnly));

                    foreach (var f in files)
                    {
                        try
                        {
                            using var pfc = new PrivateFontCollection();
                            pfc.AddFontFile(f);
                            if (pfc.Families.Length == 0) continue;

                            var family = pfc.Families[0].Name;
                            lock (Fonts)
                            {
                                if (!Fonts.ContainsKey(family)) Fonts[family] = [];
                                Fonts[family].Add(f);
                            }
                        }
                        catch { }
                    }
                });
        }

        public static List<string> GetFontPaths(string fontName)
        {
            lock (Fonts)
            {
                if (Fonts.TryGetValue(fontName, out var paths))
                    return paths;
            }
            return [];
        }

        public static bool IsContainsFont(string fontName)
        {
            lock (Fonts)
            {
                return Fonts.ContainsKey(fontName);
            }
        }
    }
}
