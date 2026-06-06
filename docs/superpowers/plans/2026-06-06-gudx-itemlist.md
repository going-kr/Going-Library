# GoItemList + ItemTemplate Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans. Steps use `- [ ]`.

**Goal:** `<GoItemList Items="{Logs}"><ItemTemplate>...</ItemTemplate></GoItemList>` — 바인딩 컬렉션을 아이템마다 템플릿으로 반복 렌더, 각 행은 아이템 객체에 바인딩.

**Architecture:** `GoItemList : GoScrollablePanel`(스크롤 상속). `Items="{Logs}"`는 평면 바인딩 스칼라 Wire가 `ItemsSource`(IEnumerable)에 컬렉션 참조를 전달. `OnUpdate`에서 변경 감지(Changed/Count) → 행 재생성: ItemTemplate 복제 + `GudxBinder.WireTree(row, item)`(root=아이템). 엔진·펌프·스크롤 재사용.

**Spec:** [2026-06-06-gudx-itemlist-design.md](../specs/2026-06-06-gudx-itemlist-design.md)

---

## File Structure

- `Going.UI/Collections/IGoObservable.cs` — **신규**. 비제네릭 `bool Changed`.
- `Going.UI/Collections/ObservableList.cs` — `IGoObservable` 구현(인터페이스만 추가, Changed 기존).
- `Going.UI/Controls/GoItemList.cs` — **신규**. `GoScrollablePanel` 파생.
- `Going.UI/Gudx/GoGudxConverter.cs` — ItemTemplate read/write 특수 + PendingBinding 일반 라운드트립(D6).
- 테스트: `Going.UI.Tests/Controls/GoItemListTests.cs`.

---

## Task 1: `IGoObservable` + ObservableList 구현

**Files:** Create `IGoObservable.cs`; Modify `ObservableList.cs`; Test `GoItemListTests.cs`.

- [ ] **Step 1: 실패 테스트**

```csharp
using Going.UI.Collections;
using Xunit;

namespace Going.UI.Tests.Controls;

public class GoItemListTests
{
    [Fact]
    public void ObservableList_ImplementsIGoObservable_ChangedFlag()
    {
        var list = new ObservableList<int>();
        IGoObservable obs = list;          // 비제네릭 접근
        list.Add(1);
        Assert.True(obs.Changed);
        obs.Changed = false;
        Assert.False(list.Changed);
    }
}
```

- [ ] **Step 2: 실패 확인** — `dotnet test Going.UI.Tests --filter GoItemListTests` → FAIL.

- [ ] **Step 3: 구현**

`IGoObservable.cs`:
```csharp
namespace Going.UI.Collections;

/// <summary>컬렉션 변경 dirty-bit를 제네릭 인자 없이 노출.</summary>
public interface IGoObservable
{
    /// <summary>마지막 소비 이후 변경되었는지. 소비자가 false로 리셋.</summary>
    bool Changed { get; set; }
}
```

`ObservableList.cs` 선언부: `public class ObservableList<T> : IList<T>` → `public class ObservableList<T> : IList<T>, IGoObservable`. (기존 `public bool Changed { get; set; }`가 인터페이스 충족.)

- [ ] **Step 4: 통과 확인** — PASS.
- [ ] **Step 5: 커밋** — `"feat(itemlist): IGoObservable on ObservableList"`

---

## Task 2: `GoItemList` 골격 + 행 재생성

**Files:** Create `Going.UI/Controls/GoItemList.cs`; Test `GoItemListTests.cs`.

- [ ] **Step 1: 실패 테스트**

