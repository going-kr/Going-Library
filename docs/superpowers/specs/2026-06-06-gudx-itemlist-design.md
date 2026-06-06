# GoItemList + ItemTemplate (컬렉션 템플릿 바인딩)

**작성일**: 2026-06-06
**브랜치**: `hjh-gudx-binding`
**대상**: Going.UI (Gudx 마크업 + 바인딩 + 컴포넌트 레이어)
**선행**: [선언적 바인딩](2026-06-06-gudx-declarative-binding-design.md), [컴포넌트](2026-06-06-gudx-components-design.md)

---

## 목표

컬렉션을 바인딩하고, 각 아이템을 템플릿으로 반복 렌더한다(WPF ItemsControl/Repeater):

```xml
<GoItemList Items="{Logs}" ItemHeight="28">
  <ItemTemplate>
    <GoBoxPanel><Childrens>
      <GoLabel Text="{Message}"/>
      <GoLabel Text="{Time:HH:mm}"/>
    </Childrens></GoBoxPanel>
  </ItemTemplate>
</GoItemList>
```

`Logs`(예: `ObservableList<LogVM>`)의 아이템마다 ItemTemplate을 복제하고, 각 행의 `{Message}`/`{Time}`을 **그 아이템 객체**에 바인딩한다.

## 비목표 (1차)

- diff 기반 부분 갱신(1차는 변경 시 전량 재빌드)
- 가변 행 높이(1차는 `ItemHeight` 고정)
- 명명 컴포넌트를 ItemTemplate으로 참조(`ItemTemplate="MotorCard"`) — 후속
- 선택/가상화/그룹핑/드래그 정렬
- 문자열 보간

---

## 핵심: 바인딩·템플릿·스크롤은 전부 재사용

| 필요한 것 | 출처 |
|---|---|
| ItemTemplate 복제 | 컴포넌트 템플릿 복제(`ReadElement`) |
| 행마다 `{member}` 바인딩 | **평면 `Wire`, root = 아이템 객체** (선언적 바인딩) |
| 행 트리 전체 wire | `GudxBinder.WireTree(row, item)` 그대로 |
| 매 프레임 동기화 | 기존 펌프 |
| 컬렉션 변경 감지 | `ObservableList<T>.Changed` dirty-bit (이미 존재) |
| 세로 스택 + 스크롤 | `GoScrollablePanel` **상속** |

**진짜 새로 만드는 건**: ① 컬렉션 바인딩 전달 ② 변경 감지 → 행 재생성 ③ ItemTemplate read/write. 레이아웃/스크롤은 상속.

## 아이템 타입은 런타임 추론 — `type` 선언 불필요

컴포넌트는 `<Param type="MotorVM">`로 타입을 선언했지만, ItemList는 **바인딩된 컬렉션 원소 타입을 런타임에 안다**. 행 wire 시 `item.GetType()` 기준으로 `{Message}`를 컴파일(기존 `CompilePath` 타입별 캐시). 선언 0.

---

## 결정 사항

### D1. `GoItemList : GoScrollablePanel`
스크롤·자식 클리핑·ViewPosition을 상속. 행은 `Childrens`에 생성된 컨트롤. content height = `ItemHeight * Count`.

### D2. `Items` 컬렉션 전달 = 일반 스칼라 Wire 재사용
`Items="{Logs}"`는 선언적 바인딩 read 훅이 `PendingBindings["Items"]`로 저장 → 펌프가 `GoItemList.ItemsSource`(IEnumerable 프로퍼티)에 **컬렉션 참조**를 매 프레임 set. 별도 컬렉션-바인딩 엔진 없음.

- `ItemsSource`는 `[JsonIgnore]` writable `IEnumerable?` (스칼라 [GoProperty] 아님 → 직렬화는 PendingBindings 라운드트립으로).

### D3. 변경 감지 = `Changed` dirty-bit 우선 + count/identity 폴백
`GoItemList.OnUpdate`에서:
- `ItemsSource`가 `ObservableList<T>`면 `.Changed` 플래그 확인(소비 후 리셋).
- 아니면 `(reference, Count)` 스냅샷 비교.
- 변하면 행 재생성.

> `ObservableList.Changed`는 내부 `bool`이라 제네릭 T 없이 접근하려면 비제네릭 인터페이스/리플렉션이 필요. 1차는 `Count` + 참조 폴링을 기본으로 하고, `Changed`는 `IGoObservable`(신규 비제네릭 인터페이스, `bool Changed { get; set; }`)를 `ObservableList<T>`에 구현해 활용. (구현 단계 확정)

### D4. 재생성 = 전량 재빌드
변경 시: 기존 `Childrens` 비우고(바인딩 해제), 아이템마다 ItemTemplate 복제 → `Bounds = (0, i*ItemHeight, Width, ItemHeight)` → `GudxBinder.WireTree(row, item)` → `Childrens.Add(row)`. 그 후 `FireInit`로 행 초기화.

