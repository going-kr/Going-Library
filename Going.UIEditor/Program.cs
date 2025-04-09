using Going.UI.Forms.Dialogs;
using Going.UI.Themes;
using Going.UI.Utils;
using Going.UIEditor.Datas;
using Going.UIEditor.Forms;
using Going.UIEditor.Managers;
using SkiaSharp;
using System.Security.Cryptography.X509Certificates;

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
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            GoTheme.Current.Select = GoTheme.Current.ToColor("#007acc");

            DataMgr = new DataManager();

            InputBox = new GoInputBox();
            SelBox = new GoSelectorBox();
            MessageBox = new GoMessageBox { MinimumWidth = 200 };
            SettingForm = new FormSetting();
            NewFileForm = new FormNewFile();
            MainForm = new FormMain();

            Application.Run(MainForm);
        }
    }
}