```csharp
// GoItemListTests.cs 에 추가
using System.Xml.Linq;
using Going.UI.Bindings;
using Going.UI.Controls;
using Going.UI.Gudx;
using Going.UI.Tools;

public sealed class LogVM { public string Message { get; set; } = ""; public int N { get; set; } }

[Fact]
public void Rebuild_GeneratesRow_PerItem_WithBounds()
{
    var il = new GoItemList { ItemHeight = 20 };
    il.Bounds = Util.FromRect(0, 0, 200, 100);
    il.ItemTemplateXml = XElement.Parse("<GoBoxPanel><Childrens><GoLabel Text='{Message}'/></Childrens></GoBoxPanel>");
    var data = new System.Collections.Generic.List<LogVM>
    {
        new() { Message = "a" }, new() { Message = "b" }, new() { Message = "c" },
    };
    il.ItemsSource = data;

    il.RebuildRows();   // 테스트용 직접 호출(OnUpdate가 호출)

    Assert.Equal(3, il.Childrens.Count);
    Assert.Equal(0, il.Childrens[0].Bounds.Top);
    Assert.Equal(20, il.Childrens[1].Bounds.Top);
    Assert.Equal(40, il.Childrens[2].Bounds.Top);
}

[Fact]
public void Rebuild_RowBindsToItem()
{
    var il = new GoItemList { ItemHeight = 20 };
    il.Bounds = Util.FromRect(0, 0, 200, 100);
    il.ItemTemplateXml = XElement.Parse("<GoBoxPanel><Childrens><GoLabel Text='{Message}'/></Childrens></GoBoxPanel>");
    il.ItemsSource = new System.Collections.Generic.List<LogVM> { new() { Message = "hello" } };

    il.RebuildRows();

    var box = (GoBoxPanel)il.Childrens[0];
    var lbl = (GoLabel)box.Childrens[0];
    lbl.FireUpdate();
    Assert.Equal("hello", lbl.Text);
}
```

- [ ] **Step 2: 실패 확인** — FAIL(타입/RebuildRows 없음).

- [ ] **Step 3: 구현** — `GoItemList.cs`

```csharp
using System.Collections;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Going.UI.Bindings;
using Going.UI.Collections;
using Going.UI.Containers;
using Going.UI.Design;
using Going.UI.Enums;
using Going.UI.Gudx;
using Going.UI.Tools;

namespace Going.UI.Controls;

/// <summary>바인딩된 컬렉션을 ItemTemplate으로 반복 렌더하는 리스트. 각 행은 아이템 객체에 바인딩된다.</summary>
public class GoItemList : GoScrollablePanel
{
    /// <summary>각 행의 높이.</summary>
    [GoProperty(PCategory.Control, 0)] public float ItemHeight { get; set; } = 30;

    /// <summary>바인딩으로 전달되는 컬렉션 참조(<c>Items="{Logs}"</c>). 펌프가 매 프레임 set.</summary>
    [JsonIgnore] public IEnumerable? ItemsSource { get; set; }

    /// <summary>ItemTemplate 원본(행 복제 소스).</summary>
    [JsonIgnore] public XElement? ItemTemplateXml { get; set; }

    private object? lastSource;
    private int lastCount = -1;

    /// <inheritdoc/>
    protected override void OnUpdate()
    {
        if (NeedsRebuild()) RebuildRows();
        base.OnUpdate();   // 자식(행) 펌프
    }

    private bool NeedsRebuild()
    {
        var src = ItemsSource;
        if (!ReferenceEquals(src, lastSource)) return true;
        if (src == null) return false;
        if (src is IGoObservable obs && obs.Changed) return true;
        return CountOf(src) != lastCount;
    }

    /// <summary>현재 ItemsSource로 행을 전량 재생성한다.</summary>
    public void RebuildRows()
    {
        foreach (var c in Childrens) if (c is GoControl gc) gc.UnbindAll();
        Childrens.Clear();

        var src = ItemsSource;
        lastSource = src;
        if (src == null) { lastCount = 0; return; }

        int i = 0;
        foreach (var item in src)
        {
            if (ItemTemplateXml != null && item != null && GoGudxConverter.ReadElement(ItemTemplateXml) is GoControl row)
            {
                row.Bounds = Util.FromRect(0, i * ItemHeight, Width, ItemHeight);
                row.Parent = this;
                Childrens.Add(row);
                GudxBinder.WireTree(row, item);
                row.FireInit(Design);
            }
            i++;
        }
        lastCount = i;
        if (src is IGoObservable obs) obs.Changed = false;
    }

    private static int CountOf(IEnumerable src)
        => src is ICollection col ? col.Count : src.Cast<object>().Count();
}
```

