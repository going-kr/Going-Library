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
    [TypeConverter(typeof(GoPaddingConverter))]
    public class GoPadding
    {
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public GoPadding() { }
        public GoPadding(float all) => this.Left = this.Top = this.Right = this.Bottom = all;
        public GoPadding(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public override string ToString() => $"{Left}, {Top}, {Right}, {Bottom}";
    }

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
            throw new ArgumentException("유효하지 않은 포맷입니다.'");
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
