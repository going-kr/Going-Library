using Going.UI.Controls;
using Going.UI.Design;
using Going.UI.Forms;
using Going.UI.Forms.Dialogs;
using Going.UI.Themes;
using Going.UI.Utils;
using Going.UIEditor.Forms;
using Going.UIEditor.Forms.Dialogs;
using Going.UIEditor.Forms.Editors;
using Going.UIEditor.Managers;
using Going.UIEditor.Utils;
using SkiaSharp;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;

namespace Going.UIEditor
{
    internal static class Program
    {
        public static GoDesign? CurrentDesign { get; set; }
        public static string? FilePath { get; set; }
        public static bool Edit { get; set; }
        public static bool ClaudeInstalled { get; private set; }
        public static string? ClaudePath { get; private set; }

        public static DataManager DataMgr { get; set; }
        public static GoInputBox InputBox { get; set; }
        public static GoMessageBox MessageBox { get; set; }
        public static GoSelectorBox SelBox { get; set; }
        public static FormMain MainForm { get; set; }
        public static FormSetting SettingForm { get; set; }
        public static FormNewFile NewFileForm { get; set; }
        public static FormNewPage NewPageForm { get; set; }
        public static FormResourceManager ResourceForm { get; set; }
        public static FormImageSelector ImageSelector { get; set; }
        public static FormMultiLineString MultiLineInputor { get; set; }
        public static FormTheme ThemeForm { get; set; }
        public static FormFontAdd FontAdder { get; set; }
        public static FormFontSelector FontSelector { get; set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.DpiUnawareGdiScaled);

            GoThemeW.Current.Select = GoThemeW.Current.ToColor("#007acc");
            GoThemeW.Current.Highlight = GoThemeW.Current.Select;
            FontPath.Build();

            ClaudePath = FindClaude();
            ClaudeInstalled = ClaudePath != null;

            DataMgr = new DataManager();

            InputBox = new GoInputBox();
            SelBox = new GoSelectorBox();
            MessageBox = new GoMessageBox { MinimumWidth = 200 };
            SettingForm = new FormSetting();
            NewFileForm = new FormNewFile();
            NewPageForm = new FormNewPage();
            ResourceForm = new FormResourceManager();
            ImageSelector = new FormImageSelector();
            MultiLineInputor = new FormMultiLineString();
            ThemeForm = new FormTheme();
            FontAdder = new FormFontAdd();
            FontSelector = new FormFontSelector();
            MainForm = new FormMain();

            Application.Run(MainForm);
        }

        static string? FindClaude()
        {
            var wingetPkgs = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft", "WinGet", "Packages");
            if (Directory.Exists(wingetPkgs))
            {
                foreach (var d in Directory.GetDirectories(wingetPkgs, "Anthropic.ClaudeCode*"))
                {
                    var exe = Path.Combine(d, "claude.exe");
                    if (File.Exists(exe)) return exe;
                }
            }

            var pathDirs = Environment.GetEnvironmentVariable("PATH")?.Split(';') ?? [];
            foreach (var d in pathDirs)
            {
                var p = Path.Combine(d, "claude.exe");
                if (File.Exists(p)) return p;
            }

            return null;
        }
    }
}