> `GoGudxConverter.ReadElement`는 internal(동일 어셈블리 접근 OK). `row.Parent`/`Bounds`는 GoControl public. Step 3 전 `ReadElement` 접근성·`Parent` setter 확인(이미 `SetParent`/internal set 존재).

- [ ] **Step 4: 통과 확인** — PASS.
- [ ] **Step 5: 커밋** — `"feat(itemlist): GoItemList row regeneration + per-item wire"`

---

## Task 3: ItemTemplate read + Items 라운드트립(D5/D6)

**Files:** Modify `Going.UI/Gudx/GoGudxConverter.cs`; Test `GoItemListTests.cs`.

- [ ] **Step 1: 실패 테스트**

```csharp
// GoItemListTests.cs 에 추가
[Fact]
public void Deserialize_CapturesItemTemplate_AndItemsBinding()
{
    var xml = "<GoItemList ItemHeight='28'><ItemTemplate><GoBoxPanel><Childrens><GoLabel Text='{Message}'/></Childrens></GoBoxPanel></ItemTemplate></GoItemList>";
    var il = (GoItemList)GoGudxConverter.ReadElement(XElement.Parse(xml.Replace("<GoItemList ItemHeight='28'>", "<GoItemList ItemHeight='28' Items='{Logs}'>")));

    Assert.Equal(28, il.ItemHeight);
    Assert.NotNull(il.ItemTemplateXml);
    Assert.Equal("GoBoxPanel", il.ItemTemplateXml!.Name.LocalName);
    Assert.Equal("{Logs}", il.PendingBindings!["Items"]);   // 컬렉션 바인딩 보류
}

[Fact]
public void RoundTrip_ItemList_PreservesItemsAndTemplate()
{
    var il = (GoItemList)GoGudxConverter.ReadElement(XElement.Parse(
        "<GoItemList ItemHeight='28' Items='{Logs}'><ItemTemplate><GoLabel Text='{Message}'/></ItemTemplate></GoItemList>"));
    var e = GoGudxConverter.WriteAny(il);

    Assert.Equal("{Logs}", e.Attribute("Items")?.Value);            // D6: 비스칼라 바인딩 보존
    var tmpl = e.Element("ItemTemplate");
    Assert.NotNull(tmpl);
    Assert.Equal("GoLabel", tmpl!.Elements().First().Name.LocalName);
    Assert.Null(e.Element("Childrens"));                            // 생성 행 미출력
}
```

> GoItemList에 `Items`라는 writable 프로퍼티가 있어야 read 훅이 PendingBindings에 저장. `ItemsSource`를 쓰되, 마크업 attribute 이름은 `Items`. → GoItemList에 writable `Items` 프로퍼티(=ItemsSource 별칭)도 필요하거나, read/write에서 `Items`↔`ItemsSource` 매핑. Step 3에서 처리(아래).

- [ ] **Step 2: 실패 확인** — FAIL.

- [ ] **Step 3: 구현**

(a) GoItemList에 마크업 이름 `Items`를 위한 writable 별칭 추가(`ItemsSource`와 동일 백킹):

```csharp
    /// <summary>마크업 attribute 이름(<c>Items</c>)용 별칭. 바인딩 전용.</summary>
    [JsonIgnore] public IEnumerable? Items { get => ItemsSource; set => ItemsSource = value; }
```

> read 훅이 `Items="{Logs}"`를 PendingBindings["Items"]로 저장하고, Wire가 `Items` setter→ItemsSource로 전달. 일관.

(b) `ReadElement`에서 ItemTemplate 캡처(PopulateAny 직후, Id 처리 앞):

```csharp
        var instance = (IGoControl)Activator.CreateInstance(type)!;
        PopulateAny(elem, instance);

        if (instance is Going.UI.Controls.GoItemList il)
        {
            var tmpl = elem.Element("ItemTemplate");
            il.ItemTemplateXml = tmpl?.Elements().FirstOrDefault();
        }
        // ... 기존 Id 처리 ...
```

(c) `WriteAny`: GoItemList는 생성 Childrens 생략 + ItemTemplate 출력 + 비스칼라 PendingBinding 출력(D6).

