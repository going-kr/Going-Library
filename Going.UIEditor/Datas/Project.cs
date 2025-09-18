using Going.UI.Design;
using Going.UIEditor.Tools;
using Going.UIEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UIEditor.Datas
{
    public class Project
    {
        #region Properties
        public string? Name { get; set; }
        public GoDesign Design { get; set; } = new GoDesign();
        public int Width { get; set; }
        public int Height { get; set; }
        public string? ProjectFolder { get; set; }

        [JsonIgnore] public string? FilePath { get; set; }
        [JsonIgnore] public bool Edit { get; set; }
        #endregion

        #region Constructor
        public Project()
        {

        }
        #endregion

        #region Method
        #region Save
        public void Save()
        {
            if (FilePath != null && Directory.Exists(Path.GetDirectoryName(FilePath)))
            {
                var v = JsonSerializer.Serialize(this, JsonOpt.Options);
                try
                {
                    File.WriteAllText(FilePath, v);
                    Edit = false;
                }
                catch (UnauthorizedAccessException) { Program.MessageBox.ShowMessageBoxOk(LM.Save, LM.SavePermissions); }
            }
            else SaveAs();
        }
        #endregion
        #region SaveAs
        public void SaveAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = LM.SaveAs;
                sfd.InitialDirectory = Program.DataMgr.ProjectFolder;
                sfd.Filter = "Going UI Editor File|*.gud";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FilePath = sfd.FileName;
                    var v = JsonSerializer.Serialize(this, JsonOpt.Options);
                    try
                    {
                        File.WriteAllText(FilePath, v);
                        Edit = false;
                    }
                    catch (UnauthorizedAccessException) { Program.MessageBox.ShowMessageBoxOk(LM.Save, LM.SavePermissions); }
                }
            }
        }
        #endregion
      
        #region Open - Static
        public static Project? Open()
        {
            Project? ret = null;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = LM.Open;
                ofd.InitialDirectory = Program.DataMgr.LastOpenFolder ??  Program.DataMgr.ProjectFolder;
                ofd.Multiselect = false;
                ofd.Filter = "Going UI Editor File|*.gud";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var s = File.ReadAllText(ofd.FileName);
                    ret = JsonSerializer.Deserialize<Project>(s, JsonOpt.Options);
                    if (ret != null) ret.FilePath = ofd.FileName;

                    Program.DataMgr.LastOpenFolder = Path.GetDirectoryName(ofd.FileName);
                    Program.DataMgr.SaveSetting();
                }
            }

            return ret;
        }
        #endregion
        #endregion
    }

    #region class : WindowInfo
    public class WindowInfo
    {
        public string Name { get; set; } = "";
        public int Width { get; set; } = 300;
        public int Height { get; set; } = 200;
    }
    #endregion
}
