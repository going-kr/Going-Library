using Going.UI.Controls;
using Going.UI.Forms;
using Going.UI.Forms.Dialogs;
using Going.UI.Themes;
using Going.UI.Utils;
using Going.UIEditor.Datas;
using Going.UIEditor.Forms;
using Going.UIEditor.Forms.Dialogs;
using Going.UIEditor.Managers;
using SkiaSharp;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;

namespace Going.UIEditor
{
    internal static class Program
    {
        public static Project? CurrentProject { get; set; }

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

        [STAThread]
        static void Main()
        {
 
            ApplicationConfiguration.Initialize();

            GoThemeW.Current.Select = GoThemeW.Current.ToColor("#007acc");
            GoThemeW.Current.Highlight = GoThemeW.Current.Select;

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
            MainForm = new FormMain();

            Application.Run(MainForm);
        }
    }
}