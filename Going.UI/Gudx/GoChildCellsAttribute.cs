using System;

namespace Going.UI.Gudx;

/// <summary>
/// Marks a property as a cell-indexed collection of IGoControl children (Pattern P3).
/// Property type must be GoTableLayoutControlCollection or GoGridLayoutControlCollection (or compatible API:
/// .Controls : IList&lt;IGoControl&gt; + .Indexes : IDictionary&lt;Guid, TIndex&gt; with .Column/.Row [+ .ColSpan/.RowSpan] fields).
/// XML: child elements directly under the owning element, each with a Cell="col,row" attribute (or "col,row,colSpan,rowSpan" if non-default spans).
/// Example: GoTableLayoutPanel.Childrens, GoGridLayoutPanel.Childrens
///
/// Optional: set <see cref="NestInto"/> to nest children inside wrapper elements
/// from a sibling [GoChildWrappers] property. Children are grouped by their Row index,
/// placed inside the wrapper at that index. Without NestInto, cells emit as flat siblings.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GoChildCellsAttribute : GoChildAttribute
{
    /// <summary>
    /// Optional: name of a sibling property (must be marked [GoChildWrappers]) to nest into.
    /// When set, the converter emits the wrapper elements and places each cell child INSIDE
    /// the wrapper at its Row index. The sibling [GoChildWrappers] property is suppressed
    /// from default flat emission to avoid duplication.
    /// </summary>
    public string? NestInto { get; set; }
}
