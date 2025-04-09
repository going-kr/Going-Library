using Going.UIEditor.Tools;
using Going.UIEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Going.UIEditor.Managers
{
    public class DataManager
    {
        #region Const
        const string PATH_SETTING = "setting.json";
        #endregion

        #region Properties 
        public string? ProjectFolder { get; set; }

        #region Language
        private Lang lang = Lang.NONE;
        public Lang Language
        {
            get => lang;
            set
            {
                if (lang != value)
                {
                    lang = value;
                    LanguageChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        #endregion
        #endregion

        #region Event
        public event EventHandler? LanguageChanged;
        #endregion

        #region Constructor
        public DataManager()
        {
            LoadSetting();
        }
        #endregion

        #region Method
        #region SaveSetting
        public void SaveSetting()
        {
            var v = JsonSerializer.Serialize(new Set
            {
                ProjectFolder = this.ProjectFolder,
                Language = this.Language,
            }, JsonOpt.Options);
            File.WriteAllText(PATH_SETTING, v);
        }
        #endregion
        #region LoadSetting
        public void LoadSetting()
        {
            if (File.Exists(PATH_SETTING))
            {
                var set = JsonSerializer.Deserialize<Set>(File.ReadAllText(PATH_SETTING));
                this.ProjectFolder = set?.ProjectFolder;
                this.Language = set?.Language ?? Lang.KO;
            }
            else
            {
                this.ProjectFolder = Path.Combine(Application.StartupPath, "ui_editor");
                this.Language = Lang.KO;
            }
        }
        #endregion
        #endregion
    }

    #region class : Set
    public class Set
    {
        public string? ProjectFolder { get; set; }
        public Lang Language { get; set; } = Lang.NONE;
    }
    #endregion
}
