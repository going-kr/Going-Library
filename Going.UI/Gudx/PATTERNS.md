# Gudx Patterns (v1.2.1+)

This document is the design reference for the `GoGudxConverter` reflection algorithm.
The converter dispatches via attribute type — each property's marker attribute determines
which pattern handler runs. Inclusion is explicit: no marker = no emission.

## Format change in v1.2.1

All collection patterns (P2/P3/P4/P5/B2) now emit children inside a **property-name group
element** (XAML / .NET XmlSerializer style). v1.2.0 emitted P4 wrappers as direct siblings
of P2 children, which caused a silent corruption (ReadP2 unfiltered walk consumed P4
elements) — the v1.2.1 grouping naturally separates collections.

```xml
<!-- v1.2.0 (deprecated) — P4 wrappers + P2 children mixed at parent level -->
<GoSparkline>
  <GoBaseline .../>
  <GoLineGraphSeries .../>
</GoSparkline>

<!-- v1.2.1 — every collection in its own property-name group element -->
<GoSparkline>
  <Baselines>
    <GoBaseline .../>
  </Baselines>
  <Series>
    <GoLineGraphSeries .../>
  </Series>
</GoSparkline>
```

## Attribute family

The converter recognizes two attribute families:

**Scalar attributes** (P1):
- `[GoProperty]` (and derivatives `[GoSizeProperty]`, `[GoSizesProperty]`,
  `[GoImageProperty]`, `[GoFontNameProperty]`, `[GoMultiLineProperty]`)
- Property emits as XML attribute on the owning element. PascalCase.

**Child markers** (P2-P5, B1, B2): all derive from `GoChildAttribute` (abstract base).
- `[GoChildList]`     — homogeneous list of `IGoControl` (P2, group + flat children)
- `[GoChildCells]`    — cell-indexed collection (P3, group + flat children + attached `Cell`)
- `[GoChildWrappers]` — list of non-`IGoControl` wrappers (P4, group + wrapper-tagged entries)
- `[GoChildMap]`      — keyed dictionary (P5, group + value-tagged entries with `Name` attr)
- `[GoChildSingle]`   — single non-collection child (B1, property name = tag)
- `[GoChildResource]` — external file reference (B2, group + `[GoChildResource(TagName, Folder, Ext)]`-driven entries)

A class may declare multiple `[GoChild*]` properties; the converter walks each in turn.
To find all child markers on a type: use `prop.IsDefined(typeof(GoChildAttribute), inherit: true)`.

`ChildProperties` walks `BindingFlags.Public | BindingFlags.NonPublic` to discover private
`[GoChildResource]`-marked properties (e.g. `GoDesign.Images`, `GoDesign.Fonts`).

## P1. Attribute property
- Source: scalar property marked with `[GoProperty]` family + scalar type
  (`bool|int|float|string|enum|Guid|TimeSpan|SKColor|SKRect|GoPadding|List<string>|Nullable<T>` + 11 numeric types)
- XML: PascalCase attribute on the owning element
- Example: `<GoButton Text="Hello" FontSize="14" Bounds="0,0,100,40" Margin="4,4,4,4"/>`
- `[JsonIgnore]`-marked properties are skipped (excludes runtime aliases like X/Y/Width/Height
  which are derived from Bounds).
- Properties without `[GoProperty]` family are NOT emitted (no opt-out attribute needed).

## P2. `[GoChildList]` — homogeneous control list
- Source: `[GoChildList] List<T>` where `T : IGoControl`
- XML: `<PropertyName>` group element with IGoControl class-name child entries
- Empty list = group element omitted entirely
- Read is **tag-filtered via `_gudxControlTypes` registry** — only registered IGoControl tags
  are processed (defensive against malformed input)
- Containers: `GoBoxPanel.Childrens`, `GoGroupBox.Childrens`, `GoPanel.Childrens`,
  `GoScrollablePanel.Childrens`, `GoScalePanel.Childrens`, `GoPicturePanel.Childrens`,
  `GoPage.Childrens`, `GoWindow.Childrens`, `GoSubPage.Childrens`, `GoTabPage.Childrens`,
  `GoTitleBar.Childrens`, `GoSideBar.Childrens`, `GoFooter.Childrens`

## P3. `[GoChildCells]` — cell-indexed collection
- Source: `[GoChildCells]` on a property whose type implements
  `IGoCellIndexedControlCollection` (`Going.UI.Collections`).
  Currently: `GoTableLayoutControlCollection`, `GoGridLayoutControlCollection`.
- XML: `<PropertyName>` (= `<Childrens>`) group element with IGoControl class-name
  child entries each carrying a `Cell="col,row"` attribute (or `Cell="col,row,colSpan,rowSpan"`
  if non-default spans).
