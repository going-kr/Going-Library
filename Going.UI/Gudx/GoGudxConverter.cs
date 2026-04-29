using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Going.UI.Containers;
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
    static GoGudxConverter()
    {
        // Force GoJsonConverter's static ctor to run so ControlTypes is populated.
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
            typeof(GoJsonConverter).TypeHandle);
    }

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

        // P2/P3/P4/P5/B1: walk [GoChilds] properties.
        foreach (var childProp in ChildProperties(type))
        {
            var value = childProp.GetValue(control);
            if (value == null) continue;

            if (value is System.Collections.IList list && IsHomogeneousControlList(childProp.PropertyType))
            {
                // P2: list of IGoControl
                foreach (var child in list)
                    if (child is IGoControl c) elem.Add(WriteElement(c));
            }
            else if (value is GoTableLayoutControlCollection tlc)
            {
                // P3: cell-indexed collection
                foreach (var child in tlc.Controls)
                {
                    var childElem = WriteElement(child);
                    if (tlc.Indexes.TryGetValue(child.Id, out var idx))
                        childElem.SetAttributeValue("Cell", $"{idx.Column},{idx.Row}");
                    elem.Add(childElem);
                }
            }
            // P4/P5/B1 dispatch added in later tasks
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

        // P2: populate [GoChilds] homogeneous-list properties from child elements.
        foreach (var childProp in ChildProperties(type))
        {
            if (IsHomogeneousControlList(childProp.PropertyType))
            {
                var list = (System.Collections.IList)(childProp.GetValue(instance)
                            ?? Activator.CreateInstance(childProp.PropertyType)!);
                foreach (var childElem in elem.Elements())
                    list.Add(ReadElement(childElem));
                // setter only if it has one
                if (childProp.CanWrite) childProp.SetValue(instance, list);
            }
            else if (childProp.PropertyType == typeof(GoTableLayoutControlCollection))
            {
                // P3: cell-indexed collection
                var coll = (GoTableLayoutControlCollection)(childProp.GetValue(instance)
                            ?? new GoTableLayoutControlCollection());
                foreach (var childElem in elem.Elements())
                {
                    var c = ReadElement(childElem);
                    var cellAttr = childElem.Attribute("Cell");
                    if (cellAttr != null)
                    {
                        var parts = cellAttr.Value.Split(',');
                        int col = int.Parse(parts[0], CultureInfo.InvariantCulture);
                        int row = int.Parse(parts[1], CultureInfo.InvariantCulture);
                        coll.Add(c, col, row);
                    }
                    else
                    {
                        coll.Controls.Add(c);
                    }
                }
            }
            // dispatch for other patterns added later
        }

        return instance;
    }

    internal static string TypeTagName(Type t)
    {
        if (t.IsGenericType)
            return t.Name.Split('`')[0] + "<T>";
        return t.Name;
    }

    private static IEnumerable<PropertyInfo> ChildProperties(Type t) =>
        t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
         .Where(p => p.GetCustomAttribute<GoChildsAttribute>() != null);

    private static bool IsHomogeneousControlList(Type t)
    {
        if (!t.IsGenericType) return false;
        if (t.GetGenericTypeDefinition() != typeof(List<>)) return false;
        var arg = t.GetGenericArguments()[0];
        return typeof(IGoControl).IsAssignableFrom(arg);
    }

    private static IEnumerable<PropertyInfo> ScalarProperties(Type t)
    {
        // Properties that are NOT marked [GoChilds] AND have a settable scalar/special type.
        return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                .Where(p => p.GetCustomAttribute<GoChildsAttribute>() == null)
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => IsScalar(p.PropertyType));
    }

    private static bool IsScalar(Type t)
    {
        if (t.IsPrimitive || t == typeof(string) || t == typeof(decimal)) return true;
        if (t.IsEnum) return true;
        if (t == typeof(List<string>)) return true;
        // SKColor/SKRect handled by GudxSpecialConverters (Task 11).
        return false;
    }

    private static string? FormatScalar(object value)
    {
        return value switch
        {
            List<string> ls => string.Join(",", ls),
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
        if (targetType == typeof(List<string>))
            return value == "" ? new List<string>() : value.Split(',').ToList();
        return null;
    }
}
