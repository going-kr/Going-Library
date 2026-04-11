using Going.Basis.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.Basis.Datas
{
    /// <summary>
    /// Windows INI 파일 읽기/쓰기 유틸리티 클래스.
    /// Win32 API(GetPrivateProfileString, WritePrivateProfileString)를 사용한다.
    /// </summary>
    public class INI(string Path)
    {
        #region Properties
        /// <summary>INI 파일 경로</summary>
        public string Path { get; private set; } = Path;
        #endregion

        #region Method
        /// <summary>INI 파일에서 지정 섹션을 삭제한다.</summary>
        /// <param name="strSection">삭제할 섹션 이름</param>
        public void DeleteSection(string strSection) => Win32Tool.WritePrivateProfileString(strSection, null, null, Path);

        /// <summary>INI 파일이 존재하는지 확인한다.</summary>
        /// <returns>파일이 존재하면 true</returns>
        public bool ExistsINI() => File.Exists(Path);

        /// <summary>INI 파일에 값을 쓴다.</summary>
        /// <param name="Section">섹션 이름</param>
        /// <param name="Key">키 이름</param>
        /// <param name="Value">저장할 값</param>
        public void Write(string Section, string Key, string Value) => Win32Tool.WritePrivateProfileString(Section, Key, Value, Path);

        /// <summary>INI 파일에서 값을 읽는다.</summary>
        /// <param name="Section">섹션 이름</param>
        /// <param name="Key">키 이름</param>
        /// <returns>읽은 값 문자열. 없으면 빈 문자열</returns>
        public string Read(string Section, string Key)
        {
            StringBuilder strValue = new StringBuilder();
            int i = Win32Tool.GetPrivateProfileString(Section, Key, "", strValue, 255, Path);
            return strValue.ToString();
        }
        #endregion
    }
}
