using System.Collections.Generic;
using Going.UI.Controls;

namespace Going.UI.Collections;

/// <summary>
/// Common helper API for cell-indexed control collections (GoTableLayoutControlCollection,
/// GoGridLayoutControlCollection). Used by GoGudxConverter P3 dispatch (Gudx v1.2.1+) to
/// handle both collections through a single code path, replacing the v1.2.0 hardcoded
/// per-type cast pattern.
///
/// Design (helper form):
/// - Each implementing collection retains its existing native shape (e.g. GoTableIndex /
///   GoGridIndex with different fields). The interface only exposes enumeration and
///   addition helpers — it does not unify the underlying Indexes types.
/// - GoTableLayoutControlCollection supports cell span (ColSpan / RowSpan).
/// - GoGridLayoutControlCollection does not support span — EnumerateCells reports 1/1
///   and AddCell discards non-default span arguments.
///
/// To add a new cell-indexed container in the future, implement this interface — no Gudx
/// converter code change needed.
/// </summary>
public interface IGoCellIndexedControlCollection
{
    /// <summary>
    /// Enumerates every cell as (control, column, row, colSpan, rowSpan). Collections that
    /// don't support span report (1, 1).
    /// </summary>
    IEnumerable<(IGoControl Control, int Column, int Row, int ColSpan, int RowSpan)> EnumerateCells();

    /// <summary>
    /// Adds a control at the given cell. Collections without span support discard non-default
    /// colSpan / rowSpan values silently.
    /// </summary>
    void AddCell(IGoControl c, int col, int row, int colSpan = 1, int rowSpan = 1);

    /// <summary>
    /// Direct access to the controls list. Used by ReadP3 fallback path when an entry is
    /// missing the Cell attribute (cellAttr == null branch).
    /// </summary>
    IList<IGoControl> Controls { get; }
}
