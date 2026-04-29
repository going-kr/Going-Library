using System;

namespace Going.UI.Gudx;

/// <summary>
/// Abstract base for Gudx child-marker attributes. Marks a property as a Gudx child node container.
/// The GoGudxConverter dispatches by the concrete derived attribute type:
///   - GoChildList: List&lt;IGoControl&gt; (P2)
///   - GoChildCells: cell-indexed collection like GoTableLayoutControlCollection / GoGridLayoutControlCollection (P3)
///   - GoChildWrappers: List&lt;TWrapper&gt; where TWrapper is not IGoControl (P4)
///   - GoChildMap: Dictionary&lt;string, T&gt; (P5)
///   - GoChildSingle: single non-collection class (B1)
///   - GoChildResource: external file reference (B2)
///
/// A class may declare multiple [GoChild*] properties; the converter walks each in turn.
/// To find all child markers on a type: use prop.IsDefined(typeof(GoChildAttribute), inherit: true).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public abstract class GoChildAttribute : Attribute { }
