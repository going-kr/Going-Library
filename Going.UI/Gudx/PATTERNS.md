# Gudx Patterns

This document is the design reference for the `GoGudxConverter` reflection algorithm
(Tasks 4-12). When the converter walks a `[GoChilds]` property it picks one of P2/P3/P4/P5/B1
based on the property's runtime type. P1 covers all scalar attributes on the owning element.

---

## P1. Attribute property
- Source: `[GoProperty]` scalar (`string|int|float|bool|enum`) or special type (`SKColor|SKRect`); also `[GoSizeProperty]` / `[GoSizesProperty]` for size strings
- XML: PascalCase attribute on owning element
- Example: `<GoButton Text="Hello" FontSize="14" Color="Good"/>`
- Also covers `GoTableLayoutPanel.Columns`/`.Rows` (`List<string>` serialized as a single delimited attribute value, e.g. `Columns="50%,50%"` `Rows="100%"`)

---

## P2. Homogeneous child collection
- Source: `[GoChilds] List<IGoControl> Childrens`
- XML: child elements directly under owner; each child's class name is the XML tag
- Containers: `GoBoxPanel.Childrens`, `GoGroupBox.Childrens`, `GoPanel.Childrens`,
  `GoScrollablePanel.Childrens`, `GoScalePanel.Childrens`, `GoPicturePanel.Childrens`,
  `GoPage.Childrens`, `GoWindow.Childrens`, `GoDropDownWindow.Childrens`,
  `GoSubPage.Childrens`, `GoTabPage.Childrens`,
  `GoTitleBar.Childrens`, `GoSideBar.Childrens`, `GoFooter.Childrens`
- Cell layout: when a P2 container is itself the child of a P3 wrapper, the child also
  carries a `Cell="c,r"` attribute that P3 serialization emits (see P3)

---

## P3. Cell-indexed child collection
- Source: `[GoChilds] GoTableLayoutControlCollection Childrens` (holds `Controls: List<IGoControl>` + `Indexes: Dictionary<Guid,GoTableIndex>`)
  or `[GoChilds] GoGridLayoutControlCollection Childrens` (same shape, `Indexes: Dictionary<Guid,GoGridIndex>`)
- XML: child elements under owner; each child carries a `Cell="c,r"` attribute (and optionally `ColSpan`/`RowSpan` for GoTableLayoutPanel)
- Containers:
  - `GoTableLayoutPanel.Childrens` (`GoTableLayoutControlCollection` — supports ColSpan/RowSpan via `GoTableIndex`)
  - `GoGridLayoutPanel.Childrens` (`GoGridLayoutControlCollection` — row/col only, no span, via `GoGridIndex`)
- Special: `Columns`/`Rows` on `GoTableLayoutPanel` are P1 attributes (`Columns="50%,50%"` `Rows="100%"`);
  `GoGridLayoutPanel` uses P4 (see below) for its row definitions instead

---

## P4. Wrapper-typed child collection
- Source: `[GoChilds] List<TWrapper>` where `TWrapper` is *not* `IGoControl`
- XML: child elements use `TWrapper` class name as tag, then their own children inside
- Cases:
    a. `GoSwitchPanel.Pages : List<GoSubPage>` → `<GoSubPage Name="…">…</GoSubPage>` children
       (`GoSubPage.Childrens` is itself a P2 collection — recursion)
    b. `GoGridLayoutPanel.Rows : List<GoGridLayoutPanelRow>` → `<GoGridLayoutPanelRow Height="…" Columns="…">…</GoGridLayoutPanelRow>`
       (controls inside the row are referenced from the companion P3 `GoGridLayoutControlCollection`; the row wrapper carries only its P1 properties `Height` and `Columns`)
    c. `GoTabControl.TabPages : List<GoTabPage>` → `<GoTabPage Name="…" IconString="…" Text="…">…</GoTabPage>`
       (`GoTabPage.Childrens` is itself a P2 collection — recursion)
- Wrapper class itself may have its own `[GoChilds]` property (recursion — see cases a and c).

---

## P5. Keyed dictionary
- Source: `[GoChilds] Dictionary<string, T>` where `T` is a poly type (`GoPage|GoWindow`)
- XML: child elements whose `Name` attribute supplies the dictionary key
- Cases:
  - `GoDesign.Pages : Dictionary<string, GoPage>` → `<GoPage Name="key">…</GoPage>` children
  - `GoDesign.Windows : Dictionary<string, GoWindow>` → `<GoWindow Name="key">…</GoWindow>` children

---

## Bonus B1. Single-child object
- Source: `[GoChilds] T` (single concrete instance, not a collection)
- XML: one child element with the class name as tag
- Cases (GoDesign): `TitleBar` (`GoTitleBar`), `LeftSideBar` (`GoSideBar`), `RightSideBar` (`GoSideBar`), `Footer` (`GoFooter`)
- Note: GoDesign also has a `<Theme>` child — handled as P1-style attribute set on a `<Theme>` child element (see Task 6).

---

## Bonus B2. External file reference
- Source: properties marked with secondary attribute `[GudxExternalRef("File")]` (introduced in Task 11)
- XML: `<Image Name="logo" File="resources/logo.png"/>`, `<GoPageRef File="Pages/Dashboard.gudx"/>`
- Cases:
  - `GoDesign.Images` (`private Dictionary<string, List<SKImage>>`) — image assets loaded from external files
  - `GoDesign.Fonts` (`private Dictionary<string, List<byte[]>>`) — font assets loaded from external files
  - Master file `<GoPageRef>`/`<GoWindowRef>` entries (introduced in Task 12)
