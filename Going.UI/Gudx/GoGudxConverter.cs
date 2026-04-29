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
    /// <summary>
    /// Registry of non-IGoControl wrapper types used in P4 wrapper collections.
    /// Maps XML tag name → CLR type.
    /// </summary>
    internal static readonly Dictionary<string, Type> WrapperTypes = new()
    {
        ["GoSubPage"] = typeof(Going.UI.Containers.GoSubPage),
        ["GoGridLayoutPanelRow"] = typeof(Going.UI.Containers.GoGridLayoutPanelRow),
    };

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

        // GoGridLayoutPanel special case: P4b dual interlock (Rows + Childrens).
        // Emits <GoGridLayoutPanelRow> wrappers with children nested inside by row index.
        // Returns early to skip generic [GoChilds] dispatch (prevents double-emission).
        if (control is GoGridLayoutPanel grid)
        {
            for (int rowIdx = 0; rowIdx < grid.Rows.Count; rowIdx++)
            {
                var rowElem = WriteWrapper(grid.Rows[rowIdx]);
                foreach (var child in grid.Childrens.Controls)
                {
                    if (grid.Childrens.Indexes.TryGetValue(child.Id, out var idx) && idx.Row == rowIdx)
                    {
                        var childElem = WriteElement(child);
                        // GoGridIndex has no ColSpan/RowSpan — always 2-part Cell
                        childElem.SetAttributeValue("Cell", $"{idx.Column},{idx.Row}");
                        rowElem.Add(childElem);
                    }
                }
                elem.Add(rowElem);
            }
            return elem;  // skip generic dispatch
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
                // P3: cell-indexed collection (GoTableLayoutPanel)
                foreach (var child in tlc.Controls)
                {
                    var childElem = WriteElement(child);
                    if (tlc.Indexes.TryGetValue(child.Id, out var idx))
                    {
                        var cell = (idx.ColSpan == 1 && idx.RowSpan == 1)
                            ? $"{idx.Column},{idx.Row}"
                            : $"{idx.Column},{idx.Row},{idx.ColSpan},{idx.RowSpan}";
                        childElem.SetAttributeValue("Cell", cell);
                    }
                    elem.Add(childElem);
                }
            }
            else if (IsWrapperList(childProp.PropertyType))
            {
                // P4a: wrapper-typed collection (e.g. GoSwitchPanel.Pages : List<GoSubPage>)
                foreach (var item in (System.Collections.IEnumerable)value)
                    elem.Add(WriteWrapper(item));
            }
            // P5/B1 dispatch added in later tasks
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

        // GoGridLayoutPanel special case: P4b dual interlock (Rows + Childrens).
        // Each <GoGridLayoutPanelRow> wrapper contains its child controls.
        // Returns early to skip generic [GoChilds] dispatch.
        if (instance is GoGridLayoutPanel gridIn)
        {
            foreach (var rowElem in elem.Elements("GoGridLayoutPanelRow"))
            {
                var row = (GoGridLayoutPanelRow)ReadWrapper(rowElem, typeof(GoGridLayoutPanelRow));
                gridIn.Rows.Add(row);
                foreach (var childElem in rowElem.Elements())
                {
                    var c = ReadElement(childElem);
                    var cellAttr = childElem.Attribute("Cell");
                    if (cellAttr != null)
                    {
                        var parts = cellAttr.Value.Split(',');
                        var col = int.Parse(parts[0], CultureInfo.InvariantCulture);
                        var rowIdx = int.Parse(parts[1], CultureInfo.InvariantCulture);
                        gridIn.Childrens.Add(c, col, rowIdx);
                    }
                    else
                    {
                        gridIn.Childrens.Controls.Add(c);
                    }
                }
            }
            return gridIn;  // skip generic dispatch
        }

        // P2/P3/P4: populate [GoChilds] properties from child elements.
        foreach (var childProp in ChildProperties(type))
        {
            if (IsHomogeneousControlList(childProp.PropertyType))
            {
                // P2: list of IGoControl
                var list = (System.Collections.IList)(childProp.GetValue(instance)
                            ?? Activator.CreateInstance(childProp.PropertyType)!);
                foreach (var childElem in elem.Elements())
                    list.Add(ReadElement(childElem));
                if (childProp.CanWrite) childProp.SetValue(instance, list);
            }
            else if (childProp.PropertyType == typeof(GoTableLayoutControlCollection))
            {
                // P3: cell-indexed collection (GoTableLayoutPanel)
                var coll = (GoTableLayoutControlCollection)(childProp.GetValue(instance)
                            ?? new GoTableLayoutControlCollection());
                foreach (var childElem in elem.Elements())
                {
                    var c = ReadElement(childElem);
                    var cellAttr = childElem.Attribute("Cell");
                    if (cellAttr != null)
                    {
                        var parts = cellAttr.Value.Split(',');
                        var col = int.Parse(parts[0], CultureInfo.InvariantCulture);
                        var row = int.Parse(parts[1], CultureInfo.InvariantCulture);
                        var colSpan = parts.Length >= 4 ? int.Parse(parts[2], CultureInfo.InvariantCulture) : 1;
                        var rowSpan = parts.Length >= 4 ? int.Parse(parts[3], CultureInfo.InvariantCulture) : 1;
                        coll.Add(c, col, row, colSpan, rowSpan);
                    }
                    else
                    {
                        coll.Controls.Add(c);
                    }
                }
            }
            else if (IsWrapperList(childProp.PropertyType))
            {
                // P4a: wrapper-typed collection (e.g. GoSwitchPanel.Pages : List<GoSubPage>)
                var list = (System.Collections.IList)(childProp.GetValue(instance)
                            ?? Activator.CreateInstance(childProp.PropertyType)!);
                var wrapperType = childProp.PropertyType.GetGenericArguments()[0];
                var wrapperTagName = wrapperType.Name;
                foreach (var childElem in elem.Elements(wrapperTagName))
                    list.Add(ReadWrapper(childElem, wrapperType));
                if (childProp.CanWrite) childProp.SetValue(instance, list);
            }
            // P5/B1 dispatch added in later tasks
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

    /// <summary>
    /// Returns true when the type is a List&lt;TWrapper&gt; where TWrapper is a known non-IGoControl wrapper.
    /// </summary>
    private static bool IsWrapperList(Type t)
    {
        if (!t.IsGenericType) return false;
        if (t.GetGenericTypeDefinition() != typeof(List<>)) return false;
        var arg = t.GetGenericArguments()[0];
        return WrapperTypes.ContainsValue(arg);
    }

    /// <summary>
    /// Serializes a non-IGoControl wrapper object (e.g. GoSubPage, GoGridLayoutPanelRow)
    /// to an XElement using the type name as tag and reflection for P1 scalars.
    /// P2 child lists (IGoControl) nested inside the wrapper are also emitted.
    /// </summary>
    internal static XElement WriteWrapper(object item)
    {
        var type = item.GetType();
        var elem = new XElement(type.Name);

        // P1: scalar attributes
        foreach (var prop in ScalarProperties(type))
        {
            var value = prop.GetValue(item);
            if (value == null) continue;
            var s = FormatScalar(value);
            if (s != null) elem.SetAttributeValue(prop.Name, s);
        }

        // Recursive P2: emit IGoControl children if the wrapper has [GoChilds] List<IGoControl>
        foreach (var childProp in ChildProperties(type))
        {
            var v = childProp.GetValue(item);
            if (v == null) continue;
            if (v is System.Collections.IList list && IsHomogeneousControlList(childProp.PropertyType))
            {
                foreach (var c in list)
                    if (c is IGoControl gc) elem.Add(WriteElement(gc));
            }
        }

        return elem;
    }

    /// <summary>
    /// Deserializes a non-IGoControl wrapper object from an XElement.
    /// Reads P1 scalar attributes and recursively populates P2 child lists.
    /// </summary>
    internal static object ReadWrapper(XElement elem, Type type)
    {
        var instance = Activator.CreateInstance(type)!;

        // P1: read attributes onto scalar properties
        foreach (var attr in elem.Attributes())
        {
            var prop = type.GetProperty(attr.Name.LocalName);
            if (prop == null || !prop.CanWrite) continue;
            var parsed = ParseScalar(prop.PropertyType, attr.Value);
            if (parsed != null) prop.SetValue(instance, parsed);
        }

        // Recursive P2: populate IGoControl children if wrapper has [GoChilds] List<IGoControl>
        foreach (var childProp in ChildProperties(type))
        {
            if (IsHomogeneousControlList(childProp.PropertyType))
            {
                var list = (System.Collections.IList)(childProp.GetValue(instance)
                            ?? Activator.CreateInstance(childProp.PropertyType)!);
                foreach (var childElem in elem.Elements())
                    list.Add(ReadElement(childElem));
                // Only assign via setter if the property has one (GoSubPage.Childrens is get-only)
                if (childProp.CanWrite) childProp.SetValue(instance, list);
            }
        }

        return instance;
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
