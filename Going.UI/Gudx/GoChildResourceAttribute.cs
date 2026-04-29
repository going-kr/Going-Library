using System;

namespace Going.UI.Gudx;

/// <summary>
/// Marks a property as an external file resource reference (Bonus B2).
/// Property type may be Dictionary&lt;string, T&gt; (image/font assets keyed by name)
/// or List&lt;TRef&gt; / single TRef (e.g., GoPageRef in Master file split — reserved for Task 12).
///
/// XML: emits each entry as &lt;TagName Name="..." File="..."&gt; — the asset is NOT inlined
/// (no base64). The Folder/Ext properties hint where the resource is stored relative to the
/// containing .gudx file (Phase 0 doesn't extract resources to disk; that's Step 3 wiring).
/// </summary>
/// <example>
/// [GoChildResource("Image", Folder = "resources", Ext = ".png")]
/// private Dictionary&lt;string, List&lt;SKImage&gt;&gt; Images { get; }
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GoChildResourceAttribute : GoChildAttribute
{
    /// <summary>The XML element tag name to emit per resource entry (e.g., "Image", "Font", "GoPageRef").</summary>
    public string TagName { get; }

    /// <summary>Optional folder relative to the master .gudx where resources live (e.g., "resources", "Pages").</summary>
    public string? Folder { get; set; }

    /// <summary>Optional file extension hint (e.g., ".png", ".ttf", ".gudx").</summary>
    public string? Ext { get; set; }

    public GoChildResourceAttribute(string tagName) { TagName = tagName; }
}
