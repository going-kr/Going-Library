using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Icons
{
    /// <summary>
    /// 아이콘 폰트의 개별 아이콘을 나타내는 클래스입니다.
    /// </summary>
    public class GoIcon
    {
        /// <summary>
        /// 아이콘에 사용되는 폰트 타입페이스를 가져옵니다.
        /// </summary>
        public SKTypeface FontFamily { get; private set; }
        /// <summary>
        /// 아이콘을 나타내는 텍스트(유니코드 문자)를 가져옵니다.
        /// </summary>
        public string IconText { get; private set; }

        /// <summary>
        /// GoIcon의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="family">아이콘 폰트 타입페이스</param>
        /// <param name="icon">아이콘 텍스트</param>
        public GoIcon(SKTypeface family, string icon)
        {
            this.FontFamily = family;
            this.IconText = icon;
        }
    }
}
