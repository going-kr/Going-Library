using System;

namespace Going.UI.Gudx;

/// <summary>
/// Marks a property as a single non-collection child object (Bonus B1).
/// Property type is any class (not a collection, not a primitive, not SKColor/SKRect).
/// XML: one child element using the property name as the default tag (disambiguates same-typed
/// siblings like LeftSideBar / RightSideBar). Use [GudxTagName("...")] alongside this attribute
/// to override the tag (e.g., CustomTheme → &lt;Theme&gt;).
/// Get-only properties (private set) are populated in-place via the existing default instance;
/// nullable / settable properties are constructed via Activator and assigned.
/// Example: GoDesign.TitleBar : GoTitleBar
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GoChildSingleAttribute : GoChildAttribute { }
