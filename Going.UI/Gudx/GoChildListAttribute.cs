using System;

namespace Going.UI.Gudx;

/// <summary>
/// Marks a property as a homogeneous list of IGoControl children (Pattern P2).
/// Property type must be List&lt;T&gt; where T : IGoControl (or List&lt;IGoControl&gt; itself).
/// XML: each child element appears directly under the owning element, with the IGoControl class name as tag.
/// Example: GoBoxPanel.Childrens : List&lt;IGoControl&gt;
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GoChildListAttribute : GoChildAttribute { }
