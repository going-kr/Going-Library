using Going.Basis.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Datas
{
    public class INI(string Path)
    {
        #region Properties
        public string Path { get; private set; } = Path;
        #endregion

        #region Method
        public void DeleteSection(string strSection) => Win32Tool.WritePrivateProfileString(strSection, null, null, Path);

        public bool ExistsINI() => File.Exists(Path);

        public void Write(string Section, string Key, string Value) => Win32Tool.WritePrivateProfileString(Section, Key, Value, Path);

        public string Read(string Section, string Key)
        {
            StringBuilder strValue = new StringBuilder();
            int i = Win32Tool.GetPrivateProfileString(Section, Key, "", strValue, 255, Path);
            return strValue.ToString();
        }
        #endregion
    }
}