- `Columns` / `Rows` (string list) emit as P1 attributes (`Columns="50%,50%"` `Rows="100%"`)
- ColSpan/RowSpan: 4-part `Cell` when non-default; 2-part otherwise.
- v1.2.1 unified dispatch (no NestInto special case — see migration note below).
- Containers: `GoTableLayoutPanel.Childrens`, `GoGridLayoutPanel.Childrens`

### Migration: NestInto removed in v1.2.1

v1.2.0 had an optional `[GoChildCells(NestInto = nameof(WrapperProp))]` that nested cells
inside `[GoChildWrappers]` rows by `Row` index — used only by `GoGridLayoutPanel`. v1.2.1
adopts XAML-style flat emission: `Rows` is an independent P4 metadata collection, and
cell positions are pure attached `Cell` attributes.

```xml
<!-- v1.2.0 (deprecated) — NestInto nested cells inside row wrappers -->
<GoGridLayoutPanel>
  <GoGridLayoutPanelRow Height="50%" Columns="50%,50%">
    <GoButton Cell="0,0" .../>
    <GoLabel  Cell="1,0" .../>
  </GoGridLayoutPanelRow>
</GoGridLayoutPanel>

<!-- v1.2.1 — Rows metadata + Childrens flat with attached Cell -->
<GoGridLayoutPanel>
  <Rows>
    <GoGridLayoutPanelRow Height="50%" Columns="50%,50%" />
  </Rows>
  <Childrens>
    <GoButton Cell="0,0" .../>
    <GoLabel  Cell="1,0" .../>
  </Childrens>
</GoGridLayoutPanel>
```

## P4. `[GoChildWrappers]` — wrapper list
- Source: `[GoChildWrappers] List<TWrapper>` (or `ObservableList<TWrapper>`) where
  `TWrapper` is NOT `IGoControl`
- XML: `<PropertyName>` group element with each wrapper as a child entry, using the wrapper
  class name as tag. The wrapper's own `[GoChild*]` properties recurse.
- Empty list = group element omitted entirely
- Cases:
  - `GoSwitchPanel.Pages : List<GoSubPage>` → `<Pages><GoSubPage Name="...">...</GoSubPage></Pages>`
  - `GoGridLayoutPanel.Rows : List<GoGridLayoutPanelRow>` → `<Rows><GoGridLayoutPanelRow Height="..." Columns="..."/></Rows>`
  - `GoTabControl.TabPages : List<GoTabPage>` → `<TabPages><GoTabPage Name="...">...</GoTabPage></TabPages>`
- Tag name = wrapper class name (no manual registry needed).
- Polymorphic read: derived concrete types in the base wrapper's assembly are auto-discovered
  (e.g. `GoDataGridColumn` → `GoDataGridLabelColumn`/`GoDataGridButtonColumn`/...).

## P5. `[GoChildMap]` — keyed dictionary
- Source: `[GoChildMap] Dictionary<string, T>`
- XML: `<PropertyName>` group element with each entry as `<T Name="key" .../>` (T = value class name).
- Empty dict = group element omitted entirely
- The group element naturally separates multiple `[GoChildMap]` properties on the same
  parent (e.g. `GoDesign.Pages` + `GoDesign.Windows`) — v1.2.0's tag-filtered read workaround
  is no longer needed.

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

## B2. `[GoChildResource]` — external file reference
- Source: `[GoChildResource("TagName", Folder = "...", Ext = "...")]` on `Dictionary<string, T>`
- XML: `<PropertyName>` group element with each entry as `<TagName Name="..." File="Folder/key.Ext"/>`.
  The byte payload itself is NOT inlined (no base64) — written to disk under the resolved
  File path by `SerializeGoDesignToFiles` / read back by `DeserializeGoDesignFromFiles`.
- Empty dict = group element omitted entirely
- The dispatch (`WriteB2_Resource` / `ReadB2_Resource`) handles the XML side; external file
  I/O is performed by `ExtractGoChildResourceFiles` / `LoadGoChildResourceFiles` helpers
  (reflection-driven, attribute-aware).
- Cases:
  - `GoDesign.Images : Dictionary<string, List<SKImage>>` →
    `<Images><Image Name="logo" File="resources/logo.png"/></Images>` + `resources/logo.png` on disk
  - `GoDesign.Fonts : Dictionary<string, List<byte[]>>` →
    `<Fonts><Font Name="Pretendard" File="resources/fonts/Pretendard.ttf"/></Fonts>` + TTF on disk
