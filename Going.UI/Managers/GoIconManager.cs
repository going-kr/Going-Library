using Going.UI.Icons;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Managers
{
    /// <summary>
    /// 아이콘 폰트를 관리하는 정적 클래스입니다. 여러 아이콘 폰트 소스에서 아이콘을 검색하고 가져옵니다.
    /// </summary>
    public class GoIconManager
    {
        #region Member Variable
        private static Dictionary<string, IIconFont> dic = [];
        #endregion

        #region Constructor
        static GoIconManager()
        {
            var fa = new FontAwesome();
            dic.Add(fa.Name, fa);
        }
        #endregion

        #region Method
        #region Contains
        /// <summary>
        /// 등록된 아이콘 폰트 중에서 지정된 아이콘 문자열이 존재하는지 확인합니다.
        /// </summary>
        /// <param name="IconString">아이콘 문자열</param>
        /// <returns>아이콘이 존재하면 true</returns>
        public static bool Contains(string IconString)
        {
            var ret = false;
            foreach (var item in dic.Values)
            {
                var v = item.Contains(IconString);
                ret |= v;
                if (v) break;
            }
            return ret;
        }
        #endregion
        #region GetIcon
        /// <summary>
        /// 아이콘 문자열에 해당하는 GoIcon 객체를 검색합니다.
        /// </summary>
        /// <param name="IconString">아이콘 문자열</param>
        /// <returns>GoIcon 객체, 없으면 null</returns>
        public static GoIcon? GetIcon(string? IconString)
        {
            GoIcon? ret = null;
            if (IconString != null)
            {
                foreach (var item in dic.Values)
                {
                    var v = item.GetIcon(IconString);
                    ret = v;
                    if (v != null) break;
                }
            }
            return ret;
        }
        #endregion
        #region Add
        /// <summary>
        /// 새로운 아이콘 폰트를 등록합니다.
        /// </summary>
        /// <param name="iconfont">등록할 아이콘 폰트</param>
        public static void Add(IIconFont iconfont)
        {
            dic.Add(iconfont.Name, iconfont);
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// 아이콘 폰트 인터페이스입니다. 아이콘 검색 및 가져오기 기능을 정의합니다.
    /// </summary>
    public interface IIconFont
    {
        /// <summary>
        /// 아이콘 폰트의 이름을 가져옵니다.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 지정된 아이콘 문자열이 이 폰트에 존재하는지 확인합니다.
        /// </summary>
        /// <param name="IconString">아이콘 문자열</param>
        /// <returns>아이콘이 존재하면 true</returns>
        bool Contains(string IconString);
        /// <summary>
        /// 아이콘 문자열에 해당하는 GoIcon 객체를 반환합니다.
        /// </summary>
        /// <param name="IconString">아이콘 문자열</param>
        /// <returns>GoIcon 객체, 없으면 null</returns>
        GoIcon? GetIcon(string IconString);
    }

}
