using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Going.UI.Datas
{
    /// <summary>
    /// 네 방향(좌, 상, 우, 하)의 여백(패딩) 값을 나타내는 클래스입니다.
    /// </summary>
    [TypeConverter(typeof(GoPaddingConverter))]
    public class GoPadding
    {
        /// <summary>왼쪽 여백</summary>
        public float Left { get; set; }
        /// <summary>상단 여백</summary>
        public float Top { get; set; }
        /// <summary>오른쪽 여백</summary>
        public float Right { get; set; }
        /// <summary>하단 여백</summary>
        public float Bottom { get; set; }

        /// <summary>기본 생성자 (모든 여백 0)</summary>
        public GoPadding() { }
        /// <summary>모든 방향에 동일한 여백을 적용하는 생성자</summary>
        /// <param name="all">모든 방향에 적용할 여백 값</param>
        public GoPadding(float all) => this.Left = this.Top = this.Right = this.Bottom = all;
        /// <summary>각 방향별 여백을 개별 지정하는 생성자</summary>
        /// <param name="left">왼쪽 여백</param>
        /// <param name="top">상단 여백</param>
        /// <param name="right">오른쪽 여백</param>
        /// <param name="bottom">하단 여백</param>
        public GoPadding(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>여백 값을 문자열로 반환합니다.</summary>
        public override string ToString() => $"{Left}, {Top}, {Right}, {Bottom}";
    }

    /// <summary>
    /// <see cref="GoPadding"/>과 문자열 간의 변환을 담당하는 TypeConverter입니다.
    /// </summary>
    public class GoPaddingConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string str)
            {
                var parts = str.Split(',').Select(x => x.Trim()).ToArray();
                if (parts.Length == 4 &&
                    float.TryParse(parts[0], out float left) &&
                    float.TryParse(parts[1], out float top) &&
                    float.TryParse(parts[2], out float right) &&
                    float.TryParse(parts[3], out float bottom))
                {
                    return new GoPadding(left, top, right, bottom);
                }
            }
            throw new ArgumentException("유효하지 않은 포맷입니다.");
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is GoPadding p)
            {
                return p.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
