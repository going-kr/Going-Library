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

        // P2/P3/P4/P5/B1/B2: walk [GoChild*] properties.
        foreach (var childProp in ChildProperties(type))
        {
            // Skip explicitly excluded children (Master split: skips Pages/Windows/Images/Fonts on master serialize)
            if (skipChildren != null && skipChildren.Contains(childProp.Name)) continue;

            var value = childProp.GetValue(obj);
            if (value == null) continue;

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
    ///   - resources/{name}.png — one per GoDesign.Images entry (first SKImage encoded as PNG)
    ///   - resources/fonts/{name}.ttf — one per GoDesign.Fonts entry (first byte[] written as TTF)
    /// </summary>
    public static void SerializeGoDesignToFiles(GoDesign d, string masterPath)
    {
        var dir = Path.GetDirectoryName(masterPath)!;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var pagesDir = Path.Combine(dir, "Pages");
        var windowsDir = Path.Combine(dir, "Windows");
        if (d.Pages.Count > 0) Directory.CreateDirectory(pagesDir);
        if (d.Windows.Count > 0) Directory.CreateDirectory(windowsDir);

        // Master: suppress Pages/Windows (replaced by refs) AND Images/Fonts (extracted to resources/).
        var skip = new HashSet<string>(StringComparer.Ordinal) { "Pages", "Windows", "Images", "Fonts" };
        var masterElem = WriteAny(d, skipChildren: skip);

        foreach (var kvp in d.Pages)
        {
            var rel = $"Pages/{kvp.Key}.gudx";
            masterElem.Add(new XElement("GoPageRef", new XAttribute("File", rel)));

            // F2: use WriteElement so the page's Id is emitted as an attribute.
            var pageElem = WriteElement(kvp.Value);
            pageElem.SetAttributeValue("Name", kvp.Key);  // dict key as Name attribute
            File.WriteAllText(Path.Combine(pagesDir, kvp.Key + ".gudx"), pageElem.ToString());
        }

        foreach (var kvp in d.Windows)
        {
            var rel = $"Windows/{kvp.Key}.gudx";
            masterElem.Add(new XElement("GoWindowRef", new XAttribute("File", rel)));

            // F2: use WriteElement so the window's Id is emitted as an attribute.
            var windowElem = WriteElement(kvp.Value);
            windowElem.SetAttributeValue("Name", kvp.Key);
            File.WriteAllText(Path.Combine(windowsDir, kvp.Key + ".gudx"), windowElem.ToString());
        }

        // B2: Images — extract first SKImage per key as PNG under resources/
        var images = d.GetImages();
        if (images.Count > 0)
        {
            var resourcesDir = Path.Combine(dir, "resources");
            Directory.CreateDirectory(resourcesDir);
            foreach (var (name, list) in images)
            {
                if (list.Count == 0) continue;
                var rel = $"resources/{name}.png";
                var abs = Path.Combine(resourcesDir, name + ".png");
                using (var data = list[0].Encode(SKEncodedImageFormat.Png, 100))
                {
                    if (data == null) continue;  // unencodable — skip (extreme edge case)
                    File.WriteAllBytes(abs, data.ToArray());
                }
                masterElem.Add(new XElement("Image",
                    new XAttribute("Name", name),
                    new XAttribute("File", rel)));
            }
        }

        // B2: Fonts — extract first byte[] per key as TTF under resources/fonts/
        var fonts = d.GetFonts();
        if (fonts.Count > 0)
        {
            var fontsDir = Path.Combine(dir, "resources", "fonts");
            Directory.CreateDirectory(fontsDir);
            foreach (var (name, list) in fonts)
            {
                if (list.Count == 0) continue;
                var rel = $"resources/fonts/{name}.ttf";
                var abs = Path.Combine(fontsDir, name + ".ttf");
                File.WriteAllBytes(abs, list[0]);
                masterElem.Add(new XElement("Font",
                    new XAttribute("Name", name),
                    new XAttribute("File", rel)));
            }
        }

        File.WriteAllText(masterPath, masterElem.ToString());
    }

    /// <summary>
    /// Loads a GoDesign from a Master.gudx file, resolving GoPageRef/GoWindowRef references
    /// and loading Images/Fonts from the resources/ folder.
    /// </summary>
    public static GoDesign? DeserializeGoDesignFromFiles(string masterPath)
    {
        if (!File.Exists(masterPath)) return null;

        var dir = Path.GetDirectoryName(masterPath)!;
        var masterElem = XElement.Parse(File.ReadAllText(masterPath));

        var d = new GoDesign();
        // Suppress Images/Fonts dict population (handled below via <Image>/<Font> elements)
        var skip = new HashSet<string>(StringComparer.Ordinal) { "Pages", "Windows", "Images", "Fonts" };
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

        // B2: Images — load PNG files back via AddImage(name, byte[])
        foreach (var imgElem in masterElem.Elements("Image"))
        {
            var name = imgElem.Attribute("Name")?.Value;
            var file = imgElem.Attribute("File")?.Value;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(file)) continue;
            var abs = Path.Combine(dir, file.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(abs)) continue;  // missing file — skip silently (round-trip resilience)
            var data = File.ReadAllBytes(abs);
            d.AddImage(name, data);
        }

        // B2: Fonts — load TTF files back via AddFont(name, byte[])
        foreach (var fontElem in masterElem.Elements("Font"))
        {
            var name = fontElem.Attribute("Name")?.Value;
            var file = fontElem.Attribute("File")?.Value;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(file)) continue;
            var abs = Path.Combine(dir, file.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(abs)) continue;  // missing file — skip silently
            var data = File.ReadAllBytes(abs);
            d.AddFont(name, data);
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
            WriteP2_HomogeneousList(elem, childProp, value);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildCellsAttribute>() != null)
        {
            WriteP3_CellIndexed(elem, childProp, value);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildWrappersAttribute>() != null)
        {
            WriteP4_WrapperList(elem, childProp, value);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildMapAttribute>() != null)
        {
            WriteP5_KeyedDict(elem, childProp, value);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildSingleAttribute>() != null)
        {
            WriteB1_SingleChild(elem, childProp, value);
            return;
        }
        if (childProp.GetCustomAttribute<GoChildResourceAttribute>() != null)
        {
            // B2: [GoChildResource]-marked dicts (Images/Fonts) are suppressed here via the skipChildren
            // set in SerializeGoDesignToFiles. File extraction happens at the top level, not in dispatch.
            return;
        }
    }

    // ─── Write pattern helpers ───

    private static void WriteP2_HomogeneousList(XElement elem, PropertyInfo childProp, object value)
    {
        // P2 (v1.2.1+): IGoControl children inside a property-name group element.
        // The group element separates P2 children from sibling P4 wrappers — fixes the
        // v1.2.0 latent bug where ReadP2 unfiltered walk consumed P4 wrapper elements.
        var groupElem = new XElement(childProp.Name);
        foreach (var child in (System.Collections.IList)value)
            if (child is IGoControl c) groupElem.Add(WriteElement(c));
        elem.Add(groupElem);
    }

    private static void WriteP3_CellIndexed(XElement elem, PropertyInfo childProp, object value)
    {
        // P3 (v1.2.1+): cell-indexed collection in <PropertyName> (= <Childrens>) group element.
        // Dispatched via IGoCellIndexedControlCollection interface — handles both
        // GoTableLayoutControlCollection (with span) and GoGridLayoutControlCollection (no span).
        var coll = (Going.UI.Collections.IGoCellIndexedControlCollection)value;
        var groupElem = new XElement(childProp.Name);
        foreach (var (child, col, row, colSpan, rowSpan) in coll.EnumerateCells())
        {
            var childElem = WriteElement(child);
            var cell = (colSpan == 1 && rowSpan == 1)
                ? $"{col},{row}"
                : $"{col},{row},{colSpan},{rowSpan}";
            childElem.SetAttributeValue("Cell", cell);
            groupElem.Add(childElem);
        }
        elem.Add(groupElem);
    }

    private static void WriteP4_WrapperList(XElement elem, PropertyInfo childProp, object value)
    {
        // P4 (v1.2.1+): wrapper list emitted inside a property-name group element.
        // e.g. GoSwitchPanel.Pages : List<GoSubPage> → <Pages><GoSubPage .../></Pages>.
        // The group element ensures wrappers don't collide with sibling [GoChildList] (P2)
        // children — fixes the v1.2.0 P2+P4 mix latent bug (GudxP2P4MixTests).
        var groupElem = new XElement(childProp.Name);
        foreach (var item in (System.Collections.IEnumerable)value)
            groupElem.Add(WriteWrapper(item));
        elem.Add(groupElem);
    }

    private static void WriteP5_KeyedDict(XElement elem, PropertyInfo childProp, object value)
    {
        // P5 (v1.2.1+): keyed dictionary inside a property-name group element.
        // e.g. GoDesign.Pages : Dictionary<string, GoPage> → <Pages><GoPage Name="..."/></Pages>.
        var groupElem = new XElement(childProp.Name);
        foreach (System.Collections.DictionaryEntry kvp in (System.Collections.IDictionary)value)
        {
            if (kvp.Value == null) continue;
            // Use WriteElement for IGoControl values to include F2 Id attribute; otherwise WriteAny.
            var childElem = kvp.Value is IGoControl ctrl ? WriteElement(ctrl) : WriteAny(kvp.Value);
            // Overwrite Name attribute with the dictionary key to ensure round-trip fidelity.
            childElem.SetAttributeValue("Name", kvp.Key.ToString());
            groupElem.Add(childElem);
        }
        elem.Add(groupElem);
    }

    private static void WriteB1_SingleChild(XElement elem, PropertyInfo childProp, object value)
    {
        // B1: single-child object (Task 9) — LAST branch (most permissive predicate).
        // Tag = [GudxTagName] override if present, else property name (not class name).
        var tagName = childProp.GetCustomAttribute<GudxTagNameAttribute>()?.Name ?? childProp.Name;
        var childElem = WriteAny(value, tagName);
        // F2: if value is IGoControl, emit Id as system attribute for round-trip preservation.
        if (value is IGoControl ctrl)
        {
            var idProp = ctrl.GetType().GetProperty("Id");
            if (idProp != null && idProp.PropertyType == typeof(Guid))
            {
                var id = (Guid)(idProp.GetValue(ctrl) ?? Guid.Empty);
                if (id != Guid.Empty) childElem.SetAttributeValue("Id", id.ToString());
            }
        }
        elem.Add(childElem);
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

        // P2/P3/P4/P5/B1/B2: populate [GoChild*] properties from child elements.
        foreach (var childProp in ChildProperties(type))
        {
            // Skip explicitly excluded children (Master split: skips Pages/Windows/Images/Fonts on master deserialize)
            if (skipChildren != null && skipChildren.Contains(childProp.Name)) continue;
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
            // B2: [GoChildResource]-marked dicts (Images/Fonts) are suppressed here via the skipChildren
            // set in DeserializeGoDesignFromFiles. File loading happens at the top level, not in dispatch.
            return;
        }
    }

    // ─── Read pattern helpers ───

    private static void ReadP2_HomogeneousList(XElement elem, PropertyInfo childProp, object instance)
    {
        // P2 (v1.2.1+): IGoControl children read from inside a property-name group element.
        // Tag-filter via _gudxControlTypes registry — non-IGoControl tags are skipped (defensive;
        // the group element should only contain IGoControl entries by design, but skipping unknowns
        // protects against malformed input).
        // Missing group element = empty list (OK).
        var groupElem = elem.Element(childProp.Name);
        if (groupElem == null) return;

        var pType = childProp.PropertyType;
        var list = (System.Collections.IList)(childProp.GetValue(instance)
                    ?? Activator.CreateInstance(pType)!);
        foreach (var childElem in groupElem.Elements())
        {
            if (!_gudxControlTypes.ContainsKey(childElem.Name.LocalName)) continue;
            list.Add(ReadElement(childElem));
        }
        if (childProp.CanWrite) childProp.SetValue(instance, list);
    }

    private static void ReadP3_CellIndexed(XElement elem, PropertyInfo childProp, object instance)
    {
        // P3 (v1.2.1+): cell-indexed children read from inside <PropertyName> (= <Childrens>) group.
        // Dispatched via IGoCellIndexedControlCollection — handles both Table/Grid collections.
        // Tag-filter via _gudxControlTypes (defensive against malformed input).
        var groupElem = elem.Element(childProp.Name);
        if (groupElem == null) return;

        var existing = childProp.GetValue(instance);
        var coll = (Going.UI.Collections.IGoCellIndexedControlCollection)(existing
                    ?? Activator.CreateInstance(childProp.PropertyType)!);

        foreach (var childElem in groupElem.Elements())
        {
            if (!_gudxControlTypes.ContainsKey(childElem.Name.LocalName)) continue;
            var c = ReadElement(childElem);
            var cellAttr = childElem.Attribute("Cell");
            if (cellAttr != null)
            {
                var parts = cellAttr.Value.Split(',');
                var col = int.Parse(parts[0], CultureInfo.InvariantCulture);
                var row = int.Parse(parts[1], CultureInfo.InvariantCulture);
                var colSpan = parts.Length >= 4 ? int.Parse(parts[2], CultureInfo.InvariantCulture) : 1;
                var rowSpan = parts.Length >= 4 ? int.Parse(parts[3], CultureInfo.InvariantCulture) : 1;
                coll.AddCell(c, col, row, colSpan, rowSpan);
            }
            else
            {
                coll.Controls.Add(c);
            }
        }
    }

    private static void ReadP4_WrapperList(XElement elem, PropertyInfo childProp, object instance)
    {
        // P4 (v1.2.1+): wrapper list read from inside a property-name group element.
        // Missing group element = empty list (OK).
        var groupElem = elem.Element(childProp.Name);
        if (groupElem == null) return;

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

        foreach (var childElem in groupElem.Elements())
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
        // P5 (v1.2.1+): keyed dictionary read from inside a property-name group element.
        // The group ensures multiple [GoChildMap] properties on the same parent (e.g.
        // GoDesign.Pages + GoDesign.Windows) don't collide — each lives in its own group.
        // The v1.2.0 TAG-FILTERED workaround (elem.Elements(valueTagName)) is therefore
        // dropped — we now walk all entries inside the group element.
        var groupElem = elem.Element(childProp.Name);
        if (groupElem == null) return;

        var pType = childProp.PropertyType;
        IsKeyedDict(pType, out var valueType);
        var dict = (System.Collections.IDictionary)(childProp.GetValue(instance)
                    ?? Activator.CreateInstance(pType)!);
        foreach (var childElem in groupElem.Elements())
        {
            var key = childElem.Attribute("Name")?.Value
                ?? throw new InvalidOperationException(
                    $"P5 dict child <{childElem.Name.LocalName}> missing required Name attribute");
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
            // F2: if existing is IGoControl, restore Id from XML attribute.
            if (existing is IGoControl existingCtrl)
                RestoreIdFromAttr(childElem, existingCtrl);
            // If also writable (private set), we can leave it as-is (already mutated).
            return;
        }

        // existing is null (nullable property not yet initialized): create new instance.
        var instance2 = Activator.CreateInstance(actualType);
        if (instance2 != null)
        {
            PopulateAny(childElem, instance2);
            // F2: if instance2 is IGoControl, restore Id from XML attribute.
            if (instance2 is IGoControl ctrl2)
                RestoreIdFromAttr(childElem, ctrl2);
            // Use reflection to set even private setters.
            var setter = childProp.GetSetMethod(nonPublic: true);
            if (setter == null)
                throw new InvalidOperationException(
                    $"[GoChilds] B1 property {instance.GetType().Name}.{childProp.Name} has no setter and was null at read time. " +
                    $"Either initialize a default value or add a (private) setter.");
            setter.Invoke(instance, new[] { instance2 });
        }
    }

    /// <summary>
    /// F2 helper: restores the exact Guid from an "Id" XML attribute onto an IGoControl instance.
    /// Used after PopulateAny to override the Activator's auto-generated Id.
    /// </summary>
    private static void RestoreIdFromAttr(XElement elem, IGoControl ctrl)
    {
        var idAttr = elem.Attribute("Id");
        if (idAttr == null || !Guid.TryParse(idAttr.Value, out var xmlId)) return;
        var idProp = ctrl.GetType().GetProperty("Id");
        var setter = idProp?.GetSetMethod(nonPublic: true);
        try { setter?.Invoke(ctrl, new object[] { xmlId }); }
        catch { /* best-effort */ }
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
        if (t == typeof(TimeSpan)) return true;  // F4: TimeSpan scalar support
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
            TimeSpan ts => ts.ToString(),  // F4: TimeSpan scalar formatting (invariant "hh:mm:ss" or "d.hh:mm:ss")
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
        // F6: additional primitive integer types
        if (targetType == typeof(byte)) return byte.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(sbyte)) return sbyte.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(short)) return short.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(ushort)) return ushort.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(uint)) return uint.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(long)) return long.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(ulong)) return ulong.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(Guid)) return Guid.Parse(value);  // F3: Guid scalar parsing
        if (targetType == typeof(TimeSpan))  // F4: TimeSpan scalar parsing
            return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
        if (targetType == typeof(List<string>))
            return value == "" ? new List<string>() : value.Split(',').ToList();
        if (targetType == typeof(SKColor)) return GudxSpecialConverters.ParseSKColor(value);
        if (targetType == typeof(SKRect)) return GudxSpecialConverters.ParseSKRect(value);
        if (targetType == typeof(GoPadding)) return GudxSpecialConverters.ParseGoPadding(value);
        return null;
    }
}
