using System;

namespace Going.UI.Gudx;

/// <summary>
/// Marks a property as a keyed dictionary of named children (Pattern P5).
/// Property type must be Dictionary&lt;string, T&gt;.
/// XML: each entry appears as a child element using T's class name as tag, with Name="key" attribute
/// carrying the dictionary key. The converter tag-filters reads by T's type name to support
/// multiple [GoChildMap] properties on the same parent (e.g., GoDesign.Pages + GoDesign.Windows).
/// Example: GoDesign.Pages : Dictionary&lt;string, GoPage&gt;
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GoChildMapAttribute : GoChildAttribute { }