### D5. ItemTemplate = inline, 특수 read/write
`<ItemTemplate>`는 [GoChild*]가 아닌 특수 자식. `GoItemList`만 read/write를 특수 처리:
- **read**: `PopulateAny` 후(또는 `ReadElement` 분기) `<ItemTemplate>`의 첫 컨트롤 요소를 `ItemTemplateXml`(XElement)로 보관.
- **write**: `<GoItemList>` 출력 시 `<ItemTemplate>` 그룹 + 원본 템플릿 XElement 재출력.

### D6. PendingBinding 라운드트립 일반화
`Items="{Logs}"`는 스칼라가 아니라 현재 `WriteAny`의 스칼라 라운드트립 루프가 안 잡는다. → **스칼라 루프 후, 미출력 PendingBindings를 일괄 출력**하도록 일반화(Items 등 비스칼라 바인딩도 보존). 선언적 바인딩에도 이로운 개선.

---

## 아키텍처: 신규/수정

| 파일 | 변경 |
|---|---|
| `Going.UI/Controls/GoItemList.cs` | **신규** — `GoScrollablePanel` 파생. ItemsSource/ItemHeight/ItemTemplateXml + 변경감지·행재생성 |
| `Going.UI/Collections/ObservableList.cs` | `IGoObservable`(비제네릭 `Changed`) 구현(선택, D3) |
| `Going.UI/Collections/IGoObservable.cs` | **신규**(선택) — `bool Changed { get; set; }` |
| `Going.UI/Gudx/GoGudxConverter.cs` | GoItemList ItemTemplate read/write 특수 처리; PendingBinding 라운드트립 일반화(D6) |

재사용(무수정): `GudxBindingExpression.Wire`/`CompilePath`, `GudxBinder.WireTree`, 펌프, `GoScrollablePanel` 스크롤.

> `GoItemList`는 `_gudxControlTypes`에 자동 등록(IGoControl 구현). 사용처 태그 `<GoItemList>`가 정상 역직렬화됨.

---

## 데이터 흐름

```
역직렬화:
  <GoItemList Items="{Logs}" ItemHeight="28"><ItemTemplate>...</ItemTemplate></GoItemList>
    → GoItemList 생성, ItemHeight 스칼라 set
    → Items="{Logs}" → PendingBindings["Items"]
    → <ItemTemplate> 첫 컨트롤 → ItemTemplateXml 보관

WireBindings(root):
  WireTree(page, root) → GoItemList의 PendingBindings["Items"]
    → Wire(itemList, "ItemsSource", "{Logs}", root)  // 컬렉션 참조 전달(스칼라 Wire)

런타임(OnUpdate):
  ItemsSource 변경 감지(Changed/Count)
    → Childrens 재생성: item마다
        row = ReadElement(ItemTemplateXml)         // 복제, {member}는 PendingBindings
        row.Bounds = (0, i*ItemHeight, Width, ItemHeight)
        GudxBinder.WireTree(row, item)             // root = item, 평면 Wire
        Childrens.Add(row); row.FireInit(Design)
    → 각 행은 기존 펌프로 자기 item 멤버 표시
```

---

## 테스트 전략

- **ItemTemplate read**: `<GoItemList><ItemTemplate>...` → ItemTemplateXml 보관, Items→PendingBindings.
- **행 생성**: ItemsSource에 3개 아이템 set → OnUpdate(또는 직접 Rebuild) → Childrens 3개, 각 Bounds y=index*ItemHeight.
- **행 바인딩**: 각 행의 `{Message}`가 해당 아이템 값 표시(아이템별 다름).
- **변경 반영**: 아이템 추가 → 다음 OnUpdate에 행 추가; 제거 → 행 감소.
- **라운드트립**: `<GoItemList Items="{Logs}"><ItemTemplate>...` 재출력(D6 + D5).
- **타입 추론**: 선언 없이 `item.GetType()` 기준 컴파일.

---

## 열린 결정 (유저 리뷰)

1. **변경 감지 깊이**: `Count`+참조 폴링만(1차) vs `ObservableList.Changed` 비제네릭 인터페이스까지(정확). 권장: 폴링 기본 + 가능하면 Changed.
2. **빈 컬렉션/null Items**: 행 0개(빈 리스트) 처리 — 기본 동작으로 충분.

---

## 결정 요약

| 항목 | 결정 |
|---|---|
| 베이스 | `GoItemList : GoScrollablePanel`(스크롤 상속) |
| Items 전달 | 선언적 바인딩 스칼라 Wire가 ItemsSource(IEnumerable)에 참조 전달 |
| 변경 감지 | Count+참조 폴링 기본, ObservableList.Changed 가능 시 활용 |
| 재생성 | 변경 시 전량 재빌드 |
| 행 바인딩 | `GudxBinder.WireTree(row, item)` — root=아이템, 평면 Wire 재사용 |
| 아이템 타입 | 런타임 `item.GetType()` 추론, 선언 불필요 |
| ItemTemplate | inline 특수 read/write, XElement 보관·복제 |
| 라운드트립 | 비스칼라 PendingBinding 일반 출력(D6) + ItemTemplate 재출력 |
| 엔진 | 바인딩·펌프·스크롤 전부 재사용 |
