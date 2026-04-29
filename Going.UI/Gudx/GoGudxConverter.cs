using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Going.UI.Controls;
using Going.UI.Json;

namespace Going.UI.Gudx;

/// <summary>
/// Converts GoControl object graphs to/from Gudx XML.
/// Reflection-driven: tags are control class names (PascalCase, T.Name);
/// attributes are scalar [GoProperty]-bearing properties; children are
/// [GoChilds]-marked properties (patterns P2-P5, B1-B2).
/// Reuses GoJsonConverter.ControlTypes for the type-name registry.
/// </summary>
public static class GoGudxConverter
{
    public static string SerializeControl(IGoControl control)
    {
        var elem = WriteElement(control);
        return elem.ToString(SaveOptions.None);
    }

    public static IGoControl DeserializeControl(string xml)
    {
        var elem = XElement.Parse(xml);
        return ReadElement(elem);
    }

    internal static XElement WriteElement(IGoControl control)
    {
        var type = control.GetType();
        var tagName = TypeTagName(type);
        var elem = new XElement(tagName);

        // P1: scalar properties become attributes.
        foreach (var prop in ScalarProperties(type))
        {
            var value = prop.GetValue(control);
            if (value == null) continue;
            var s = FormatScalar(value);
            if (s != null) elem.SetAttributeValue(prop.Name, s);
        }

        return elem;
    }

    internal static IGoControl ReadElement(XElement elem)
    {
        var tagName = elem.Name.LocalName;
        if (!GoJsonConverter.ControlTypes.TryGetValue(tagName, out var type))
            throw new InvalidOperationException($"Unknown Gudx tag: {tagName}");

        var instance = (IGoControl)Activator.CreateInstance(type)!;

        // P1: read attributes onto scalar properties.
        foreach (var attr in elem.Attributes())
        {
            var prop = type.GetProperty(attr.Name.LocalName);
            if (prop == null || !prop.CanWrite) continue;
            var parsed = ParseScalar(prop.PropertyType, attr.Value);
            if (parsed != null) prop.SetValue(instance, parsed);
        }

        return instance;
    }

    internal static string TypeTagName(Type t)
    {
        if (t.IsGenericType)
            return t.Name.Split('`')[0] + "<T>";
        return t.Name;
    }

    private static IEnumerable<PropertyInfo> ScalarProperties(Type t)
    {
        // Properties that are NOT marked [GoChilds] AND have a settable scalar/special type.
        return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<GoChildsAttribute>() == null)
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => IsScalar(p.PropertyType));
    }

    private static bool IsScalar(Type t)
    {
        if (t.IsPrimitive || t == typeof(string) || t == typeof(decimal)) return true;
        if (t.IsEnum) return true;
        // SKColor/SKRect handled by GudxSpecialConverters (Task 11).
        return false;
    }

    private static string? FormatScalar(object value)
    {
        return value switch
        {
            string s => s,
            bool b => b ? "true" : "false",
            float f => f.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            Enum e => e.ToString(),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }

    private static object? ParseScalar(Type targetType, string value)
    {
        if (targetType == typeof(string)) return value;
        if (targetType == typeof(bool)) return value.Equals("true", StringComparison.OrdinalIgnoreCase);
        if (targetType.IsEnum) return Enum.Parse(targetType, value, ignoreCase: false);
        if (targetType == typeof(int)) return int.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(float)) return float.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(double)) return double.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(decimal)) return decimal.Parse(value, CultureInfo.InvariantCulture);
        return null;
    }
}
