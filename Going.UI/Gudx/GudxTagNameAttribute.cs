using System;

namespace Going.UI.Gudx;

/// <summary>
/// Overrides the XML tag name used for a [GoChilds] single-object (B1) property.
/// Default behavior: tag = property name (e.g., LeftSideBar → &lt;LeftSideBar&gt;).
/// Use this attribute when the desired tag differs from the property name
/// (e.g., CustomTheme → &lt;Theme&gt;).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class GudxTagNameAttribute : Attribute
{
    public string Name { get; }
    public GudxTagNameAttribute(string name) => Name = name;
}