스칼라 루프 뒤(자식 루프 앞)에 D6 일반 출력 추가:
```csharp
        // 비스칼라 PendingBinding(Items 등)도 라운드트립
        if (obj is Going.UI.Controls.GoControl gctrl && gctrl.PendingBindings is { } allPend)
            foreach (var kv in allPend)
                if (elem.Attribute(kv.Key) == null) elem.SetAttributeValue(kv.Key, kv.Value);
```

자식 루프에서 GoItemList의 Childrens(생성 행) 생략 + ItemTemplate 출력:
```csharp
        var localSkip = skipChildren;
        if (obj is Going.UI.Controls.GoItemList) { localSkip = new HashSet<string>(skipChildren ?? System.Linq.Enumerable.Empty<string>(), StringComparer.Ordinal) { "Childrens" }; }
        foreach (var childProp in ChildProperties(type))
        {
            if (localSkip != null && localSkip.Contains(childProp.Name)) continue;
            ...
        }
        if (obj is Going.UI.Controls.GoItemList ilw && ilw.ItemTemplateXml != null)
            elem.Add(new XElement("ItemTemplate", new XElement(ilw.ItemTemplateXml)));
```

> 정확한 삽입 위치(스칼라 루프/자식 루프 경계, `return elem` 앞)는 Step 3 전 WriteAny 현재 코드 확인 후 반영.

- [ ] **Step 4: 통과 확인** — PASS.
- [ ] **Step 5: 커밋** — `"feat(itemlist): ItemTemplate read/write + non-scalar binding round-trip"`

---

## Task 4: 변경 반영 통합 + 전체 회귀 + CHANGELOG

**Files:** Test `GoItemListTests.cs`; `CHANGELOG.md`.

- [ ] **Step 1: 변경 반영 테스트**

```csharp
// GoItemListTests.cs 에 추가
[Fact]
public void Update_AddItem_AddsRow()
{
    var il = new GoItemList { ItemHeight = 20 };
    il.Bounds = Util.FromRect(0, 0, 200, 100);
    il.ItemTemplateXml = XElement.Parse("<GoLabel Text='{Message}'/>");
    var data = new ObservableList<LogVM> { new() { Message = "a" } };
    il.ItemsSource = data;

    il.RebuildRows();
    Assert.Single(il.Childrens);

    data.Add(new LogVM { Message = "b" });
    il.FireUpdate();   // OnUpdate가 Changed 감지 → 재생성
    Assert.Equal(2, il.Childrens.Count);
}
```

- [ ] **Step 2: 실패 확인 → 통과** — RebuildRows/OnUpdate 경로로 PASS.

- [ ] **Step 3: 전체 테스트** — `dotnet test Going.UI.Tests` → 전부 PASS.

- [ ] **Step 4: CHANGELOG**

```markdown
- **GoItemList + ItemTemplate** — 바인딩 컬렉션(`Items="{Logs}"`)을 ItemTemplate으로 반복 렌더. 각 행은 아이템 객체에 평면 바인딩(`{Message}` 등, 타입 런타임 추론). `GoScrollablePanel` 상속으로 스크롤, `ObservableList.Changed`/Count로 변경 감지 후 행 재생성.
```

- [ ] **Step 5: 커밋** — `"feat(itemlist): change detection + CHANGELOG"`

---

## Self-Review 메모

- **Spec 커버리지**: D1 base(T2) / D2 Items전달(T2,T3) / D3 변경감지(T1,T2,T4) / D4 재빌드(T2) / D5 ItemTemplate(T3) / D6 라운드트립(T3) — 1:1.
- **타입 일관성**: `GoItemList.{ItemHeight,ItemsSource,Items,ItemTemplateXml,RebuildRows}`, `IGoObservable.Changed` 전 Task 동일.
- **재사용**: `GudxBinder.WireTree`/평면 `Wire`/펌프/`GoScrollablePanel` 무수정.
- **검증 가드**: `ReadElement` 접근성, `GoControl.Parent` setter, `WriteAny` 스칼라/자식 경계, `OnUpdate` override 시그니처는 Step 3 전 확인.
