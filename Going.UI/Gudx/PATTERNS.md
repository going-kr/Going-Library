# Gudx Patterns

This document is the design reference for the `GoGudxConverter` reflection algorithm
(Tasks 4-12 + R1-R5 refactor). The converter dispatches via attribute type — each
property's marker attribute determines which pattern handler runs. Inclusion is explicit:
no marker = no emission.

## Attribute family

The converter recognizes two attribute families:

**Scalar attributes** (P1):
- `[GoProperty]` (and derivatives `[GoSizeProperty]`, `[GoSizesProperty]`,
  `[GoImageProperty]`, `[GoFontNameProperty]`, `[GoMultiLineProperty]`)
- Property emits as XML attribute on the owning element. PascalCase.

**Child markers** (P2-P5, B1, B2): all derive from `GoChildAttribute` (abstract base).
- `[GoChildList]`     — homogeneous list of `IGoControl` (P2)
- `[GoChildCells]`    — cell-indexed collection (P3)
- `[GoChildWrappers]` — list of non-`IGoControl` wrappers (P4)
- `[GoChildMap]`      — keyed dictionary (P5)
- `[GoChildSingle]`   — single non-collection child (B1)
- `[GoChildResource]` — external file reference (B2, Task 11)

Multi-property classes (e.g., `GoGridLayoutPanel` has both `Rows` and `Childrens`)
are supported — every `[GoChild*]` property dispatches independently. Use
`[GoChildCells(NestInto = nameof(WrapperProp))]` to nest cells inside their
matching wrapper element.

## P1. Attribute property
- Source: scalar property marked with `[GoProperty]` family + scalar type
  (`bool|int|float|string|enum|SKColor|SKRect|GoPadding|List<string>`)
- XML: PascalCase attribute on the owning element
- Example: `<GoButton Text="Hello" FontSize="14" Bounds="0,0,100,40" Margin="4,4,4,4"/>`
- `[JsonIgnore]`-marked properties are skipped (excludes runtime aliases like X/Y/Width/Height
  which are derived from Bounds).
- Properties without `[GoProperty]` family are NOT emitted (no opt-out attribute needed).

## P2. `[GoChildList]` — homogeneous control list
- Source: `[GoChildList] List<T>` where `T : IGoControl`
- XML: child elements directly under owner, IGoControl class name as tag
- Containers: `GoBoxPanel.Childrens`, `GoGroupBox.Childrens`, `GoPanel.Childrens`,
  `GoScrollablePanel.Childrens`, `GoScalePanel.Childrens`, `GoPicturePanel.Childrens`,
  `GoPage.Childrens`, `GoWindow.Childrens`, `GoSubPage.Childrens`, `GoTabPage.Childrens`,
  `GoTitleBar.Childrens`, `GoSideBar.Childrens`, `GoFooter.Childrens`
- When parent uses P3 (cell-indexed), the child element receives a `Cell="c,r"` attribute
  emitted by P3.

## P3. `[GoChildCells]` — cell-indexed collection
- Source: `[GoChildCells] GoTableLayoutControlCollection` or
  `[GoChildCells] GoGridLayoutControlCollection`
- XML: child elements under owner with `Cell="col,row"` attribute (or
  `Cell="col,row,colSpan,rowSpan"` if non-default spans)
- Containers: `GoTableLayoutPanel.Childrens`, `GoGridLayoutPanel.Childrens`
- `Columns` / `Rows` (string list) emit as P1 attributes (`Columns="50%,50%"` `Rows="100%"`)
- ColSpan/RowSpan: 4-part Cell when non-default; 2-part otherwise.
- **NestInto**: when `[GoChildCells(NestInto = nameof(WrapperProp))]` is set, children are
  grouped by `Row` index and nested inside the matching `<WrapperType>` element from the
  named `[GoChildWrappers]` property. The wrapper property's default emission is suppressed
  (no double-emit).

## P4. `[GoChildWrappers]` — wrapper list
- Source: `[GoChildWrappers] List<TWrapper>` where `TWrapper` is NOT `IGoControl`
- XML: each wrapper appears as a child element, using the wrapper class name as tag.
  Wrapper's own `[GoChild*]` properties recurse.
- Cases:
  - `GoSwitchPanel.Pages : List<GoSubPage>` → `<GoSubPage Name="...">...</GoSubPage>`
  - `GoGridLayoutPanel.Rows : List<GoGridLayoutPanelRow>` → `<GoGridLayoutPanelRow Height="..." Columns="...">...</GoGridLayoutPanelRow>`
  - `GoTabControl.TabPages : List<GoTabPage>` → `<GoTabPage Name="...">...</GoTabPage>`
- Tag name = wrapper class name (no manual registry needed).

## P5. `[GoChildMap]` — keyed dictionary
- Source: `[GoChildMap] Dictionary<string, T>`
- XML: each entry as a child element using `T` class name as tag, with `Name="key"` attribute.
- Cases: `GoDesign.Pages`, `GoDesign.Windows`
- Read uses **tag-filtered enumeration** — multiple `[GoChildMap]` properties on the same
  parent (`GoDesign` has both Pages + Windows) route correctly by element tag name.

## B1. `[GoChildSingle]` — single child object
- Source: `[GoChildSingle] T` where `T` is a non-collection class (not primitive, not enum,
  not collection, not `SKColor`/`SKRect`/`GoPadding`)
- XML: one child element using **property name** as default tag (disambiguates same-typed
  siblings like `LeftSideBar` / `RightSideBar` both `GoSideBar` type). Use
  `[GudxTagName("...")]` alongside to override the tag.
