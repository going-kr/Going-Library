using System;

namespace Going.UI.Gudx;

/// <summary>
/// Marks a property as a cell-indexed collection of IGoControl children (Pattern P3).
///
/// Property type must implement <see cref="Going.UI.Collections.IGoCellIndexedControlCollection"/>
/// (e.g. GoTableLayoutControlCollection, GoGridLayoutControlCollection).
///
/// XML (v1.2.1+): children are emitted inside a property-name group element (=&lt;Childrens&gt;),
/// each with a Cell="col,row" attribute (or "col,row,colSpan,rowSpan" if non-default spans).
///
/// Example: GoTableLayoutPanel.Childrens, GoGridLayoutPanel.Childrens
/// </summary>
/// <remarks>
/// v1.2.0 had an optional <c>NestInto</c> property that grouped cells under a sibling
/// [GoChildWrappers] element by Row index (used only by GoGridLayoutPanel.Rows).
/// v1.2.1 removed NestInto for XAML-style flat emission — Rows is now an independent P4
/// metadata collection, and cell positions are pure attached <c>Cell</c> attributes on
/// children inside the Childrens group.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GoChildCellsAttribute : GoChildAttribute { }
