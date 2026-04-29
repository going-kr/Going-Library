using System;

namespace Going.UI.Gudx;

/// <summary>
/// Marks a property as a list of wrapper objects (Pattern P4).
/// Property type must be List&lt;TWrapper&gt; where TWrapper is NOT IGoControl
/// (typically GoSubPage, GoGridLayoutPanelRow, GoTabPage, GoTrendSeries, etc.).
/// XML: each wrapper appears as a child element using the wrapper class name as tag,
/// with the wrapper's own [GoChild*]-marked children nested inside.
/// Example: GoSwitchPanel.Pages : List&lt;GoSubPage&gt;
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GoChildWrappersAttribute : GoChildAttribute { }