- Note: `Images` and `Fonts` are declared `private` on `GoDesign`; the Gudx converter will need reflection or a dedicated accessor to reach them.

---

## Coverage matrix

| Class | Child-bearing property | Type | Pattern |
|---|---|---|---|
| `GoBoxPanel` | `Childrens` | `List<IGoControl>` | P2 |
| `GoGroupBox` | `Childrens` | `List<IGoControl>` | P2 |
| `GoPanel` | `Childrens` | `List<IGoControl>` | P2 |
| `GoScrollablePanel` | `Childrens` | `List<IGoControl>` | P2 |
| `GoScalePanel` | `Childrens` | `List<IGoControl>` | P2 |
| `GoPicturePanel` | `Childrens` | `List<IGoControl>` | P2 |
| `GoTableLayoutPanel` | `Childrens` | `GoTableLayoutControlCollection` | P3 |
| `GoTableLayoutPanel` | `Columns` / `Rows` | `List<string>` | P1 (serialized as delimited attribute) |
| `GoGridLayoutPanel` | `Childrens` | `GoGridLayoutControlCollection` | P3 |
| `GoGridLayoutPanel` | `Rows` | `List<GoGridLayoutPanelRow>` | P4b |
| `GoGridLayoutPanelRow` | `Height` / `Columns` | scalar / `List<string>` | P1 (no child controls) |
| `GoSwitchPanel` | `Pages` | `List<GoSubPage>` | P4a |
| `GoSubPage` | `Childrens` | `List<IGoControl>` | P2 (recursive inside P4a) |
| `GoTabControl` | `TabPages` | `List<GoTabPage>` | P4c |
| `GoTabPage` | `Childrens` | `List<IGoControl>` | P2 (recursive inside P4c) |
| `GoPage` | `Childrens` | `List<IGoControl>` | P2 |
| `GoWindow` | `Childrens` | `List<IGoControl>` | P2 |
| `GoDropDownWindow` | `Childrens` | `List<IGoControl>` | P2 |
| `GoTitleBar` | `Childrens` | `List<IGoControl>` | P2 |
| `GoSideBar` | `Childrens` | `List<IGoControl>` | P2 |
| `GoFooter` | `Childrens` | `List<IGoControl>` | P2 |
| `GoDesign` | `Pages` | `Dictionary<string, GoPage>` | P5 |
| `GoDesign` | `Windows` | `Dictionary<string, GoWindow>` | P5 |
| `GoDesign` | `TitleBar` | `GoTitleBar` (single) | B1 |
| `GoDesign` | `LeftSideBar` | `GoSideBar` (single) | B1 |
| `GoDesign` | `RightSideBar` | `GoSideBar` (single) | B1 |
| `GoDesign` | `Footer` | `GoFooter` (single) | B1 |
| `GoDesign` | `Images` | `private Dictionary<string, List<SKImage>>` | B2 |
| `GoDesign` | `Fonts` | `private Dictionary<string, List<byte[]>>` | B2 |

---

## Unmapped (G1 risk)

### 1. `GoGridLayoutPanel` — dual child-bearing properties (P3 + P4 interlock)

`GoGridLayoutPanel` is unique in having **two** child-bearing structures that must be serialized
together and kept in sync:

- `Rows : List<GoGridLayoutPanelRow>` (P4) — defines row heights and per-row column widths
- `Childrens : GoGridLayoutControlCollection` (P3) — holds the actual controls with `(col, row)` index

Unlike `GoTableLayoutPanel` (where `Columns`/`Rows` are simple `List<string>` P1 attributes),
here `Rows` is a *wrapper list* whose entries each carry their own `Columns` definition. The row
structure and the control collection are two separate properties that must round-trip consistently.

**Risk:** the converter needs a combined emit strategy for `GoGridLayoutPanel` — it cannot treat
`Rows` (P4) and `Childrens` (P3) independently without cross-referencing them. Suggested approach:
emit `<GoGridLayoutPanelRow>` wrappers in document order (from `Rows`), and for each row emit its
child controls (filtered from `GoGridLayoutControlCollection.Indexes` by row index) as nested
elements with a `Col="c"` attribute. This is a local extension of P3+P4 but does not fit either
pattern in isolation.

### 2. `GoSwitchPanel.Childrens` — computed proxy (not a real [GoChilds] target)

`GoSwitchPanel.Childrens` is declared `[JsonIgnore] public override IEnumerable<IGoControl> Childrens`
and returns `SelectedPage?.Childrens ?? []` — it is a runtime view, not the authoritative storage.
The real storage is `Pages : List<GoSubPage>` (P4). The converter must target `Pages`, not `Childrens`,
on `GoSwitchPanel`. Same applies to `GoTabControl.Childrens` (delegates to `SelectedTab?.Childrens`);
the real storage is `TabPages` (P4).

**Risk:** reflection walking `GoContainer.Childrens` as the canonical property name will hit the
proxy on `GoSwitchPanel` and `GoTabControl`. The `[GoChilds]` attribute must be placed on `Pages`
/ `TabPages`, not on the inherited `Childrens` override, to avoid double-serialization.

### 3. `GoDesign.Images` / `GoDesign.Fonts` — private properties

Both are declared `private` with `[JsonInclude]` (required for System.Text.Json). Standard
reflection (`BindingFlags.Public`) will not find them. The B2 pattern works conceptually but
the converter must use `BindingFlags.NonPublic | BindingFlags.Instance` or a dedicated accessor
method to reach these properties.

**Risk:** low severity (B2 is introduced in Task 11), but the access modifier mismatch with the
rest of the `[GoChilds]` contract should be documented before Task 11 implementation begins.
