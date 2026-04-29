using System;

namespace Going.UI.Gudx;

/// <summary>
/// Marks a property as a cell-indexed collection of IGoControl children (Pattern P3).
/// Property type must be GoTableLayoutControlCollection or GoGridLayoutControlCollection (or compatible API:
/// .Controls : IList&lt;IGoControl&gt; + .Indexes : IDictionary&lt;Guid, TIndex&gt; with .Column/.Row [+ .ColSpan/.RowSpan] fields).
/// XML: child elements directly under the owning element, each with a Cell="col,row" attribute (or "col,row,colSpan,rowSpan" if non-default spans).
/// Example: GoTableLayoutPanel.Childrens, GoGridLayoutPanel.Childrens
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GoChildCellsAttribute : GoChildAttribute { }
