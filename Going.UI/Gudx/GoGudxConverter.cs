using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Design;
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

    // ─────────────────────────────────────────────────────────────────────────
    // Public entry points
    // ─────────────────────────────────────────────────────────────────────────

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

    /// <summary>
    /// Serializes any object (including non-IGoControl roots such as GoDesign) to an XElement.
    /// Tag name is derived from the runtime type. This is the generalized write entry point.
    /// Made public so tests (and future Task 12 high-level API) can call it directly.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="explicitTagName">Optional XML tag name override. When omitted, the runtime type's class name is used. Used by B1 dispatch to map property names (e.g., LeftSideBar) or [GudxTagName] overrides (e.g., Theme) to XML tags.</param>
    /// <remarks>
    /// PROVISIONAL: This API is exposed for tests and Task 12's GoDesign.SerializeGudx wiring.
    /// External callers should prefer the higher-level GoDesign.SerializeGudx(masterPath) once Task 12 lands.
    /// </remarks>
    public static XElement WriteAny(object obj, string? explicitTagName = null)
    {
        var type = obj.GetType();
        var tagName = explicitTagName ?? TypeTagName(type);
        var elem = new XElement(tagName);

        // GoGridLayoutPanel special case: P4b dual interlock (Rows + Childrens).
        // Emits <GoGridLayoutPanelRow> wrappers with children nested inside by row index.
        // Returns early to skip generic [GoChilds] dispatch (prevents double-emission).
        if (obj is GoGridLayoutPanel grid)
        {
            // P1: scalar attributes first
            foreach (var prop in ScalarProperties(type))
            {
                var value = prop.GetValue(grid);
                if (value == null) continue;
                var s = FormatScalar(value);
                if (s != null) elem.SetAttributeValue(prop.Name, s);
            }
            // GoGridLayoutPanel special case interlock
            for (int rowIdx = 0; rowIdx < grid.Rows.Count; rowIdx++)
            {
                var rowElem = WriteWrapper(grid.Rows[rowIdx]);
                foreach (var child in grid.Childrens.Controls)
                {
                    if (grid.Childrens.Indexes.TryGetValue(child.Id, out var idx) && idx.Row == rowIdx)
                    {
                        var childElem = WriteAny(child);
                        // GoGridIndex has no ColSpan/RowSpan — always 2-part Cell
                        childElem.SetAttributeValue("Cell", $"{idx.Column},{idx.Row}");
                        rowElem.Add(childElem);
                    }
                }
                elem.Add(rowElem);
            }
            return elem;  // skip generic dispatch
        }

        // P1: scalar properties become attributes.
        foreach (var prop in ScalarProperties(type))
        {
            var value = prop.GetValue(obj);
            if (value == null) continue;
            var s = FormatScalar(value);
            if (s != null) elem.SetAttributeValue(prop.Name, s);
        }

        // P2/P3/P4/P5: walk [GoChilds] properties.
        foreach (var childProp in ChildProperties(type))
        {
            var value = childProp.GetValue(obj);
            if (value == null) continue;
            DispatchChildWrite(elem, childProp, value);
        }

        return elem;
    }

    /// <summary>
    /// Serializes a GoDesign to an XElement. Convenience wrapper over WriteAny.
    /// </summary>
    public static XElement WriteGoDesign(GoDesign design) => WriteAny(design);

    /// <summary>
    /// Deserializes a GoDesign from an XElement. Returns null if the root tag is not "GoDesign".
    /// </summary>
    public static GoDesign? ReadGoDesign(XElement elem)
    {
        if (elem.Name.LocalName != "GoDesign") return null;
        var d = new GoDesign();
        PopulateAny(elem, d);
        return d;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internal write helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Thin facade over WriteAny for IGoControl. Preserves the existing call surface
    /// used by serialization tests and wrapper recursion.
    /// </summary>
    internal static XElement WriteElement(IGoControl control) => WriteAny(control);

    private static void DispatchChildWrite(XElement elem, PropertyInfo childProp, object value)
    {
        // Order: P2 (List<IGoControl>) → P3 (TLC) → P4 (wrapper list) → P5 (dict) → B1 (single non-collection, Task 9).
        // Most-specific predicates first.
        var pType = childProp.PropertyType;

        if (value is System.Collections.IList list && IsHomogeneousControlList(pType))
        {
            // P2: list of IGoControl
            foreach (var child in list)
                if (child is IGoControl c) elem.Add(WriteAny(c));
            return;
        }

        if (value is GoTableLayoutControlCollection tlc)
        {
            // P3: cell-indexed collection (GoTableLayoutPanel)
            foreach (var child in tlc.Controls)
            {
                var childElem = WriteAny(child);
                if (tlc.Indexes.TryGetValue(child.Id, out var idx))
                {
                    var cell = (idx.ColSpan == 1 && idx.RowSpan == 1)
                        ? $"{idx.Column},{idx.Row}"
                        : $"{idx.Column},{idx.Row},{idx.ColSpan},{idx.RowSpan}";
                    childElem.SetAttributeValue("Cell", cell);
                }
                elem.Add(childElem);
            }
            return;
        }

        if (IsWrapperList(pType))
        {
            // P4a: wrapper-typed collection (e.g. GoSwitchPanel.Pages : List<GoSubPage>)
            foreach (var item in (System.Collections.IEnumerable)value)
                elem.Add(WriteWrapper(item));
            return;
        }

        if (IsKeyedDict(pType, out _))
        {
            // P5: keyed dictionary (e.g. GoDesign.Pages : Dictionary<string, GoPage>)
            foreach (System.Collections.DictionaryEntry kvp in (System.Collections.IDictionary)value)
            {
                if (kvp.Value == null) continue;
                var childElem = WriteAny(kvp.Value);
                // Overwrite Name attribute with the dictionary key to ensure round-trip fidelity.
                childElem.SetAttributeValue("Name", kvp.Key.ToString());
                elem.Add(childElem);
            }
            return;
        }

        // B1: single-child object (Task 9) — LAST branch (most permissive predicate).
        // Tag = [GudxTagName] override if present, else property name (not class name).
        if (IsSingleChildObject(pType))
        {
            var tagName = childProp.GetCustomAttribute<GudxTagNameAttribute>()?.Name ?? childProp.Name;
            elem.Add(WriteAny(value, tagName));
            return;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internal read helpers
    // ─────────────────────────────────────────────────────────────────────────

    internal static IGoControl ReadElement(XElement elem)
    {
        var tagName = elem.Name.LocalName;
        if (!GoJsonConverter.ControlTypes.TryGetValue(tagName, out var type))
            throw new InvalidOperationException($"Unknown Gudx tag: {tagName}");

        var instance = (IGoControl)Activator.CreateInstance(type)!;
        PopulateAny(elem, instance);
        return instance;
    }

    /// <summary>
    /// Populates an existing object instance from an XElement using reflection.
    /// Handles P1 scalar attributes and [GoChilds] child dispatching.
    /// Works for IGoControl and non-IGoControl types (GoDesign, GoPage, GoWindow, wrappers).
    /// </summary>
    internal static void PopulateAny(XElement elem, object instance)
    {
        var type = instance.GetType();

        // GoGridLayoutPanel special case: P4b dual interlock.
        // Must be checked before generic dispatch to avoid double-emission.
        if (instance is GoGridLayoutPanel gridIn)
        {
            // P1: read scalar attributes
            foreach (var attr in elem.Attributes())
            {
                var prop = type.GetProperty(attr.Name.LocalName);
                if (prop == null || !prop.CanWrite) continue;
                var parsed = ParseScalar(prop.PropertyType, attr.Value);
                if (parsed != null) prop.SetValue(instance, parsed);
            }
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
            return;  // skip generic dispatch
        }

        // P1: read attributes onto scalar properties.
        foreach (var attr in elem.Attributes())
        {
            var prop = type.GetProperty(attr.Name.LocalName);
            if (prop == null || !prop.CanWrite) continue;
            var parsed = ParseScalar(prop.PropertyType, attr.Value);
            if (parsed != null) prop.SetValue(instance, parsed);
        }

        // P2/P3/P4/P5: populate [GoChilds] properties from child elements.
        foreach (var childProp in ChildProperties(type))
        {
            DispatchChildRead(elem, childProp, instance);
        }
    }

    private static void DispatchChildRead(XElement elem, PropertyInfo childProp, object instance)
    {
        // Order: P2 (List<IGoControl>) → P3 (TLC) → P4 (wrapper list) → P5 (dict) → B1 (single non-collection, Task 9).
        // Most-specific predicates first.
        var pType = childProp.PropertyType;

        if (IsHomogeneousControlList(pType))
        {
            // P2: list of IGoControl.
            // Design decision: P2 read is UNFILTERED (consumes all child elements).
            // This is safe because no type currently has BOTH a P2 and a P5 [GoChilds] property.
            // GoDesign's [GoChilds] properties are P5 only. If a future type mixes P2 + P5,
            // P2 will need the same tag-filter as P5 (check GoJsonConverter.ControlTypes lookup).
            var list = (System.Collections.IList)(childProp.GetValue(instance)
                        ?? Activator.CreateInstance(pType)!);
            foreach (var childElem in elem.Elements())
                list.Add(ReadElement(childElem));
            if (childProp.CanWrite) childProp.SetValue(instance, list);
            return;
        }

        if (pType == typeof(GoTableLayoutControlCollection))
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
            return;
        }

        if (IsWrapperList(pType))
        {
            // P4a: wrapper-typed collection (e.g. GoSwitchPanel.Pages : List<GoSubPage>)
            var list = (System.Collections.IList)(childProp.GetValue(instance)
                        ?? Activator.CreateInstance(pType)!);
            var wrapperType = pType.GetGenericArguments()[0];
            var wrapperTagName = wrapperType.Name;
            foreach (var childElem in elem.Elements(wrapperTagName))
                list.Add(ReadWrapper(childElem, wrapperType));
            if (childProp.CanWrite) childProp.SetValue(instance, list);
            return;
        }

        if (IsKeyedDict(pType, out var valueType))
        {
            // P5: keyed dictionary — TAG-FILTERED by value type's tag name (Task 7 review I-3).
            // Each [GoChilds] dict property on a multi-property type (e.g. GoDesign.Pages +
            // GoDesign.Windows) reads ONLY elements whose LocalName matches its value type.
            // This prevents one property from consuming elements destined for another.
            var dict = (System.Collections.IDictionary)(childProp.GetValue(instance)
                        ?? Activator.CreateInstance(pType)!);
            var valueTagName = TypeTagName(valueType);
            foreach (var childElem in elem.Elements(valueTagName))
            {
                var key = childElem.Attribute("Name")?.Value
                    ?? throw new InvalidOperationException(
                        $"P5 dict child <{valueTagName}> missing required Name attribute");
                object childInstance;
                if (typeof(IGoControl).IsAssignableFrom(valueType))
                {
                    childInstance = ReadElement(childElem);
                }
                else
                {
                    childInstance = Activator.CreateInstance(valueType)!;
                    PopulateAny(childElem, childInstance);
                }
                dict[key] = childInstance;
            }
            if (childProp.CanWrite) childProp.SetValue(instance, dict);
            return;
        }

        // B1: single-child object (Task 9) — LAST branch (most permissive predicate).
        // Tag-filtered by property name (or [GudxTagName] override) to avoid consuming
        // elements destined for other [GoChilds] properties.
        if (IsSingleChildObject(pType))
        {
            var tagName = childProp.GetCustomAttribute<GudxTagNameAttribute>()?.Name ?? childProp.Name;
            var childElem = elem.Element(tagName);
            if (childElem == null) return;  // missing optional child OK

            // Resolve actual type (unwrap nullable T? → T for Activator.CreateInstance).
            var actualType = Nullable.GetUnderlyingType(pType) ?? pType;

            // Try get-only / private-set first: mutate the existing instance in place.
            var existing = childProp.GetValue(instance);
            if (existing != null)
            {
                PopulateAny(childElem, existing);
                // If also writable (private set), we can leave it as-is (already mutated).
                return;
            }

            // existing is null (nullable property not yet initialized): create new instance.
            var instance2 = Activator.CreateInstance(actualType);
            if (instance2 != null)
            {
                PopulateAny(childElem, instance2);
                // Use reflection to set even private setters.
                var setter = childProp.GetSetMethod(nonPublic: true);
                if (setter == null)
                    throw new InvalidOperationException(
                        $"[GoChilds] B1 property {instance.GetType().Name}.{childProp.Name} has no setter and was null at read time. " +
                        $"Either initialize a default value or add a (private) setter.");
                setter.Invoke(instance, new[] { instance2 });
            }
            return;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Wrapper serialization (P4)
    // ─────────────────────────────────────────────────────────────────────────

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
                    if (c is IGoControl gc) elem.Add(WriteAny(gc));
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

    // ─────────────────────────────────────────────────────────────────────────
    // Reflection helpers
    // ─────────────────────────────────────────────────────────────────────────

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
    /// Returns true when the type is a Dictionary&lt;string, TValue&gt;.
    /// Sets valueType to the TValue type argument.
    /// </summary>
    private static bool IsKeyedDict(Type t, out Type valueType)
    {
        valueType = typeof(object);
        if (!t.IsGenericType) return false;
        if (t.GetGenericTypeDefinition() != typeof(Dictionary<,>)) return false;
        var args = t.GetGenericArguments();
        if (args[0] != typeof(string)) return false;
        valueType = args[1];
        return true;
    }

    /// <summary>
    /// Returns true when the type represents a single non-collection object (B1 pattern).
    /// Excludes: primitives, strings, enums, decimal, IEnumerable types (collections),
    /// and known SkiaSharp value types (SKColor, SKRect) that are handled as P1 scalars.
    /// This is the most permissive predicate and MUST be the last branch in dispatch.
    /// </summary>
    private static bool IsSingleChildObject(Type t)
    {
        if (t.IsPrimitive || t == typeof(string) || t == typeof(decimal) || t.IsEnum) return false;
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t)) return false;
        if (t == typeof(SkiaSharp.SKColor) || t == typeof(SkiaSharp.SKRect)) return false;
        // Reference type (or nullable reference) — assume single-child object.
        // Handles nullable T? by checking the underlying type.
        var underlying = Nullable.GetUnderlyingType(t);
        if (underlying != null) return IsSingleChildObject(underlying);
        return t.IsClass;
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
