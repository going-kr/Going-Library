using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Going.UI.Collections;
using Going.UI.Containers;
using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Design;
using Going.UI.Json;
using SkiaSharp;

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
    // Cache: base wrapper type → { class-name → derived concrete type }
    private static readonly Dictionary<Type, Dictionary<string, Type>> _wrapperDerivedTypesCache = new();
    private static readonly object _wrapperDerivedTypesLock = new();

    // Gudx-specific type registry: XML-safe tag name → Type.
    // Derived once from GoJsonConverter.ControlTypes by encoding '<' as '_' and dropping '>'.
    // Example: "GoInputNumber<double>" in ControlTypes → "GoInputNumber_double" here.
    private static readonly Dictionary<string, Type> _gudxControlTypes;

    // Reverse map: Type → XML-safe gudx tag name (avoids O(n) scan in TypeTagName hot path).
    private static readonly Dictionary<Type, string> _gudxTypeToTag;

    private static Dictionary<string, Type> GetWrapperDerivedTypes(Type baseType)
    {
        if (_wrapperDerivedTypesCache.TryGetValue(baseType, out var cached)) return cached;

        lock (_wrapperDerivedTypesLock)
        {
            if (_wrapperDerivedTypesCache.TryGetValue(baseType, out cached)) return cached;

            var dict = new Dictionary<string, Type>(StringComparer.Ordinal);
            // Include the base type itself if concrete (instantiable) and non-generic
            if (!baseType.IsAbstract && !baseType.IsGenericTypeDefinition)
                dict[baseType.Name] = baseType;
            // Scan the base type's assembly for concrete derived types
            foreach (var t in baseType.Assembly.GetTypes())
            {
                if (t == baseType) continue;
                if (t.IsAbstract) continue;
                if (t.IsGenericTypeDefinition) continue;  // skip open generics (e.g. GoDataGridNumberColumn<T>)
                if (!baseType.IsAssignableFrom(t)) continue;
                dict[t.Name] = t;
            }
            _wrapperDerivedTypesCache[baseType] = dict;
            return dict;
        }
    }

    static GoGudxConverter()
    {
        // Force GoJsonConverter's static ctor to run so ControlTypes is populated.
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(
            typeof(GoJsonConverter).TypeHandle);

        // Build _gudxControlTypes: XML-safe name → Type.
        // Encoding rule: replace '<' with '_', drop '>'.
        // "GoInputNumber<double>" → "GoInputNumber_double"
        // "GoButton" → "GoButton" (unchanged)
        _gudxControlTypes = new Dictionary<string, Type>(StringComparer.Ordinal);
        _gudxTypeToTag    = new Dictionary<Type, string>();
        foreach (var kvp in GoJsonConverter.ControlTypes)
        {
            var xmlTag = JsonTagToXmlTag(kvp.Key);
            // ControlTypes may have multiple aliases per type (e.g. "GoInputNumber<int>" + "GoInputNumber<Int32>")
            // Keep first registered entry per XML tag; add all type→tag mappings (last writer wins for type→tag).
            if (!_gudxControlTypes.ContainsKey(xmlTag))
                _gudxControlTypes[xmlTag] = kvp.Value;
            // For type→tag prefer the lower-case alias (shorter, stable) — first registered wins.
            if (!_gudxTypeToTag.ContainsKey(kvp.Value))
                _gudxTypeToTag[kvp.Value] = xmlTag;
        }
    }

    /// <summary>
    /// Encodes a JSON/C# type name to an XML-safe Gudx tag name.
    /// Replaces '&lt;' with '_' and drops '&gt;'.
    /// e.g. "GoInputNumber&lt;double&gt;" → "GoInputNumber_double"
    /// </summary>
    private static string JsonTagToXmlTag(string jsonTag)
        => jsonTag.Replace('<', '_').Replace(">", "");

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
    public static XElement WriteAny(object obj, string? explicitTagName = null, ISet<string>? skipChildren = null)
    {
        var type = obj.GetType();
        var tagName = explicitTagName ?? TypeTagName(type);
        var elem = new XElement(tagName);

        // P1: scalar properties become attributes.
        foreach (var prop in ScalarProperties(type))
        {
            var value = prop.GetValue(obj);
            if (value == null) continue;
            var s = FormatScalar(value);
            if (s != null) elem.SetAttributeValue(prop.Name, s);
        }

        // Identify wrappers that are nest-targets — they're emitted via the nested cells dispatch, not standalone.
        var childPropsList = ChildProperties(type).ToList();
        var nestedTargets = new HashSet<string>(StringComparer.Ordinal);
        foreach (var p in childPropsList)
        {
            var cellsAttr = p.GetCustomAttribute<GoChildCellsAttribute>();
            if (cellsAttr?.NestInto != null) nestedTargets.Add(cellsAttr.NestInto);
        }

        // P2/P3/P4/P5: walk [GoChilds] properties.
        foreach (var childProp in childPropsList)
        {
            // Skip explicitly excluded children (Task 12 Master split)
            if (skipChildren != null && skipChildren.Contains(childProp.Name)) continue;

            var value = childProp.GetValue(obj);
            if (value == null) continue;

            // Skip wrappers that are nest-targets — handled via the cells dispatch
            if (childProp.GetCustomAttribute<GoChildWrappersAttribute>() != null
                && nestedTargets.Contains(childProp.Name))
            {
                continue;
            }

            DispatchChildWrite(elem, childProp, value, obj);
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

    /// <summary>
    /// Serializes a GoDesign across multiple .gudx files:
    ///   - {masterPath} contains GoDesign root + theme + bars + &lt;GoPageRef/&gt;/&lt;GoWindowRef/&gt; references
    ///   - Pages/&lt;name&gt;.gudx (relative to masterPath's directory) — one per GoPage
    ///   - Windows/&lt;name&gt;.gudx — one per GoWindow
    /// </summary>
    public static void SerializeGoDesignToFiles(GoDesign d, string masterPath)
    {
        var dir = Path.GetDirectoryName(masterPath)!;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var pagesDir = Path.Combine(dir, "Pages");
        var windowsDir = Path.Combine(dir, "Windows");
        if (d.Pages.Count > 0) Directory.CreateDirectory(pagesDir);
        if (d.Windows.Count > 0) Directory.CreateDirectory(windowsDir);

        // Master: emit GoDesign root with Pages/Windows dicts SUPPRESSED (replaced by refs below)
        var skip = new HashSet<string>(StringComparer.Ordinal) { "Pages", "Windows" };
        var masterElem = WriteAny(d, skipChildren: skip);

        foreach (var kvp in d.Pages)
        {
            var rel = $"Pages/{kvp.Key}.gudx";
            masterElem.Add(new XElement("GoPageRef", new XAttribute("File", rel)));

            var pageElem = WriteAny(kvp.Value);
            pageElem.SetAttributeValue("Name", kvp.Key);  // dict key as Name attribute
            File.WriteAllText(Path.Combine(pagesDir, kvp.Key + ".gudx"), pageElem.ToString());
        }

        foreach (var kvp in d.Windows)
        {
            var rel = $"Windows/{kvp.Key}.gudx";
            masterElem.Add(new XElement("GoWindowRef", new XAttribute("File", rel)));

            var windowElem = WriteAny(kvp.Value);
            windowElem.SetAttributeValue("Name", kvp.Key);
            File.WriteAllText(Path.Combine(windowsDir, kvp.Key + ".gudx"), windowElem.ToString());
        }

        File.WriteAllText(masterPath, masterElem.ToString());
    }

    /// <summary>
    /// Loads a GoDesign from a Master.gudx file, resolving GoPageRef/GoWindowRef references.
    /// </summary>
    public static GoDesign? DeserializeGoDesignFromFiles(string masterPath)
    {
        if (!File.Exists(masterPath)) return null;

        var dir = Path.GetDirectoryName(masterPath)!;
        var masterElem = XElement.Parse(File.ReadAllText(masterPath));

        var d = new GoDesign();
        var skip = new HashSet<string>(StringComparer.Ordinal) { "Pages", "Windows" };
        PopulateAny(masterElem, d, skipChildren: skip);

        foreach (var refElem in masterElem.Elements("GoPageRef"))
        {
            var file = refElem.Attribute("File")?.Value
                ?? throw new FormatException("GoPageRef missing File attribute");
            var pagePath = Path.Combine(dir, file.Replace('/', Path.DirectorySeparatorChar));
            var pageElem = XElement.Parse(File.ReadAllText(pagePath));
            var page = ReadElement(pageElem) as GoPage
                ?? throw new FormatException($"Page file {file} root is not <GoPage>");
            // Use the Name attribute as the dict key (set during serialize)
            var name = pageElem.Attribute("Name")?.Value;
            if (!string.IsNullOrEmpty(name) && page.Name == null) page.Name = name;
            if (page.Name != null) d.AddPage(page);
        }

        foreach (var refElem in masterElem.Elements("GoWindowRef"))
        {
            var file = refElem.Attribute("File")?.Value
                ?? throw new FormatException("GoWindowRef missing File attribute");
            var windowPath = Path.Combine(dir, file.Replace('/', Path.DirectorySeparatorChar));
            var windowElem = XElement.Parse(File.ReadAllText(windowPath));
            var window = ReadElement(windowElem) as GoWindow
                ?? throw new FormatException($"Window file {file} root is not <GoWindow>");
            var name = windowElem.Attribute("Name")?.Value;
            if (!string.IsNullOrEmpty(name) && window.Name == null) window.Name = name;
            if (window.Name != null) d.Windows[window.Name] = window;
        }

        return d;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internal write helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Thin facade over WriteAny for IGoControl. Emits the Id as a system attribute
    /// for round-trip preservation (F2: IGoControl Id round-trip).
    /// </summary>
    internal static XElement WriteElement(IGoControl control)
    {
        var elem = WriteAny(control);
        // F2: emit Id as system attribute so ReadElement can restore the exact same Guid.
        // Id is not in ScalarProperties (no [GoProperty]) so we handle it specially here.
        var idProp = control.GetType().GetProperty("Id");
        if (idProp != null && idProp.PropertyType == typeof(Guid))
        {
            var id = (Guid)(idProp.GetValue(control) ?? Guid.Empty);
            if (id != Guid.Empty)
                elem.SetAttributeValue("Id", id.ToString());
        }
        return elem;
    }

    private static void DispatchChildWrite(XElement elem, PropertyInfo childProp, object value, object parentInstance)
    {
        // Order: P2 → P3 → P4 → P5 → B1 → B2. New attribute family takes precedence.

        // ─── NEW dispatch by attribute type ───
        if (childProp.GetCustomAttribute<GoChildListAttribute>() != null)
        {
            WriteP2_HomogeneousList(elem, value);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildCellsAttribute>() != null)
        {
            var cellsAttr = childProp.GetCustomAttribute<GoChildCellsAttribute>()!;
            if (cellsAttr.NestInto != null)
                WriteP3_CellIndexed_NestedIn(elem, childProp, value, parentInstance, cellsAttr.NestInto);
            else
                WriteP3_CellIndexed(elem, value);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildWrappersAttribute>() != null)
        {
            WriteP4_WrapperList(elem, value);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildMapAttribute>() != null)
        {
            WriteP5_KeyedDict(elem, value);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildSingleAttribute>() != null)
        {
            WriteB1_SingleChild(elem, childProp, value);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildResourceAttribute>() != null)
        {
            // B2: deferred to Task 11. Suppress emission for now.
            return;
        }
    }

    // ─── Write pattern helpers ───

    private static void WriteP2_HomogeneousList(XElement elem, object value)
    {
        // P2: list of IGoControl — use WriteElement to include F2 Id attribute
        foreach (var child in (System.Collections.IList)value)
            if (child is IGoControl c) elem.Add(WriteElement(c));
    }

    private static void WriteP3_CellIndexed(XElement elem, object value)
    {
        // P3: cell-indexed collection (GoTableLayoutPanel) — use WriteElement to include F2 Id attribute
        var tlc = (GoTableLayoutControlCollection)value;
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

    internal static void WriteP3_CellIndexed_NestedIn(
        XElement elem,
        PropertyInfo cellsProp,
        object cellsValue,
        object parentInstance,
        string wrapperPropName)
    {
        var wrapperProp = parentInstance.GetType().GetProperty(wrapperPropName)
            ?? throw new InvalidOperationException(
                $"[GoChildCells(NestInto = \"{wrapperPropName}\")] target property not found on {parentInstance.GetType().Name}");
        var wrappers = (System.Collections.IList)(wrapperProp.GetValue(parentInstance)
            ?? throw new InvalidOperationException(
                $"[GoChildCells(NestInto = \"{wrapperPropName}\")] target list is null"));

        // Emit each wrapper as <WrapperType ...>; nest cells with matching Row index inside.
        var collection = cellsValue as GoGridLayoutControlCollection
            ?? throw new InvalidOperationException(
                $"[GoChildCells(NestInto)] currently supports GoGridLayoutControlCollection only; got {cellsValue.GetType().Name}");

        for (int rowIdx = 0; rowIdx < wrappers.Count; rowIdx++)
        {
            var rowElem = WriteWrapper(wrappers[rowIdx]!);
            foreach (var child in collection.Controls)
            {
                if (collection.Indexes.TryGetValue(child.Id, out var idx) && idx.Row == rowIdx)
                {
                    var childElem = WriteElement(child);
                    childElem.SetAttributeValue("Cell", $"{idx.Column},{idx.Row}");
                    rowElem.Add(childElem);
                }
            }
            elem.Add(rowElem);
        }
    }

    internal static void ReadP3_CellIndexed_NestedIn(
        XElement elem,
        PropertyInfo cellsProp,
        object instance,
        string wrapperPropName)
    {
        var wrapperProp = instance.GetType().GetProperty(wrapperPropName)
            ?? throw new InvalidOperationException(
                $"[GoChildCells(NestInto = \"{wrapperPropName}\")] target property not found on {instance.GetType().Name}");
        var wrappers = (System.Collections.IList)(wrapperProp.GetValue(instance)
            ?? Activator.CreateInstance(wrapperProp.PropertyType)!);

        var wrapperType = wrapperProp.PropertyType.GetGenericArguments()[0];
        var wrapperTagName = wrapperType.Name;

        var collection = (GoGridLayoutControlCollection)(cellsProp.GetValue(instance)
            ?? new GoGridLayoutControlCollection());

        int rowIdx = 0;
        foreach (var rowElem in elem.Elements(wrapperTagName))
        {
            var wrapper = ReadWrapper(rowElem, wrapperType);
            wrappers.Add(wrapper);

            foreach (var childElem in rowElem.Elements())
            {
                // Skip nested wrapper-type tags (defensive — not expected here)
                if (childElem.Name.LocalName == wrapperTagName) continue;

                var c = ReadElement(childElem);
                var cell = childElem.Attribute("Cell")?.Value;
                if (cell != null)
                {
                    var parts = cell.Split(',');
                    var col = int.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                    // ignore parts[1] — Row is implicit from rowIdx
                    collection.Add(c, col, rowIdx);
                }
            }
            rowIdx++;
        }

        if (wrapperProp.CanWrite) wrapperProp.SetValue(instance, wrappers);
        if (cellsProp.CanWrite) cellsProp.SetValue(instance, collection);
    }

    private static void WriteP4_WrapperList(XElement elem, object value)
    {
        // P4a: wrapper-typed collection (e.g. GoSwitchPanel.Pages : List<GoSubPage>)
        foreach (var item in (System.Collections.IEnumerable)value)
            elem.Add(WriteWrapper(item));
    }

    private static void WriteP5_KeyedDict(XElement elem, object value)
    {
        // P5: keyed dictionary (e.g. GoDesign.Pages : Dictionary<string, GoPage>)
        foreach (System.Collections.DictionaryEntry kvp in (System.Collections.IDictionary)value)
        {
            if (kvp.Value == null) continue;
            // Use WriteElement for IGoControl values to include F2 Id attribute; otherwise WriteAny.
            var childElem = kvp.Value is IGoControl ctrl ? WriteElement(ctrl) : WriteAny(kvp.Value);
            // Overwrite Name attribute with the dictionary key to ensure round-trip fidelity.
            childElem.SetAttributeValue("Name", kvp.Key.ToString());
            elem.Add(childElem);
        }
    }

    private static void WriteB1_SingleChild(XElement elem, PropertyInfo childProp, object value)
    {
        // B1: single-child object (Task 9) — LAST branch (most permissive predicate).
        // Tag = [GudxTagName] override if present, else property name (not class name).
        var tagName = childProp.GetCustomAttribute<GudxTagNameAttribute>()?.Name ?? childProp.Name;
        elem.Add(WriteAny(value, tagName));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Internal read helpers
    // ─────────────────────────────────────────────────────────────────────────

    internal static IGoControl ReadElement(XElement elem)
    {
        var tagName = elem.Name.LocalName;
        if (!_gudxControlTypes.TryGetValue(tagName, out var type))
            throw new InvalidOperationException($"Unknown Gudx tag: {tagName}");

        var instance = (IGoControl)Activator.CreateInstance(type)!;
        PopulateAny(elem, instance);

        // F2: prefer XML-provided Id over the Activator's auto-generated Guid.
        // This restores the exact Id that was serialized (needed for P3 cell-index keys).
        var idAttr = elem.Attribute("Id");
        if (idAttr != null && Guid.TryParse(idAttr.Value, out var xmlId))
        {
            var idProp = type.GetProperty("Id");
            var setter = idProp?.GetSetMethod(nonPublic: true);
            try { setter?.Invoke(instance, new object[] { xmlId }); }
            catch { /* best-effort: auto-init Id is acceptable fallback */ }
        }

        // Defensive: ensure non-empty Id (P3 cell-index dict uses Id as key)
        EnsureNonEmptyId(instance);

        return instance;
    }

    /// <summary>
    /// Defensive lazy Id assignment: if an IGoControl has Id == Guid.Empty after deserialization,
    /// assigns a fresh Guid via reflection. This ensures P3 cell-index dict lookups (which use Id as key) work correctly.
    /// </summary>
    private static void EnsureNonEmptyId(object obj)
    {
        var idProp = obj.GetType().GetProperty("Id");
        if (idProp == null) return;
        if (idProp.PropertyType != typeof(Guid)) return;

        var current = (Guid)(idProp.GetValue(obj) ?? Guid.Empty);
        if (current != Guid.Empty) return;

        // Try to invoke the setter (public OR non-public, including init-only via reflection)
        var setter = idProp.GetSetMethod(nonPublic: true);
        if (setter == null) return;

        try
        {
            setter.Invoke(obj, new object[] { Guid.NewGuid() });
        }
        catch
        {
            // Best-effort: if reflection can't reach the setter (e.g., sealed init), silently continue.
            // Most cases have auto-init Id = Guid.NewGuid() and will never reach here.
        }
    }

    /// <summary>
    /// Populates an existing object instance from an XElement using reflection.
    /// Handles P1 scalar attributes and [GoChilds] child dispatching.
    /// Works for IGoControl and non-IGoControl types (GoDesign, GoPage, GoWindow, wrappers).
    /// </summary>
    internal static void PopulateAny(XElement elem, object instance, ISet<string>? skipChildren = null)
    {
        var type = instance.GetType();

        // P1: read attributes onto scalar properties.
        foreach (var attr in elem.Attributes())
        {
            var prop = type.GetProperty(attr.Name.LocalName);
            if (prop == null || !prop.CanWrite) continue;
            var parsed = ParseScalar(prop.PropertyType, attr.Value);
            if (parsed != null) prop.SetValue(instance, parsed);
        }

        // Identify wrappers that are nest-targets — they're read via the nested cells dispatch, not standalone.
        var childPropsList = ChildProperties(type).ToList();
        var nestedTargets = new HashSet<string>(StringComparer.Ordinal);
        foreach (var p in childPropsList)
        {
            var cellsAttr = p.GetCustomAttribute<GoChildCellsAttribute>();
            if (cellsAttr?.NestInto != null) nestedTargets.Add(cellsAttr.NestInto);
        }

        // P2/P3/P4/P5: populate [GoChilds] properties from child elements.
        foreach (var childProp in childPropsList)
        {
            // Skip explicitly excluded children (Task 12 Master split)
            if (skipChildren != null && skipChildren.Contains(childProp.Name)) continue;

            // Skip wrappers that are nest-targets — handled via the cells dispatch
            if (childProp.GetCustomAttribute<GoChildWrappersAttribute>() != null
                && nestedTargets.Contains(childProp.Name))
            {
                continue;
            }
            DispatchChildRead(elem, childProp, instance);
        }
    }

    private static void DispatchChildRead(XElement elem, PropertyInfo childProp, object instance)
    {
        // Order: P2 → P3 → P4 → P5 → B1 → B2. New attribute family takes precedence.

        // ─── NEW dispatch by attribute type ───
        if (childProp.GetCustomAttribute<GoChildListAttribute>() != null)
        {
            ReadP2_HomogeneousList(elem, childProp, instance);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildCellsAttribute>() != null)
        {
            var cellsAttr = childProp.GetCustomAttribute<GoChildCellsAttribute>()!;
            if (cellsAttr.NestInto != null)
                ReadP3_CellIndexed_NestedIn(elem, childProp, instance, cellsAttr.NestInto);
            else
                ReadP3_CellIndexed(elem, childProp, instance);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildWrappersAttribute>() != null)
        {
            ReadP4_WrapperList(elem, childProp, instance);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildMapAttribute>() != null)
        {
            ReadP5_KeyedDict(elem, childProp, instance);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildSingleAttribute>() != null)
        {
            ReadB1_SingleChild(elem, childProp, instance);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildResourceAttribute>() != null)
        {
            // B2: deferred to Task 11. No-op read.
            return;
        }
    }

    // ─── Read pattern helpers ───

    private static void ReadP2_HomogeneousList(XElement elem, PropertyInfo childProp, object instance)
    {
        // P2: list of IGoControl.
        // Design decision: P2 read is UNFILTERED (consumes all child elements).
        // This is safe because no type currently has BOTH a P2 and a P5 [GoChilds] property.
        // GoDesign's [GoChilds] properties are P5 only. If a future type mixes P2 + P5,
        // P2 will need the same tag-filter as P5 (check GoJsonConverter.ControlTypes lookup).
        var pType = childProp.PropertyType;
        var list = (System.Collections.IList)(childProp.GetValue(instance)
                    ?? Activator.CreateInstance(pType)!);
        foreach (var childElem in elem.Elements())
            list.Add(ReadElement(childElem));
        if (childProp.CanWrite) childProp.SetValue(instance, list);
    }

    private static void ReadP3_CellIndexed(XElement elem, PropertyInfo childProp, object instance)
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

    private static void ReadP4_WrapperList(XElement elem, PropertyInfo childProp, object instance)
    {
        var pType = childProp.PropertyType;
        var listObj = childProp.GetValue(instance) ?? Activator.CreateInstance(pType)!;
        var baseType = pType.GetGenericArguments()[0];

        // Tag name = class name. Build a map of acceptable tags (base + derived concrete types in baseType's assembly).
        var derivedMap = GetWrapperDerivedTypes(baseType);

        var addMethod = pType.GetMethod("Add", new[] { baseType })
            ?? throw new InvalidOperationException(
                $"P4 wrapper list type {pType.Name} has no Add({baseType.Name}) method");

        var genericDef = pType.GetGenericTypeDefinition();
        var isStandardList = genericDef == typeof(List<>);
        var isObservableList = genericDef == typeof(ObservableList<>);

        if (!isStandardList && !isObservableList)
        {
            throw new InvalidOperationException(
                $"P4 wrapper list type {pType.Name} not supported (expected List<T> or ObservableList<T>)");
        }

        foreach (var childElem in elem.Elements())
        {
            var tagName = childElem.Name.LocalName;
            if (!derivedMap.TryGetValue(tagName, out var actualType)) continue;

            var item = ReadWrapper(childElem, actualType);

            if (isStandardList)
                ((System.Collections.IList)listObj).Add(item);
            else
                addMethod.Invoke(listObj, new[] { item });
        }

        if (childProp.CanWrite) childProp.SetValue(instance, listObj);
    }

    private static void ReadP5_KeyedDict(XElement elem, PropertyInfo childProp, object instance)
    {
        // P5: keyed dictionary — TAG-FILTERED by value type's tag name (Task 7 review I-3).
        // Each [GoChilds] dict property on a multi-property type (e.g. GoDesign.Pages +
        // GoDesign.Windows) reads ONLY elements whose LocalName matches its value type.
        // This prevents one property from consuming elements destined for another.
        var pType = childProp.PropertyType;
        IsKeyedDict(pType, out var valueType);
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
    }

    private static void ReadB1_SingleChild(XElement elem, PropertyInfo childProp, object instance)
    {
        // B1: single-child object (Task 9) — LAST branch (most permissive predicate).
        // Tag-filtered by property name (or [GudxTagName] override) to avoid consuming
        // elements destined for other [GoChilds] properties.
        var pType = childProp.PropertyType;
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
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Wrapper serialization (P4)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Serializes a non-IGoControl wrapper object (e.g. GoSubPage, GoGridLayoutPanelRow, GoTreeNode)
    /// to an XElement using the type name as tag and reflection for P1 scalars.
    /// All [GoChilds] child properties are dispatched via DispatchChildWrite (P2, P4, etc.).
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

        // Full dispatch: emit all [GoChilds] properties (P2 IGoControl lists, P4 wrapper lists, etc.)
        foreach (var childProp in ChildProperties(type))
        {
            var v = childProp.GetValue(item);
            if (v == null) continue;
            DispatchChildWrite(elem, childProp, v, item);
        }

        return elem;
    }

    /// <summary>
    /// Deserializes a non-IGoControl wrapper object from an XElement.
    /// Uses PopulateAny for full dispatch (P1 scalars, P2 IGoControl lists, P4 nested wrappers, etc.).
    /// </summary>
    internal static object ReadWrapper(XElement elem, Type type)
    {
        var instance = Activator.CreateInstance(type)!;
        PopulateAny(elem, instance);
        return instance;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Reflection helpers
    // ─────────────────────────────────────────────────────────────────────────

    internal static string TypeTagName(Type t)
    {
        // For registered control types (including closed generics), use the pre-computed XML-safe tag.
        if (_gudxTypeToTag.TryGetValue(t, out var tag)) return tag;

        // Non-generic, non-registered types: use class name directly (already XML-safe).
        return t.Name;
    }

    private static IEnumerable<PropertyInfo> ChildProperties(Type t) =>
        t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
         .Where(p => p.IsDefined(typeof(GoChildAttribute), inherit: true));

    private static bool IsHomogeneousControlList(Type t)
    {
        if (!t.IsGenericType) return false;
        if (t.GetGenericTypeDefinition() != typeof(List<>)) return false;
        var arg = t.GetGenericArguments()[0];
        return typeof(IGoControl).IsAssignableFrom(arg);
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

    private static IEnumerable<PropertyInfo> ScalarProperties(Type t)
    {
        return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<GoPropertyAttribute>(inherit: true) != null)
                .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => IsScalar(p.PropertyType));
    }

    private static bool IsScalar(Type t)
    {
        // F3: Nullable<T> — recurse on underlying type (e.g. float? → float, int? → int)
        var underlying = Nullable.GetUnderlyingType(t);
        if (underlying != null) return IsScalar(underlying);

        if (t.IsPrimitive || t == typeof(string) || t == typeof(decimal)) return true;
        if (t.IsEnum) return true;
        if (t == typeof(Guid)) return true;  // F3 bonus: Guid is scalar (also helps F2 indirectly)
        if (t == typeof(List<string>)) return true;
        if (t == typeof(SKColor) || t == typeof(SKRect) || t == typeof(GoPadding)) return true;
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
            Guid g => g.ToString(),  // F3: Guid scalar formatting
            Enum e => e.ToString(),
            SKColor sc => GudxSpecialConverters.FormatSKColor(sc),
            SKRect sr => GudxSpecialConverters.FormatSKRect(sr),
            GoPadding gp => GudxSpecialConverters.FormatGoPadding(gp),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }

    private static object? ParseScalar(Type targetType, string value)
    {
        // F3: Nullable<T> — parse as T then box (CLR auto-boxes T to T? on assignment)
        var underlying = Nullable.GetUnderlyingType(targetType);
        if (underlying != null) return ParseScalar(underlying, value);

        if (targetType == typeof(string)) return value;
        if (targetType == typeof(bool)) return value.Equals("true", StringComparison.OrdinalIgnoreCase);
        if (targetType.IsEnum) return Enum.Parse(targetType, value, ignoreCase: false);
        if (targetType == typeof(int)) return int.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(float)) return float.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(double)) return double.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(decimal)) return decimal.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(Guid)) return Guid.Parse(value);  // F3: Guid scalar parsing
        if (targetType == typeof(List<string>))
            return value == "" ? new List<string>() : value.Split(',').ToList();
        if (targetType == typeof(SKColor)) return GudxSpecialConverters.ParseSKColor(value);
        if (targetType == typeof(SKRect)) return GudxSpecialConverters.ParseSKRect(value);
        if (targetType == typeof(GoPadding)) return GudxSpecialConverters.ParseGoPadding(value);
        return null;
    }
}
