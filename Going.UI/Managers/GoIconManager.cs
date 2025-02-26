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
        public static void Add(IIconFont iconfont)
        {
            dic.Add(iconfont.Name, iconfont);
        }
        #endregion
        #endregion
    }

    public interface IIconFont
    {
        string Name { get; }

        bool Contains(string IconString);
        GoIcon? GetIcon(string IconString);
    }

}
