using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Going.UI.Tools
{
    public class ValueTool
    {
        #region ToString
        public static string? ToString<T>(T Value, string? FormatString) => ToString((object?)Value, FormatString);
        public static string? ToString(object? Value, string? FormatString)
        {
            var ret = "";

            if (Value != null)
            {
                var tp = Value.GetType();
                if (tp == typeof(sbyte)) ret = FormatString != null ? ((sbyte)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(short)) ret = FormatString != null ? ((short)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(int)) ret = FormatString != null ? ((int)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(long)) ret = FormatString != null ? ((long)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(byte)) ret = FormatString != null ? ((byte)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(ushort)) ret = FormatString != null ? ((ushort)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(uint)) ret = FormatString != null ? ((uint)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(ulong)) ret = FormatString != null ? ((ulong)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(float)) ret = FormatString != null ? ((float)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(double)) ret = FormatString != null ? ((double)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(decimal)) ret = FormatString != null ? ((decimal)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(string)) ret = (string)Value;
                else if (tp == typeof(bool)) ret = ((bool)Value).ToString();
                else if (tp == typeof(DateTime)) ret = FormatString != null ? ((DateTime)Value).ToString(FormatString) : Value.ToString();
                else if (tp == typeof(TimeSpan)) ret = FormatString != null ? ((TimeSpan)Value).ToString(FormatString) : Value.ToString();
                else ret = Value.ToString();
            }

            return ret;
        }
        #endregion

        #region FromString
        public static T? FromString<T>(string? value)  where T : struct
        {
            T? ret = null;

            if(value != null)
            {
                var tp = typeof(T);
                if (tp == typeof(sbyte) && sbyte.TryParse(value, out var v1)) ret = (T)(object)v1;
                else if (tp == typeof(short) && short.TryParse(value, out var v2)) ret = (T)(object)v2;
                else if (tp == typeof(int) && int.TryParse(value, out var v3)) ret = (T)(object)v3;
                else if (tp == typeof(long) && long.TryParse(value, out var v4)) ret = (T)(object)v4;
                else if (tp == typeof(byte) && byte.TryParse(value, out var v5)) ret = (T)(object)v5;
                else if (tp == typeof(ushort) && ushort.TryParse(value, out var v6)) ret = (T)(object)v6;
                else if (tp == typeof(uint) && uint.TryParse(value, out var v7)) ret = (T)(object)v7;
                else if (tp == typeof(ulong) && ulong.TryParse(value, out var v8)) ret = (T)(object)v8;
                else if (tp == typeof(float) && float.TryParse(value, out var v9)) ret = (T)(object)v9;
                else if (tp == typeof(double) && double.TryParse(value, out var v10)) ret = (T)(object)v10;
                else if (tp == typeof(decimal) && decimal.TryParse(value, out var v11)) ret = (T)(object)v11;
                else if (tp == typeof(string)) ret = (T?)(object?)value;
                else if (tp == typeof(bool) && bool.TryParse(value, out var v12)) ret = (T)(object)v12;
                else if (tp == typeof(DateTime) && DateTime.TryParse(value, out var v13)) ret = (T)(object)v13;
                else if (tp == typeof(TimeSpan) && TimeSpan.TryParse(value, out var v14)) ret = (T)(object)v14;
            }
            return ret;
        }
        #endregion

        #region Valid
        public static bool Valid<T>(T Value, T? Minimum, T? Maximum) where T : struct
        {
            var Valid = false;
            var t = typeof(T);
            if (t == typeof(sbyte)) Valid = ((sbyte)(object)Value >= (((sbyte?)(object?)Minimum) ?? sbyte.MinValue)) && ((sbyte)(object)Value <= (((sbyte?)(object?)Maximum) ?? sbyte.MaxValue));
            else if (t == typeof(short)) Valid = ((short)(object)Value >= (((short?)(object?)Minimum) ?? short.MinValue)) && ((short)(object)Value <= (((short?)(object?)Maximum) ?? short.MaxValue));
            else if (t == typeof(int)) Valid = ((int)(object)Value >= (((int?)(object?)Minimum) ?? int.MinValue)) && ((int)(object)Value <= (((int?)(object?)Maximum) ?? int.MaxValue));
            else if (t == typeof(long)) Valid = ((long)(object)Value >= (((long?)(object?)Minimum) ?? long.MinValue)) && ((long)(object)Value <= (((long?)(object?)Maximum) ?? long.MaxValue));
            else if (t == typeof(byte)) Valid = ((byte)(object)Value >= (((byte?)(object?)Minimum) ?? byte.MinValue)) && ((byte)(object)Value <= (((byte?)(object?)Maximum) ?? byte.MaxValue));
            else if (t == typeof(ushort)) Valid = ((ushort)(object)Value >= (((ushort?)(object?)Minimum) ?? ushort.MinValue)) && ((ushort)(object)Value <= (((ushort?)(object?)Maximum) ?? ushort.MaxValue));
            else if (t == typeof(uint)) Valid = ((uint)(object)Value >= (((uint?)(object?)Minimum) ?? uint.MinValue)) && ((uint)(object)Value <= (((uint?)(object?)Maximum) ?? uint.MaxValue));
            else if (t == typeof(ulong)) Valid = ((ulong)(object)Value >= (((ulong?)(object?)Minimum) ?? ulong.MinValue)) && ((ulong)(object)Value <= (((ulong?)(object?)Maximum) ?? ulong.MaxValue));
            else if (t == typeof(float)) Valid = ((float)(object)Value >= (((float?)(object?)Minimum) ?? float.MinValue)) && ((float)(object)Value <= (((float?)(object?)Maximum) ?? float.MaxValue));
            else if (t == typeof(double)) Valid = ((double)(object)Value >= (((double?)(object?)Minimum) ?? double.MinValue)) && ((double)(object)Value <= (((double?)(object?)Maximum) ?? double.MaxValue));
            else if (t == typeof(decimal)) Valid = ((decimal)(object)Value >= (((decimal?)(object?)Minimum) ?? decimal.MinValue)) && ((decimal)(object)Value <= (((decimal?)(object?)Maximum) ?? decimal.MaxValue));
            return Valid;
        }
        #endregion

        #region Constrain
        public static T Constrain<T>(T Value, T? Minimum, T? Maximum) where T : struct
        {
            T ret= Value;
            var t = typeof(T);
            if (t == typeof(sbyte)) ret = (T)(object)MathTool.Constrain((sbyte)(object)Value, (((sbyte?)(object?)Minimum) ?? sbyte.MinValue), (((sbyte?)(object?)Maximum) ?? sbyte.MaxValue));
            else if (t == typeof(short)) ret = (T)(object)MathTool.Constrain((short)(object)Value, (((short?)(object?)Minimum) ?? short.MinValue), (((short?)(object?)Maximum) ?? short.MaxValue));
            else if (t == typeof(int)) ret = (T)(object)MathTool.Constrain((int)(object)Value, (((int?)(object?)Minimum) ?? int.MinValue), (((int?)(object?)Maximum) ?? int.MaxValue));
            else if (t == typeof(long)) ret = (T)(object)MathTool.Constrain((long)(object)Value, (((long?)(object?)Minimum) ?? long.MinValue), (((long?)(object?)Maximum) ?? long.MaxValue));
            else if (t == typeof(byte)) ret = (T)(object)MathTool.Constrain((byte)(object)Value, (((byte?)(object?)Minimum) ?? byte.MinValue), (((byte?)(object?)Maximum) ?? byte.MaxValue));
            else if (t == typeof(ushort)) ret = (T)(object)MathTool.Constrain((ushort)(object)Value, (((ushort?)(object?)Minimum) ?? ushort.MinValue), (((ushort?)(object?)Maximum) ?? ushort.MaxValue));
            else if (t == typeof(uint)) ret = (T)(object)MathTool.Constrain((uint)(object)Value, (((uint?)(object?)Minimum) ?? uint.MinValue), (((uint?)(object?)Maximum) ?? uint.MaxValue));
            else if (t == typeof(ulong)) ret = (T)(object)MathTool.Constrain((ulong)(object)Value, (((ulong?)(object?)Minimum) ?? ulong.MinValue), (((ulong?)(object?)Maximum) ?? ulong.MaxValue));
            else if (t == typeof(float)) ret = (T)(object)MathTool.Constrain((float)(object)Value, (((float?)(object?)Minimum) ?? float.MinValue), (((float?)(object?)Maximum) ?? float.MaxValue));
            else if (t == typeof(double)) ret = (T)(object)MathTool.Constrain((double)(object)Value, (((double?)(object?)Minimum) ?? double.MinValue), (((double?)(object?)Maximum) ?? double.MaxValue));
            else if (t == typeof(decimal)) ret = (T)(object)MathTool.Constrain((decimal)(object)Value, (((decimal?)(object?)Minimum) ?? decimal.MinValue), (((decimal?)(object?)Maximum) ?? decimal.MaxValue));
            return ret;
        }
        #endregion
    }
}