- Master split: `<Pages>` / `<Windows>` group elements contain `<GoPageRef File="..."/>` /
  `<GoWindowRef File="..."/>` entries (separate `.gudx` files). Built directly by
  `SerializeGoDesignToFiles` (Pages/Windows are skipped from dispatch via `skipChildren`).

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
Walked across `Public | NonPublic` instance properties (private members like `GoDesign.Images`
are discoverable).

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
| GoBoxPanel | Childrens | List&lt;IGoControl&gt; | P2 |
| GoGroupBox | Childrens | List&lt;IGoControl&gt; | P2 |
| GoGroupBox | Buttons | List&lt;GoButtonItem&gt; | P4 |
| GoPanel | Childrens | List&lt;IGoControl&gt; | P2 |
| GoPanel | Buttons | List&lt;GoButtonItem&gt; | P4 |
| GoScrollablePanel | Childrens | List&lt;IGoControl&gt; | P2 |
| GoScalePanel | Childrens | List&lt;IGoControl&gt; | P2 |
| GoPicturePanel | Childrens | List&lt;IGoControl&gt; | P2 |
| GoTableLayoutPanel | Childrens | GoTableLayoutControlCollection | P3 |
| GoSwitchPanel | Pages | List&lt;GoSubPage&gt; | P4 |
|   - GoSubPage (inner) | Childrens | List&lt;IGoControl&gt; | P2 |
| GoGridLayoutPanel | Rows | List&lt;GoGridLayoutPanelRow&gt; | P4 |
| GoGridLayoutPanel | Childrens | GoGridLayoutControlCollection | P3 |
| GoTabControl | TabPages | List&lt;GoTabPage&gt; | P4 |
|   - GoTabPage (inner) | Childrens | List&lt;IGoControl&gt; | P2 |
| GoPage | Childrens | List&lt;IGoControl&gt; | P2 |
| GoWindow | Childrens | List&lt;IGoControl&gt; | P2 |
| GoTitleBar | Childrens | List&lt;IGoControl&gt; | P2 |
| GoSideBar | Childrens | List&lt;IGoControl&gt; | P2 |
| GoFooter | Childrens | List&lt;IGoControl&gt; | P2 |
| GoDataGrid | Columns | ObservableList&lt;GoDataGridColumn&gt; | P4 |
| GoBarGraph / GoCircleGraph / GoLineGraph / GoSparkline / GoTimeGraph / GoTrendGraph | Series | List&lt;GoLineGraphSeries / GoGraphSeries&gt; | P4 |
| GoSparkline | Baselines | List&lt;GoBaseline&gt; | P4 |
| GoListBox / GoInput | Items | ObservableList / List&lt;GoListItem&gt; | P4 |
| GoNavigator | Menus | ObservableList&lt;GoMenuItem&gt; | P4 |
| GoButtons / GoInput / GoValue | Buttons | List&lt;GoButtonItem&gt; | P4 |
| GoToolBox | Categories | ObservableList&lt;GoToolCategory&gt; | P4 |
| GoTreeView / GoTreeNode | Nodes | ObservableList&lt;GoTreeNode&gt; (recursive) | P4 |
| GoStateLamp | States | List&lt;StateLamp&gt; | P4 |
| GoStepBar | Steps | ObservableList&lt;GoStepItem&gt; | P4 |
| GoDesign | Pages | Dictionary&lt;string, GoPage&gt; | P5 |
| GoDesign | Windows | Dictionary&lt;string, GoWindow&gt; | P5 |
| GoDesign | TitleBar | GoTitleBar | B1 |
| GoDesign | LeftSideBar | GoSideBar | B1 |
| GoDesign | RightSideBar | GoSideBar | B1 |
| GoDesign | Footer | GoFooter | B1 |
| GoDesign | CustomTheme | GoTheme? | B1 (tag = "Theme") |
| GoDesign | Images (private) | Dictionary&lt;string, List&lt;SKImage&gt;&gt; | B2 |
| GoDesign | Fonts (private) | Dictionary&lt;string, List&lt;byte[]&gt;&gt; | B2 |

## Maintenance contract

When a new container/wrapper class is added to Going.UI:
1. Add the appropriate `[GoChild*]` attribute on its child-bearing property
2. Add a row to the Coverage matrix above
3. If the property has no existing pattern fit, escalate — do not invent a new dispatch path
   without reviewing this catalog

When a new cell-indexed container is added (P3):
1. Implement `IGoCellIndexedControlCollection` on the collection type (`EnumerateCells`,
   `AddCell`, `Controls`)
2. Mark the container's child property with `[GoChildCells]`
3. No converter code change needed — dispatch picks it up via the interface

When a new resource type is added (B2):
1. Add the property with `[GoChildResource("TagName", Folder = "...", Ext = "...")]`
2. Add a `Add<TagName>(name, byte[] data)` helper on the owning class for read population
3. Extend `LoadGoChildResourceFiles` with a branch dispatching to that helper
4. Polymorphic byte payload (`SKImage`/`byte[]`/...) handled by `ExtractGoChildResourceFiles` —
   add a branch if the payload type differs from these two

When a new pattern is genuinely needed:
1. Add a new `GoChildXxxAttribute : GoChildAttribute` derivative
2. Add a dispatch branch in `DispatchChildWrite` / `DispatchChildRead`
3. Add a regression test
4. Document the new pattern here with example