- Cases (`GoDesign`):
  - `TitleBar : GoTitleBar` → `<TitleBar .../>`
  - `LeftSideBar : GoSideBar` → `<LeftSideBar .../>`
  - `RightSideBar : GoSideBar` → `<RightSideBar .../>`
  - `Footer : GoFooter` → `<Footer .../>`
  - `CustomTheme : GoTheme?` (nullable) with `[GudxTagName("Theme")]` → `<Theme .../>`
- Read: get-only properties (default-initialized) are populated in-place via `PopulateAny`;
  null nullable properties are constructed via `Activator.CreateInstance` then assigned
  via reflection (private setters supported, throws if no setter).

## B2. `[GoChildResource]` — external file reference (Task 11)
- Source: `[GoChildResource("TagName", Folder = "...", Ext = "...")]` on `Dictionary<string, T>`
  or list/single ref types
- XML: each entry emits as `<TagName Name="..." File="..."/>` — the asset is NOT inlined
  (no base64). Folder/Ext hint where files live relative to the master `.gudx`.
- Cases (planned for Task 11):
  - `GoDesign.Images` → `<Resources><Image Name="logo" File="resources/logo.png"/></Resources>`
  - `GoDesign.Fonts` → `<Resources><Font Name="Pretendard" File="resources/fonts/Pretendard.ttf"/></Resources>`
  - Master file `<GoPageRef File="Pages/...gudx"/>` and `<GoWindowRef File="Windows/...gudx"/>`
- B2 dispatch is currently a no-op (Task 11 will wire emission/parse).

## Inclusion vs exclusion

**Scalar inclusion (P1)** is via `[GoProperty]` family presence:
```csharp
.Where(p => p.GetCustomAttribute<GoPropertyAttribute>(inherit: true) != null)
.Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
.Where(p => p.CanRead && p.CanWrite)
.Where(p => IsScalar(p.PropertyType));
```

**Child inclusion (P2-P5/B1/B2)** is via `[GoChildAttribute]`-derived presence:
```csharp
.Where(p => p.IsDefined(typeof(GoChildAttribute), inherit: true));
```

No exclusion attributes (no `[GudxIgnore]`, `[JsonIgnore]` only filters scalars).

## Not Serialized (system-internal)

Some `IGoControl`-implementing types are registered in `GoJsonConverter.ControlTypes`
but never appear in user-authored `.gudx` files:

| Type | Why |
|------|-----|
| `GoDropDownWindow` | Runtime-only popup, held in private field `GoDesign.dropdownWindow` |
| `GoDialogs.SystemWindows.*` | System dialogs, registered globally at `Init()` |

These types are auto-excluded because:
- They're not in the user's `Pages`/`Windows` dictionary
- The fields holding them are `private`/`[JsonIgnore]`
- They have no `[GoChild*]` markings on parent properties pointing at them

If a hand-written `.gudx` ever contains such a tag, the converter will deserialize it
defensively but it's not the normal path.

## Coverage matrix

| Class | Property | Type | Pattern |
|-------|----------|------|---------|
| GoBoxPanel | Childrens | List<IGoControl> | P2 |
| GoGroupBox | Childrens | List<IGoControl> | P2 |
| GoPanel | Childrens | List<IGoControl> | P2 |
| GoScrollablePanel | Childrens | List<IGoControl> | P2 |
| GoScalePanel | Childrens | List<IGoControl> | P2 |
| GoPicturePanel | Childrens | List<IGoControl> | P2 |
| GoTableLayoutPanel | Childrens | GoTableLayoutControlCollection | P3 |
| GoSwitchPanel | Pages | List<GoSubPage> | P4 |
|   - GoSubPage (inner) | Childrens | List<IGoControl> | P2 |
| GoGridLayoutPanel | Rows | List<GoGridLayoutPanelRow> | P4 |
| GoGridLayoutPanel | Childrens | GoGridLayoutControlCollection | P3 (NestInto = "Rows") |
|   - GoGridLayoutPanelRow (inner) | (scalars only) | — | (no children) |
| GoTabControl | TabPages | List<GoTabPage> | P4 |
|   - GoTabPage (inner) | Childrens | List<IGoControl> | P2 |
| GoPage | Childrens | List<IGoControl> | P2 |
| GoWindow | Childrens | List<IGoControl> | P2 |
| GoTitleBar | Childrens | List<IGoControl> | P2 |
| GoSideBar | Childrens | List<IGoControl> | P2 |
| GoFooter | Childrens | List<IGoControl> | P2 |
| GoDesign | Pages | Dictionary<string, GoPage> | P5 |
| GoDesign | Windows | Dictionary<string, GoWindow> | P5 |
| GoDesign | TitleBar | GoTitleBar | B1 |
| GoDesign | LeftSideBar | GoSideBar | B1 |
| GoDesign | RightSideBar | GoSideBar | B1 |
| GoDesign | Footer | GoFooter | B1 |
| GoDesign | CustomTheme | GoTheme? | B1 (tag = "Theme") |
| GoDesign | Images | Dictionary<string, List<SKImage>> | B2 (Task 11) |
| GoDesign | Fonts | Dictionary<string, List<byte[]>> | B2 (Task 11) |

## Maintenance contract

When a new container/wrapper class is added to Going.UI:
1. Add the appropriate `[GoChild*]` attribute on its child-bearing property
2. Add a row to the Coverage matrix above
3. If the property has no existing pattern fit, escalate — do not invent a new dispatch path
   without reviewing this catalog

When a new pattern is genuinely needed:
1. Add a new `GoChildXxxAttribute : GoChildAttribute` derivative
2. Add a dispatch branch in `DispatchChildWrite` / `DispatchChildRead`
3. Add a regression test
4. Document the new pattern here with example
